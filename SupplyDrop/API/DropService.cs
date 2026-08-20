using System;
using System.Collections.Generic;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using MEC;
using Mirror;
using UnityEngine;
using ToyLight = Exiled.API.Features.Toys.Light;
using ExiledCassie = Exiled.API.Features.Cassie;
using ExiledRound = Exiled.API.Features.Round;

namespace SupplyDrop.API
{
    public sealed class DropService
    {
        private readonly Config config;
        private readonly Dictionary<string, DropState> states = new Dictionary<string, DropState>();
        private readonly List<CoroutineHandle> pending = new List<CoroutineHandle>(8);

        private CoroutineHandle scheduler;
        private bool running;

        public DropService(Config config) => this.config = config;

        public void Start()
        {
            Stop();

            states.Clear();

            foreach (DropProfile profile in config.Profiles)
            {
                if (profile is null || !profile.IsEnabled || string.IsNullOrEmpty(profile.Key))
                    continue;

                states[profile.Key] = new DropState { NextDropAt = profile.FirstDropSeconds };
            }

            if (states.Count == 0)
                return;

            running = true;
            scheduler = Timing.RunCoroutine(Schedule(), Segment.FixedUpdate);
        }

        public void Stop()
        {
            if (running)
                Timing.KillCoroutines(scheduler);

            running = false;

            foreach (CoroutineHandle handle in pending)
                Timing.KillCoroutines(handle);

            pending.Clear();
            states.Clear();
        }

        public DropProfile Find(string key)
        {
            foreach (DropProfile profile in config.Profiles)
            {
                if (profile is not null && string.Equals(profile.Key, key, StringComparison.OrdinalIgnoreCase))
                    return profile;
            }

            return null;
        }

        public void Trigger(DropProfile profile, bool announce)
        {
            if (profile is null)
                return;

            if (!announce)
            {
                Deliver(profile);
                return;
            }

            Announce(profile);

            if (profile.AnnouncementLeadSeconds <= 0f)
            {
                Deliver(profile);
                return;
            }

            pending.Add(Timing.CallDelayed(profile.AnnouncementLeadSeconds, () => Deliver(profile)));
        }

        private IEnumerator<float> Schedule()
        {
            float tick = Mathf.Max(0.5f, config.SchedulerTick);

            while (running)
            {
                yield return Timing.WaitForSeconds(tick);

                if (!ExiledRound.IsStarted || ExiledRound.IsEnded)
                    continue;

                float elapsed = (float)ExiledRound.ElapsedTime.TotalSeconds;

                foreach (DropProfile profile in config.Profiles)
                {
                    if (profile is null || !profile.IsEnabled)
                        continue;

                    if (!states.TryGetValue(profile.Key, out DropState state))
                        continue;

                    if (elapsed < state.NextDropAt)
                        continue;

                    if (profile.MaxDropsPerRound > 0 && state.DropsUsed >= profile.MaxDropsPerRound)
                        continue;

                    if (Player.List.Count < config.MinPlayers)
                    {
                        state.NextDropAt = elapsed + config.RetrySeconds;
                        continue;
                    }

                    state.DropsUsed++;
                    state.NextDropAt = elapsed + profile.AnnouncementLeadSeconds + profile.IntervalSeconds;

                    Trigger(profile, true);
                }
            }
        }

        private void Announce(DropProfile profile)
        {
            try
            {
                if (!string.IsNullOrEmpty(profile.Broadcast))
                    Map.Broadcast(profile.BroadcastDuration, profile.Broadcast);

                if (string.IsNullOrEmpty(profile.CassieAnnouncement))
                    return;

                if (string.IsNullOrEmpty(profile.CassieSubtitles))
                    ExiledCassie.Message(profile.CassieAnnouncement, false, false, false);
                else
                    ExiledCassie.MessageTranslated(profile.CassieAnnouncement, profile.CassieSubtitles, false, false, false);
            }
            catch (Exception e)
            {
                Log.Error($"DropService.Announce: {e}");
            }
        }

        private void Deliver(DropProfile profile)
        {
            try
            {
                if (!ExiledRound.IsStarted || ExiledRound.IsEnded)
                    return;

                if (profile.Items is null || profile.Items.Count == 0)
                {
                    Log.Warn($"Le profil de largage \"{profile.Key}\" ne contient aucun objet.");
                    return;
                }

                Vector3 fallback = ResolveFallback(profile);
                int spawned = 0;
                int attempted = 0;

                foreach (DropItem entry in profile.Items)
                {
                    if (entry is null || entry.Item == ItemType.None || entry.Quantity <= 0)
                        continue;

                    Vector3 origin = ResolvePosition(profile, entry.Item, fallback);
                    int chance = Mathf.Clamp(entry.Chance, 0, 100);

                    for (int i = 0; i < entry.Quantity; i++)
                    {
                        if (spawned >= config.MaxItemsPerDrop)
                        {
                            Log.Warn($"Largage \"{profile.Key}\" tronque a {config.MaxItemsPerDrop} objets (max_items_per_drop).");
                            goto done;
                        }

                        attempted++;

                        if (UnityEngine.Random.Range(0, 100) >= chance)
                            continue;

                        Pickup.CreateAndSpawn(entry.Item, Scatter(origin, profile.ScatterRadius), null);
                        spawned++;
                    }
                }

                done:

                PlaceBeacon(profile, fallback);

                if (config.LogDrops)
                    Log.Info($"[SupplyDrop] Largage \"{profile.Key}\" : {spawned}/{attempted} objets places.");
            }
            catch (Exception e)
            {
                Log.Error($"DropService.Deliver: {e}");
            }
        }

        private static Vector3 ResolvePosition(DropProfile profile, ItemType type, Vector3 fallback)
        {
            if (type.IsAmmo() && profile.AmmoPosition != Vector3.zero)
                return profile.AmmoPosition;

            if (type.IsArmor() && profile.ArmorPosition != Vector3.zero)
                return profile.ArmorPosition;

            if ((type.IsWeapon(true) || type.IsThrowable()) && profile.WeaponPosition != Vector3.zero)
                return profile.WeaponPosition;

            if ((type.IsKeycard() || type.IsMedical() || type.IsUtility() || type.IsScp())
                && profile.ItemPosition != Vector3.zero)
            {
                return profile.ItemPosition;
            }

            return fallback;
        }

        private static Vector3 ResolveFallback(DropProfile profile)
        {
            SpawnLocation location = profile.FallbackRole.GetRandomSpawnLocation();

            return location is null ? Vector3.zero : location.Position;
        }

        private static Vector3 Scatter(Vector3 origin, float radius)
        {
            if (radius <= 0f)
                return origin;

            Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;

            return new Vector3(origin.x + offset.x, origin.y, origin.z + offset.y);
        }

        private void PlaceBeacon(DropProfile profile, Vector3 fallback)
        {
            if (!profile.EnableBeacon)
                return;

            Vector3 position = profile.ItemPosition != Vector3.zero ? profile.ItemPosition : fallback;

            if (position == Vector3.zero)
                return;

            if (!ColorUtility.TryParseHtmlString(profile.BeaconColor, out Color color))
                color = Color.white;

            ToyLight beacon = ToyLight.Create(position + (Vector3.up * 1.5f));

            if (beacon?.Base is null)
                return;

            beacon.Color = color;
            beacon.Intensity = 3f;
            beacon.Range = 12f;
            beacon.ShadowType = LightShadows.None;

            pending.Add(Timing.CallDelayed(Mathf.Max(1f, profile.BeaconDuration), () =>
            {
                if (beacon?.Base is null || beacon.Base.gameObject is null)
                    return;

                NetworkServer.Destroy(beacon.Base.gameObject);
            }));
        }
    }
}

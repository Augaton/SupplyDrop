using System;
using System.Collections.Generic;
using Exiled.API.Enums;
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
        private readonly List<string> customNames = new List<string>(48);
        private readonly List<int> customWeights = new List<int>(48);
        private readonly List<string> discovered = new List<string>(48);
        private readonly HashSet<string> unresolvedCustomItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private bool warnedEmptyRegistry;

        private CoroutineHandle scheduler;
        private bool running;

        public DropService(Config config) => this.config = config;

        public void Start()
        {
            Stop();

            states.Clear();
            unresolvedCustomItems.Clear();
            warnedEmptyRegistry = false;
            CustomItemBridge.Reset();

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
            customNames.Clear();
            customWeights.Clear();
            discovered.Clear();
            unresolvedCustomItems.Clear();
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

        public void Trigger(DropProfile profile, bool announce, Vector3? forcedPosition = null)
        {
            if (profile is null)
                return;

            if (!announce)
            {
                Deliver(profile, forcedPosition);
                return;
            }

            Announce(profile);

            if (profile.AnnouncementLeadSeconds <= 0f)
            {
                Deliver(profile, forcedPosition);
                return;
            }

            pending.Add(Timing.CallDelayed(profile.AnnouncementLeadSeconds, () => Deliver(profile, forcedPosition)));
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

        private void Deliver(DropProfile profile, Vector3? forcedPosition)
        {
            try
            {
                if (!ExiledRound.IsStarted || ExiledRound.IsEnded)
                    return;

                bool hasItems = profile.Items is not null && profile.Items.Count > 0;
                bool hasCustomItems = profile.CustomItems is not null
                    && profile.CustomItems.IsEnabled
                    && profile.CustomItems.Draws > 0;

                if (!hasItems && !hasCustomItems)
                {
                    Log.Warn($"Le profil de largage \"{profile.Key}\" ne contient aucun objet.");
                    return;
                }

                Vector3 anchor = forcedPosition ?? ResolveAnchor(profile);

                if (anchor == Vector3.zero)
                {
                    Log.Error(
                        $"Largage \"{profile.Key}\" annule : aucune position exploitable. " +
                        $"Le role de repli {profile.FallbackRole} n'expose aucun point d'apparition et aucune " +
                        "coordonnee n'est configuree dans le profil.");
                    return;
                }

                Vector3 lift = Vector3.up * config.SpawnHeightOffset;
                int spawned = 0;
                int attempted = 0;

                if (hasItems)
                {
                    foreach (DropItem entry in profile.Items)
                    {
                        if (entry is null || entry.Item == ItemType.None || entry.Quantity <= 0)
                            continue;

                        Vector3 origin = forcedPosition ?? ResolvePosition(profile, entry.Item, anchor);
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

                            Pickup.CreateAndSpawn(entry.Item, Scatter(origin, profile.ScatterRadius) + lift, null);
                            spawned++;
                        }
                    }
                }

                done:

                Vector3 customOrigin = forcedPosition ?? ResolvePosition(profile, ItemType.SCP500, anchor);
                int custom = DeliverCustomItems(profile, customOrigin, lift);

                PlaceBeacon(profile, forcedPosition ?? ResolveBeaconPosition(profile, anchor));

                if (config.LogDrops)
                {
                    Log.Info(
                        $"[SupplyDrop] Largage \"{profile.Key}\" : {spawned}/{attempted} objets places " +
                        $"et {custom} objet(s) personnalise(s), autour de {Describe(anchor)}.");
                }
            }
            catch (Exception e)
            {
                Log.Error($"DropService.Deliver: {e}");
            }
        }

        private int DeliverCustomItems(DropProfile profile, Vector3 origin, Vector3 lift)
        {
            CustomDrop custom = profile.CustomItems;

            if (custom is null || !custom.IsEnabled || custom.Draws <= 0)
                return 0;

            if (!CustomItemBridge.Available)
            {
                if (config.Debug)
                    Log.Debug($"Exiled.CustomItems absent, les objets personnalises du profil \"{profile.Key}\" sont ignores.");

                return 0;
            }

            BuildCandidates(profile, custom);

            if (customNames.Count == 0)
            {
                if (custom.Source != CustomDropSource.Pool && !warnedEmptyRegistry)
                {
                    warnedEmptyRegistry = true;
                    Log.Warn(
                        $"Aucun objet personnalise ne correspond au filtre du profil \"{profile.Key}\". " +
                        "Verifier base_items et excluded, ou que le plugin proprietaire est bien charge.");
                }

                return 0;
            }

            int chance = Mathf.Clamp(custom.Chance, 0, 100);
            int draws = custom.AllowDuplicates ? custom.Draws : Mathf.Min(custom.Draws, customNames.Count);
            int placed = 0;

            for (int i = 0; i < draws; i++)
            {
                if (UnityEngine.Random.Range(0, 100) >= chance)
                    continue;

                int index = PickWeightedIndex();

                if (index < 0)
                    break;

                string reference = customNames[index];

                if (!custom.AllowDuplicates)
                {
                    customNames.RemoveAt(index);
                    customWeights.RemoveAt(index);
                }

                if (CustomItemBridge.Spawn(reference, Scatter(origin, profile.ScatterRadius) + lift))
                    placed++;
            }

            customNames.Clear();
            customWeights.Clear();
            return placed;
        }

        private void BuildCandidates(DropProfile profile, CustomDrop custom)
        {
            customNames.Clear();
            customWeights.Clear();

            if (custom.Source != CustomDropSource.Registered && custom.Pool is not null)
            {
                foreach (CustomDropItem entry in custom.Pool)
                {
                    if (entry is null || entry.Weight <= 0 || string.IsNullOrEmpty(entry.Reference))
                        continue;

                    if (!CustomItemBridge.Exists(entry.Reference))
                    {
                        if (unresolvedCustomItems.Add(entry.Reference))
                            Log.Warn($"Objet personnalise \"{entry.Reference}\" introuvable, il est retire du pool du profil \"{profile.Key}\".");

                        continue;
                    }

                    Append(entry.Reference, entry.Weight);
                }
            }

            if (custom.Source == CustomDropSource.Pool)
                return;

            int weight = Mathf.Max(1, custom.DefaultWeight);

            discovered.Clear();
            CustomItemBridge.Collect(discovered, custom.BaseItems);

            foreach (string reference in discovered)
            {
                if (IsExcluded(custom, reference))
                    continue;

                Append(reference, weight);
            }

            discovered.Clear();
        }

        private void Append(string reference, int weight)
        {
            foreach (string existing in customNames)
            {
                if (string.Equals(existing, reference, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            customNames.Add(reference);
            customWeights.Add(weight);
        }

        private static bool IsExcluded(CustomDrop custom, string reference)
        {
            if (custom.Excluded is null || custom.Excluded.Count == 0)
                return false;

            foreach (string excluded in custom.Excluded)
            {
                if (string.Equals(excluded, reference, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private int PickWeightedIndex()
        {
            int total = 0;

            foreach (int weight in customWeights)
                total += weight;

            if (total <= 0)
                return -1;

            int roll = UnityEngine.Random.Range(0, total);

            for (int i = 0; i < customWeights.Count; i++)
            {
                roll -= customWeights[i];

                if (roll < 0)
                    return i;
            }

            return customWeights.Count - 1;
        }

        private static Vector3 ResolvePosition(DropProfile profile, ItemType type, Vector3 anchor)
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

            return anchor;
        }

        private static Vector3 ResolveBeaconPosition(DropProfile profile, Vector3 anchor)
            => profile.ItemPosition != Vector3.zero ? profile.ItemPosition : anchor;

        private static Vector3 ResolveAnchor(DropProfile profile)
        {
            SpawnLocation location = profile.FallbackRole.GetRandomSpawnLocation();

            if (location is not null && location.Position != Vector3.zero)
                return location.Position;

            Vector3 nuke = SpawnLocationType.InsideSurfaceNuke.GetPosition();

            if (nuke != Vector3.zero)
                return nuke;

            Room surface = Room.Get(RoomType.Surface);

            return surface is null ? Vector3.zero : surface.Position;
        }

        private static string Describe(Vector3 position)
            => $"{position.x:0.0} / {position.y:0.0} / {position.z:0.0}";

        private static Vector3 Scatter(Vector3 origin, float radius)
        {
            if (radius <= 0f)
                return origin;

            Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;

            return new Vector3(origin.x + offset.x, origin.y, origin.z + offset.y);
        }

        private void PlaceBeacon(DropProfile profile, Vector3 position)
        {
            if (!profile.EnableBeacon || position == Vector3.zero)
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

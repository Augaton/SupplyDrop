using System;
using System.Collections.Generic;
using Exiled.API.Features;
using SupplyDrop.API;
using SupplyDrop.Handlers;
using ServerEvents = Exiled.Events.Handlers.Server;

namespace SupplyDrop
{
    public sealed class Plugin : Plugin<Config, Translation>
    {
        public override string Name => "SupplyDrop";

        public override string Author => "Zone-Shilari (base: Wafel, KadeDev, JesusQC)";

        public override string Prefix => "supplydrop";

        public override Version Version => new Version(4, 3, 0);

        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        public static Plugin Instance { get; private set; }

        public DropService Service { get; private set; }

        private ServerHandlers serverHandlers;

        public override void OnEnabled()
        {
            Instance = this;

            ValidateConfig();

            Service = new DropService(Config);
            serverHandlers = new ServerHandlers(Service);

            ServerEvents.RoundStarted += serverHandlers.OnRoundStarted;
            ServerEvents.RoundEnded += serverHandlers.OnRoundEnded;
            ServerEvents.RestartingRound += serverHandlers.OnRestartingRound;
            ServerEvents.WaitingForPlayers += serverHandlers.OnWaitingForPlayers;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            ServerEvents.RoundStarted -= serverHandlers.OnRoundStarted;
            ServerEvents.RoundEnded -= serverHandlers.OnRoundEnded;
            ServerEvents.RestartingRound -= serverHandlers.OnRestartingRound;
            ServerEvents.WaitingForPlayers -= serverHandlers.OnWaitingForPlayers;

            Service?.Stop();

            serverHandlers = null;
            Service = null;
            Instance = null;

            base.OnDisabled();
        }

        private void ValidateConfig()
        {
            if (Config.MinPlayers < 0)
            {
                Log.Warn($"MinPlayers ({Config.MinPlayers}) negatif, remis a 0.");
                Config.MinPlayers = 0;
            }

            if (Config.RetrySeconds < 5f)
            {
                Log.Warn($"RetrySeconds ({Config.RetrySeconds}) trop bas, remis a 60.");
                Config.RetrySeconds = 60f;
            }

            if (Config.SchedulerTick < 0.5f)
            {
                Log.Warn($"SchedulerTick ({Config.SchedulerTick}) trop bas, remis a 1.");
                Config.SchedulerTick = 1f;
            }

            if (Config.MaxItemsPerDrop < 1)
            {
                Log.Warn($"MaxItemsPerDrop ({Config.MaxItemsPerDrop}) invalide, remis a 60.");
                Config.MaxItemsPerDrop = 60;
            }

            if (Config.SpawnHeightOffset < 0f || Config.SpawnHeightOffset > 10f)
            {
                Log.Warn($"SpawnHeightOffset ({Config.SpawnHeightOffset}) hors plage, remis a 0.5.");
                Config.SpawnHeightOffset = 0.5f;
            }

            if (Config.Profiles is null || Config.Profiles.Count == 0)
            {
                Log.Warn("Aucun profil de largage configure, le plugin ne fera rien.");
                return;
            }

            HashSet<string> keys = new HashSet<string>();

            foreach (DropProfile profile in Config.Profiles)
            {
                if (profile is null)
                    continue;

                if (string.IsNullOrEmpty(profile.Key))
                {
                    Log.Warn("Un profil de largage n'a pas de cle, il sera ignore.");
                    profile.IsEnabled = false;
                    continue;
                }

                if (!keys.Add(profile.Key))
                {
                    Log.Warn($"Cle de profil \"{profile.Key}\" en double, la seconde entree est desactivee.");
                    profile.IsEnabled = false;
                    continue;
                }

                if (profile.IntervalSeconds < 30f)
                {
                    Log.Warn($"IntervalSeconds du profil \"{profile.Key}\" ({profile.IntervalSeconds}) trop bas, remis a 30.");
                    profile.IntervalSeconds = 30f;
                }

                if (profile.FirstDropSeconds < 0f)
                {
                    Log.Warn($"FirstDropSeconds du profil \"{profile.Key}\" negatif, remis a 0.");
                    profile.FirstDropSeconds = 0f;
                }

                if (profile.AnnouncementLeadSeconds < 0f)
                {
                    Log.Warn($"AnnouncementLeadSeconds du profil \"{profile.Key}\" negatif, remis a 0.");
                    profile.AnnouncementLeadSeconds = 0f;
                }

                if (profile.ScatterRadius < 0f)
                    profile.ScatterRadius = 0f;

                ValidateCustomItems(profile);
            }
        }

        private static void ValidateCustomItems(DropProfile profile)
        {
            CustomDrop custom = profile.CustomItems;

            if (custom is null)
                return;

            if (custom.Draws < 0)
            {
                Log.Warn($"CustomItems.Draws du profil \"{profile.Key}\" negatif, remis a 0.");
                custom.Draws = 0;
            }

            if (custom.Draws > 10)
            {
                Log.Warn($"CustomItems.Draws du profil \"{profile.Key}\" ({custom.Draws}) trop eleve, plafonne a 10.");
                custom.Draws = 10;
            }

            if (custom.Chance < 0 || custom.Chance > 100)
            {
                Log.Warn($"CustomItems.Chance du profil \"{profile.Key}\" ({custom.Chance}) hors plage, remis a 100.");
                custom.Chance = 100;
            }

            if (custom.DefaultWeight < 1)
            {
                Log.Warn($"CustomItems.DefaultWeight du profil \"{profile.Key}\" ({custom.DefaultWeight}) invalide, remis a 1.");
                custom.DefaultWeight = 1;
            }

            if (custom.Source != CustomDropSource.Registered && (custom.Pool is null || custom.Pool.Count == 0))
            {
                Log.Warn(
                    $"CustomItems.Source du profil \"{profile.Key}\" vaut {custom.Source} mais le pool est vide. " +
                    "Passer la source a Registered pour un tirage entierement aleatoire.");
            }

            if (custom.Source != CustomDropSource.Pool && (custom.BaseItems is null || custom.BaseItems.Count == 0))
            {
                Log.Warn(
                    $"CustomItems.BaseItems du profil \"{profile.Key}\" est vide : la decouverte automatique " +
                    "retiendra tous les objets personnalises, pas uniquement les SCP-500.");
            }

            if (custom.Pool is null)
                return;

            foreach (CustomDropItem entry in custom.Pool)
            {
                if (entry is null)
                    continue;

                if (string.IsNullOrEmpty(entry.Reference))
                {
                    Log.Warn($"Une entree du pool d'objets personnalises du profil \"{profile.Key}\" n'a pas de reference, elle sera ignoree.");
                    entry.Weight = 0;
                    continue;
                }

                if (entry.Weight >= 0)
                    continue;

                Log.Warn($"Poids negatif pour \"{entry.Reference}\" dans le profil \"{profile.Key}\", remis a 0.");
                entry.Weight = 0;
            }
        }
    }
}

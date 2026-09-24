using System;
using System.Collections.Generic;
using System.Text;
using CommandSystem;
using Exiled.API.Features;
using NorthwoodLib.Pools;
using SupplyDrop.API;

namespace SupplyDrop.Commands
{
    public sealed class ListCommand : ICommand
    {
        public string Command => "list";

        public string[] Aliases => new[] { "l" };

        public string Description => "Liste les profils de largage configures.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                return Run(arguments, sender, out response);
            }
            catch (Exception e)
            {
                Log.Error($"supplydrop list: {e}");
                response = "Erreur interne, voir la console serveur.";
                return false;
            }
        }

        private bool Run(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!SupplyDropCommand.CheckAccess(sender, out string error))
            {
                response = error;
                return false;
            }

            Plugin plugin = Plugin.Instance;

            if (plugin.Config.Profiles is null || plugin.Config.Profiles.Count == 0)
            {
                response = plugin.Translation.NoProfile;
                return false;
            }

            StringBuilder builder = StringBuilderPool.Shared.Rent();

            try
            {
                builder.AppendLine("Profils de largage :");

                foreach (DropProfile profile in plugin.Config.Profiles)
                {
                    if (profile is null)
                        continue;

                    string state = profile.IsEnabled ? "actif" : "desactive";
                    int items = profile.Items?.Count ?? 0;
                    string limit = profile.MaxDropsPerRound > 0 ? profile.MaxDropsPerRound.ToString() : "illimite";

                    builder.AppendLine(
                        $"  {profile.Key}  [{state}]  premier a {profile.FirstDropSeconds:0}s, " +
                        $"puis toutes les {profile.IntervalSeconds:0}s, max {limit}, {items} entree(s) d'objets");

                    AppendCustomItems(builder, profile);
                }

                response = builder.ToString();
                return true;
            }
            finally
            {
                StringBuilderPool.Shared.Return(builder);
            }
        }

        private static void AppendCustomItems(StringBuilder builder, DropProfile profile)
        {
            CustomDrop custom = profile.CustomItems;

            if (custom is null)
                return;

            if (!custom.IsEnabled || custom.Draws <= 0)
            {
                builder.AppendLine("      objets personnalises : desactives");
                return;
            }

            if (!CustomItemBridge.Available)
            {
                builder.AppendLine("      objets personnalises : Exiled.CustomItems n'est pas charge, ignores");
                return;
            }

            int candidates = 0;

            if (custom.Source != CustomDropSource.Registered && custom.Pool is not null)
            {
                foreach (CustomDropItem entry in custom.Pool)
                {
                    if (entry is null || entry.Weight <= 0 || string.IsNullOrEmpty(entry.Reference))
                        continue;

                    if (CustomItemBridge.Exists(entry.Reference))
                        candidates++;
                    else
                        builder.AppendLine($"      reference introuvable : {entry.Reference}");
                }
            }

            if (custom.Source != CustomDropSource.Pool)
            {
                List<string> found = new List<string>(48);
                CustomItemBridge.Collect(found, custom.BaseItems);

                int excluded = 0;

                foreach (string reference in found)
                {
                    if (DropService.IsExcluded(custom, reference))
                        excluded++;
                }

                candidates += found.Count - excluded;

                builder.AppendLine(
                    $"      decouverte automatique : {found.Count - excluded} objet(s) retenu(s) " +
                    $"sur {found.Count} enregistre(s), {excluded} exclu(s)");
            }

            builder.AppendLine(
                $"      objets personnalises : source {custom.Source}, {custom.Draws} tirage(s) a {custom.Chance}%, " +
                $"{candidates} candidat(s)");
        }
    }
}

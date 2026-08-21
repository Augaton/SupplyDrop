using System;
using System.Text;
using CommandSystem;
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

            if (custom is null || custom.Pool is null || custom.Pool.Count == 0)
                return;

            if (!custom.IsEnabled)
            {
                builder.AppendLine("      objets personnalises : desactives");
                return;
            }

            if (!CustomItemBridge.Available)
            {
                builder.AppendLine("      objets personnalises : Exiled.CustomItems n'est pas charge, pool ignore");
                return;
            }

            int resolved = 0;

            foreach (CustomDropItem entry in custom.Pool)
            {
                if (entry is null || entry.Weight <= 0)
                    continue;

                if (CustomItemBridge.Exists(entry.Reference))
                    resolved++;
                else
                    builder.AppendLine($"      objet personnalise introuvable : {entry.Reference}");
            }

            builder.AppendLine(
                $"      objets personnalises : {custom.Draws} tirage(s) a {custom.Chance}%, " +
                $"{resolved}/{custom.Pool.Count} reference(s) resolue(s)");
        }
    }
}

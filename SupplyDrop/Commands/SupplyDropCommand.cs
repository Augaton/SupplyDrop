using System;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace SupplyDrop.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class SupplyDropCommand : ParentCommand
    {
        public SupplyDropCommand() => LoadGeneratedCommands();

        public override string Command => "supplydrop";

        public override string[] Aliases => new[] { "sd" };

        public override string Description => "Gestion des largages de ravitaillement.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new CallCommand());
            RegisterCommand(new ListCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Sous-commandes : call <profil> [silencieux], list.";
            return false;
        }

        internal static bool CheckAccess(ICommandSender sender, out string error)
        {
            error = null;

            if (Plugin.Instance is null)
            {
                error = "Plugin indisponible.";
                return false;
            }

            if (sender.CheckPermission("supplydrop.call"))
                return true;

            error = Plugin.Instance.Translation.NoPermission;
            return false;
        }
    }
}

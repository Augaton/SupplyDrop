using System;
using CommandSystem;
using Exiled.API.Features;
using SupplyDrop.API;
using ExiledRound = Exiled.API.Features.Round;

namespace SupplyDrop.Commands
{
    public sealed class CallCommand : ICommand
    {
        public string Command => "call";

        public string[] Aliases => new[] { "c" };

        public string Description => "Declenche un largage immediatement. Usage : supplydrop call <profil> [silencieux]";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!SupplyDropCommand.CheckAccess(sender, out string error))
            {
                response = error;
                return false;
            }

            Plugin plugin = Plugin.Instance;

            if (arguments.Count < 1)
            {
                response = "Usage : supplydrop call <profil> [silencieux]";
                return false;
            }

            if (!ExiledRound.IsStarted || ExiledRound.IsEnded)
            {
                response = plugin.Translation.RoundNotStarted;
                return false;
            }

            string key = arguments.At(0);

            if (key.Length > 64)
            {
                response = "Argument trop long.";
                return false;
            }

            DropProfile profile = plugin.Service.Find(key);

            if (profile is null)
            {
                response = plugin.Translation.UnknownProfile.Replace("%KEY%", key);
                return false;
            }

            bool announce = true;

            if (arguments.Count >= 2)
            {
                string mode = arguments.At(1).ToLowerInvariant();
                announce = mode != "silencieux" && mode != "silent" && mode != "quiet";
            }

            plugin.Service.Trigger(profile, announce);

            string author = Player.Get(sender) is Player player
                ? $"{player.Nickname} ({player.UserId})"
                : sender.LogName;

            Log.Info($"[SupplyDrop] {author} a declenche le largage \"{profile.Key}\" (annonce : {announce}).");

            response = plugin.Translation.DropCalled.Replace("%KEY%", profile.Key);
            return true;
        }
    }
}

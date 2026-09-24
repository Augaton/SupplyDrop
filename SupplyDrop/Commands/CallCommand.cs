using System;
using CommandSystem;
using Exiled.API.Features;
using SupplyDrop.API;
using UnityEngine;
using ExiledRound = Exiled.API.Features.Round;

namespace SupplyDrop.Commands
{
    public sealed class CallCommand : ICommand
    {
        public string Command => "call";

        public string[] Aliases => new[] { "c" };

        public string Description => "Declenche un largage immediatement. Usage : supplydrop call <profil> [silencieux] [ici]";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                return Run(arguments, sender, out response);
            }
            catch (Exception e)
            {
                Log.Error($"supplydrop call: {e}");
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

            if (arguments.Count < 1)
            {
                response = "Usage : supplydrop call <profil> [silencieux] [ici]";
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
            Vector3? forcedPosition = null;
            Player caller = Player.Get(sender);

            for (int i = 1; i < arguments.Count; i++)
            {
                string mode = arguments.At(i).ToLowerInvariant();

                if (mode == "silencieux" || mode == "silent" || mode == "quiet")
                {
                    announce = false;
                    continue;
                }

                if (mode != "ici" && mode != "here")
                    continue;

                if (caller is null)
                {
                    response = "Le mode \"ici\" n'est disponible que depuis un client en jeu.";
                    return false;
                }

                forcedPosition = caller.Position;
            }

            plugin.Service.Trigger(profile, announce, forcedPosition);

            string author = caller is not null
                ? $"{caller.Nickname} ({caller.UserId})"
                : sender.LogName;

            Log.Info(
                $"[SupplyDrop] {author} a declenche le largage \"{profile.Key}\" " +
                $"(annonce : {announce}, position forcee : {forcedPosition?.ToString() ?? "non"}).");

            response = plugin.Translation.DropCalled.Replace("%KEY%", profile.Key);
            return true;
        }
    }
}

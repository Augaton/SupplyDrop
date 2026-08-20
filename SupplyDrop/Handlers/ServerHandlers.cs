using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using SupplyDrop.API;

namespace SupplyDrop.Handlers
{
    public sealed class ServerHandlers
    {
        private readonly DropService service;

        public ServerHandlers(DropService service) => this.service = service;

        public void OnRoundStarted()
        {
            try
            {
                service.Start();
            }
            catch (Exception e)
            {
                Log.Error($"OnRoundStarted: {e}");
            }
        }

        public void OnRoundEnded(RoundEndedEventArgs ev) => Cleanup();

        public void OnRestartingRound() => Cleanup();

        public void OnWaitingForPlayers() => Cleanup();

        private void Cleanup()
        {
            try
            {
                service.Stop();
            }
            catch (Exception e)
            {
                Log.Error($"Cleanup: {e}");
            }
        }
    }
}

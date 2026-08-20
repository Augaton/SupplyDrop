using Exiled.API.Interfaces;

namespace SupplyDrop
{
    public sealed class Translation : ITranslation
    {
        public string NoPermission { get; set; } = "Permission refusee.";

        public string PluginUnavailable { get; set; } = "Plugin indisponible.";

        public string RoundNotStarted { get; set; } = "Le round n'a pas commence.";

        public string UnknownProfile { get; set; } = "Profil \"%KEY%\" inconnu. Utilisez supplydrop list pour voir les cles disponibles.";

        public string DropCalled { get; set; } = "Largage \"%KEY%\" declenche.";

        public string NoProfile { get; set; } = "Aucun profil de largage configure.";
    }
}

using System.ComponentModel;
using Exiled.API.Interfaces;

namespace SupplyDrop
{
    public sealed class Translation : ITranslation
    {
        [Description("Reponse de commande quand l'appelant n'a pas la permission.")]
        public string NoPermission { get; set; } = "Permission refusee.";

        [Description("Reponse de commande quand le plugin n'est pas actif.")]
        public string PluginUnavailable { get; set; } = "Plugin indisponible.";

        [Description("Reponse quand la commande exige un round en cours.")]
        public string RoundNotStarted { get; set; } = "Le round n'a pas commence.";

        [Description("Reponse quand la cle de profil n'existe pas. %KEY% est remplace par la cle saisie.")]
        public string UnknownProfile { get; set; } = "Profil \"%KEY%\" inconnu. Utilisez supplydrop list pour voir les cles disponibles.";

        [Description("Reponse apres declenchement manuel d'un largage. %KEY% est remplace par la cle du profil.")]
        public string DropCalled { get; set; } = "Largage \"%KEY%\" declenche.";

        [Description("Reponse quand aucun profil de largage n'est configure.")]
        public string NoProfile { get; set; } = "Aucun profil de largage configure.";
    }
}

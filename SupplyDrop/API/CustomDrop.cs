using System.Collections.Generic;
using System.ComponentModel;

namespace SupplyDrop.API
{
    public sealed class CustomDrop
    {
        [Description("Ajoute des objets personnalises au largage. Sans effet si Exiled.CustomItems n'est pas charge.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Registered = tirage au hasard parmi tout ce qui est enregistre, Pool = uniquement la liste ci-dessous, Both = les deux.")]
        public CustomDropSource Source { get; set; } = CustomDropSource.Registered;

        [Description("Types d'objet de base retenus par la decouverte automatique. SCP500 ne garde que les pilules.")]
        public List<ItemType> BaseItems { get; set; } = new List<ItemType> { ItemType.SCP500 };

        [Description("References exclues du tirage automatique. Nom affiche ou identifiant numerique.")]
        public List<string> Excluded { get; set; } = new List<string>();

        [Description("Poids attribue aux objets trouves par la decouverte automatique.")]
        public int DefaultWeight { get; set; } = 10;

        [Description("Nombre de tirages effectues a chaque largage.")]
        public int Draws { get; set; } = 2;

        [Description("Chance en pourcent qu'un tirage donne effectivement un objet (0-100).")]
        public int Chance { get; set; } = 60;

        [Description("Autorise le meme objet plusieurs fois dans un largage.")]
        public bool AllowDuplicates { get; set; } = false;

        [Description("Liste explicite, utilisee par les sources Pool et Both. Les references introuvables sont signalees une fois par round.")]
        public List<CustomDropItem> Pool { get; set; } = new List<CustomDropItem>();
    }
}

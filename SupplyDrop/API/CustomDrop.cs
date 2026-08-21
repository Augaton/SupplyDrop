using System.Collections.Generic;
using System.ComponentModel;

namespace SupplyDrop.API
{
    public sealed class CustomDrop
    {
        [Description("Ajoute des objets personnalises au largage. Sans effet si Exiled.CustomItems n'est pas charge.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Nombre de tirages effectues dans le pool a chaque largage.")]
        public int Draws { get; set; } = 2;

        [Description("Chance en pourcent qu'un tirage donne effectivement un objet (0-100).")]
        public int Chance { get; set; } = 60;

        [Description("Autorise le meme objet plusieurs fois dans un largage.")]
        public bool AllowDuplicates { get; set; } = false;

        [Description("Pool de tirage. Les references introuvables sont ignorees, signalees une seule fois par round.")]
        public List<CustomDropItem> Pool { get; set; } = new List<CustomDropItem>();
    }
}

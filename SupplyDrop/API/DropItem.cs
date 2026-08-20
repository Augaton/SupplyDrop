using System.ComponentModel;

namespace SupplyDrop.API
{
    public sealed class DropItem
    {
        [Description("Objet largue.")]
        public ItemType Item { get; set; } = ItemType.None;

        [Description("Nombre d'exemplaires tentes.")]
        public int Quantity { get; set; } = 1;

        [Description("Chance en pourcent que chaque exemplaire apparaisse reellement (0-100).")]
        public int Chance { get; set; } = 100;
    }
}

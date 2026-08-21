using System.ComponentModel;

namespace SupplyDrop.API
{
    public sealed class CustomDropItem
    {
        [Description("Nom affiche de l'objet personnalise, ou son identifiant numerique. Ex : SCP500-Blindage ou 18.")]
        public string Reference { get; set; } = string.Empty;

        [Description("Poids relatif dans le tirage. 0 = jamais tire.")]
        public int Weight { get; set; } = 10;
    }
}

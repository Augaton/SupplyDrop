using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using SupplyDrop.API;

namespace SupplyDrop
{
    public sealed class Config : IConfig
    {
        [Description("Active ou desactive le plugin.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Active les logs de debug.")]
        public bool Debug { get; set; } = false;

        [Description("Nombre minimum de joueurs connectes pour qu'un largage se declenche.")]
        public int MinPlayers { get; set; } = 2;

        [Description("Delai en secondes avant de reessayer quand le nombre de joueurs est insuffisant.")]
        public float RetrySeconds { get; set; } = 60f;

        [Description("Intervalle en secondes du planificateur. Une seule coroutine gere tous les largages.")]
        public float SchedulerTick { get; set; } = 1f;

        [Description("Journalise chaque largage dans la console serveur.")]
        public bool LogDrops { get; set; } = true;

        [Description("Nombre maximum d'objets places par largage, garde-fou contre une config aberrante.")]
        public int MaxItemsPerDrop { get; set; } = 60;

        [Description("Hauteur en metres ajoutee au point de largage pour eviter que les objets traversent le sol.")]
        public float SpawnHeightOffset { get; set; } = 0.5f;

        [Description("Profils de largage. Ajouter une entree avec une cle inutilisee cree un nouveau type de largage.")]
        public List<DropProfile> Profiles { get; set; } = DefaultProfiles.Create();
    }
}

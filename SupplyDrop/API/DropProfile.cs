using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using PlayerRoles;
using UnityEngine;

namespace SupplyDrop.API
{
    public sealed class DropProfile
    {
        [Description("Cle interne du largage, utilisee par la commande et dans les logs.")]
        public string Key { get; set; } = string.Empty;

        [Description("Active ou desactive ce largage.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Nom affiche du largage.")]
        public string DisplayName { get; set; } = string.Empty;

        [Description("Broadcast diffuse a l'annonce du largage.")]
        public string Broadcast { get; set; } = string.Empty;

        [Description("Duree du broadcast, en secondes.")]
        public ushort BroadcastDuration { get; set; } = 10;

        [Description("Annonce C.A.S.S.I.E diffusee en meme temps. Laisser vide pour desactiver.")]
        public string CassieAnnouncement { get; set; } = string.Empty;

        [Description("Sous-titres C.A.S.S.I.E. Laisser vide pour ne pas en afficher.")]
        public string CassieSubtitles { get; set; } = string.Empty;

        [Description("Delai en secondes apres le debut du round avant le premier largage.")]
        public float FirstDropSeconds { get; set; } = 600f;

        [Description("Delai en secondes entre deux largages de ce type.")]
        public float IntervalSeconds { get; set; } = 600f;

        [Description("Delai en secondes entre l'annonce et l'apparition reelle des objets.")]
        public float AnnouncementLeadSeconds { get; set; } = 15f;

        [Description("Nombre maximum de largages par round. 0 ou moins = sans limite.")]
        public int MaxDropsPerRound { get; set; } = -1;

        [Description("Faction associee, utilisee uniquement pour l'affichage. NtfWave ou ChaosWave.")]
        public SpawnableFaction Faction { get; set; } = SpawnableFaction.NtfWave;

        [Description("Role dont le point d'apparition sert d'ancre de largage. Utilise tant qu'aucune coordonnee explicite n'est renseignee.")]
        public RoleTypeId FallbackRole { get; set; } = RoleTypeId.NtfPrivate;

        [Description("Coordonnees fixes des munitions. 0,0,0 = ancre du role de repli, valeur recommandee.")]
        public Vector3 AmmoPosition { get; set; } = Vector3.zero;

        [Description("Coordonnees fixes des armures. 0,0,0 = ancre du role de repli, valeur recommandee.")]
        public Vector3 ArmorPosition { get; set; } = Vector3.zero;

        [Description("Coordonnees fixes des objets divers. 0,0,0 = ancre du role de repli, valeur recommandee.")]
        public Vector3 ItemPosition { get; set; } = Vector3.zero;

        [Description("Coordonnees fixes des armes. 0,0,0 = ancre du role de repli, valeur recommandee.")]
        public Vector3 WeaponPosition { get; set; } = Vector3.zero;

        [Description("Rayon horizontal de dispersion autour de la position, en metres. 0 = empilement exact.")]
        public float ScatterRadius { get; set; } = 1.2f;

        [Description("Pose un halo lumineux sur la zone de largage.")]
        public bool EnableBeacon { get; set; } = true;

        [Description("Couleur du halo, au format hexadecimal.")]
        public string BeaconColor { get; set; } = "#FFFFFF";

        [Description("Duree de vie du halo, en secondes.")]
        public float BeaconDuration { get; set; } = 60f;

        [Description("Contenu du largage.")]
        public List<DropItem> Items { get; set; } = new List<DropItem>();

        [Description("Objets personnalises ajoutes au largage, via Exiled.CustomItems. Compatibilite optionnelle.")]
        public CustomDrop CustomItems { get; set; } = new CustomDrop();
    }
}

using System.Collections.Generic;
using Exiled.API.Enums;
using PlayerRoles;
using UnityEngine;

namespace SupplyDrop.API
{
    public static class DefaultProfiles
    {
        public static List<DropProfile> Create()
        {
            return new List<DropProfile>
            {
                new DropProfile
                {
                    Key = "mtf",
                    DisplayName = "Largage du Foudroyeur",
                    Broadcast = "<size=35><i><color=#0080FF>Un helicoptere du Foudroyeur</color> <color=#5c5c5c>vient de larguer</color> <color=#7a7a7a>du ravitaillement</color></i></size>",
                    BroadcastDuration = 10,
                    CassieAnnouncement = "mtf supply drop incoming",
                    CassieSubtitles = "Largage de ravitaillement du Foudroyeur en approche",
                    FirstDropSeconds = 600f,
                    IntervalSeconds = 600f,
                    AnnouncementLeadSeconds = 15f,
                    MaxDropsPerRound = -1,
                    Faction = SpawnableFaction.NtfWave,
                    FallbackRole = RoleTypeId.NtfPrivate,
                    AmmoPosition = new Vector3(173f, 993f, -56f),
                    ArmorPosition = new Vector3(173f, 993f, -58f),
                    ItemPosition = new Vector3(173f, 993f, -60f),
                    WeaponPosition = new Vector3(173f, 993f, -62f),
                    ScatterRadius = 1.2f,
                    EnableBeacon = true,
                    BeaconColor = "#0080FF",
                    BeaconDuration = 60f,
                    Items = new List<DropItem>
                    {
                        new DropItem { Item = ItemType.GunCOM18, Quantity = 1, Chance = 100 },
                        new DropItem { Item = ItemType.GunE11SR, Quantity = 1, Chance = 100 },
                        new DropItem { Item = ItemType.Ammo762x39, Quantity = 2, Chance = 100 },
                        new DropItem { Item = ItemType.Ammo9x19, Quantity = 2, Chance = 100 },
                        new DropItem { Item = ItemType.Medkit, Quantity = 2, Chance = 100 },
                        new DropItem { Item = ItemType.Medkit, Quantity = 2, Chance = 20 },
                        new DropItem { Item = ItemType.Adrenaline, Quantity = 1, Chance = 100 },
                        new DropItem { Item = ItemType.ArmorCombat, Quantity = 1, Chance = 40 },
                        new DropItem { Item = ItemType.KeycardO5, Quantity = 1, Chance = 10 },
                    },
                },
                new DropProfile
                {
                    Key = "chaos",
                    DisplayName = "Largage de l'Insurrection du Chaos",
                    Broadcast = "<size=35><i><color=#5c5c5c>Un vehicule de</color> <color=#28AD00>l'Insurrection du Chaos</color> <color=#5c5c5c>vient de deposer</color> <color=#7a7a7a>du ravitaillement</color></i></size>",
                    BroadcastDuration = 10,
                    CassieAnnouncement = "chaos insurgency supply drop detected",
                    CassieSubtitles = "Largage de ravitaillement de l'Insurrection du Chaos detecte",
                    FirstDropSeconds = 900f,
                    IntervalSeconds = 600f,
                    AnnouncementLeadSeconds = 15f,
                    MaxDropsPerRound = -1,
                    Faction = SpawnableFaction.ChaosWave,
                    FallbackRole = RoleTypeId.ChaosRifleman,
                    AmmoPosition = new Vector3(2f, 989f, -50f),
                    ArmorPosition = new Vector3(4.5f, 989f, -50f),
                    ItemPosition = new Vector3(7f, 989f, -50f),
                    WeaponPosition = new Vector3(9.5f, 989f, -50f),
                    ScatterRadius = 1.2f,
                    EnableBeacon = true,
                    BeaconColor = "#28AD00",
                    BeaconDuration = 60f,
                    Items = new List<DropItem>
                    {
                        new DropItem { Item = ItemType.GunLogicer, Quantity = 2, Chance = 100 },
                        new DropItem { Item = ItemType.Ammo762x39, Quantity = 5, Chance = 100 },
                        new DropItem { Item = ItemType.Medkit, Quantity = 2, Chance = 100 },
                        new DropItem { Item = ItemType.ArmorCombat, Quantity = 2, Chance = 20 },
                        new DropItem { Item = ItemType.Adrenaline, Quantity = 1, Chance = 100 },
                        new DropItem { Item = ItemType.KeycardO5, Quantity = 1, Chance = 10 },
                    },
                },
            };
        }
    }
}

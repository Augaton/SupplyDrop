using System.Collections.Generic;
using Exiled.API.Enums;
using PlayerRoles;

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
                    DisplayName = "Largage MTF",
                    Broadcast = "<size=35><i><color=#0080FF>Un helicoptere MTF</color> <color=#5c5c5c>vient de larguer</color> <color=#7a7a7a>du ravitaillement</color></i></size>",
                    BroadcastDuration = 10,
                    CassieAnnouncement = "mtf supply drop incoming",
                    CassieSubtitles = "Largage de ravitaillement MTF en approche",
                    FirstDropSeconds = 600f,
                    IntervalSeconds = 600f,
                    AnnouncementLeadSeconds = 15f,
                    MaxDropsPerRound = -1,
                    Faction = SpawnableFaction.NtfWave,
                    FallbackRole = RoleTypeId.NtfPrivate,
                    ScatterRadius = 2.5f,
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
                        new DropItem { Item = ItemType.Coin, Quantity = 2, Chance = 100 },
                        new DropItem { Item = ItemType.KeycardO5, Quantity = 1, Chance = 10 },
                    },
                    CustomItems = new CustomDrop
                    {
                        IsEnabled = true,
                        Draws = 2,
                        Chance = 60,
                        AllowDuplicates = false,
                        Pool = new List<CustomDropItem>
                        {
                            new CustomDropItem { Reference = "SCP500-Blindage", Weight = 15 },
                            new CustomDropItem { Reference = "SCP500-Chirurgien", Weight = 15 },
                            new CustomDropItem { Reference = "SCP500-Vitalite", Weight = 15 },
                            new CustomDropItem { Reference = "SCP500-Endurance", Weight = 10 },
                            new CustomDropItem { Reference = "SCP500-Nyctalope", Weight = 10 },
                            new CustomDropItem { Reference = "SCP500-Arsenal", Weight = 6 },
                            new CustomDropItem { Reference = "SCP500-Panacee", Weight = 4 },
                        },
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
                    ScatterRadius = 2.5f,
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
                        new DropItem { Item = ItemType.Coin, Quantity = 2, Chance = 100 },
                        new DropItem { Item = ItemType.KeycardO5, Quantity = 1, Chance = 10 },
                    },
                    CustomItems = new CustomDrop
                    {
                        IsEnabled = true,
                        Draws = 2,
                        Chance = 60,
                        AllowDuplicates = false,
                        Pool = new List<CustomDropItem>
                        {
                            new CustomDropItem { Reference = "SCP500-Adrenaline", Weight = 15 },
                            new CustomDropItem { Reference = "SCP500-Regeneration", Weight = 15 },
                            new CustomDropItem { Reference = "SCP500-Sonic", Weight = 12 },
                            new CustomDropItem { Reference = "SCP500-Shadow", Weight = 10 },
                            new CustomDropItem { Reference = "SCP500-Plume", Weight = 10 },
                            new CustomDropItem { Reference = "SCP500-Juggernaut", Weight = 6 },
                            new CustomDropItem { Reference = "SCP500-Passe-partout", Weight = 4 },
                        },
                    },
                },
            };
        }
    }
}

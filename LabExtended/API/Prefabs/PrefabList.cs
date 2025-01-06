using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Extensions;

using Mirror;

using System.Collections.ObjectModel;

namespace LabExtended.API.Prefabs
{
    /// <summary>
    /// Serves as a list of all known prefabs.
    /// </summary>
    public static class PrefabList
    {
        private static IReadOnlyDictionary<string, PrefabDefinition> _allPrefabs;

        /// <summary>
        /// Gets a list of all prefabs keyed by property names.
        /// </summary>
        public static IReadOnlyDictionary<string, PrefabDefinition> AllPrefabs
        {
            get
            {
                if (_allPrefabs is null)
                {
                    var allDict = DictionaryPool<string, PrefabDefinition>.Shared.Rent();
                    var allProps = typeof(PrefabList).GetAllProperties();

                    foreach (var prop in allProps)
                    {
                        if (prop.PropertyType != typeof(PrefabDefinition))
                            continue;

                        allDict.Add(prop.Name, (PrefabDefinition)prop.GetValue(null));
                    }

                    _allPrefabs = new ReadOnlyDictionary<string, PrefabDefinition>(allDict);

                    DictionaryPool<string, PrefabDefinition>.Shared.Return(allDict);

                    foreach (var prefab in NetworkClient.prefabs)
                    {
                        if (!_allPrefabs.TryGetFirst(x => x.Value.Name == prefab.Value.name, out _))
                            ApiLog.Warn("Prefab API", $"Prefab &1{prefab.Value.name}&r is not defined in PrefabList!");
                    }

                    foreach (var prefab in _allPrefabs)
                    {
                        if (!NetworkClient.prefabs.TryGetFirst(x => x.Value.name == prefab.Value.Name, out _))
                            ApiLog.Warn("Prefab API", $"Prefab &1{prefab.Key}&r has either been renamed or removed!");
                    }

                    ApiLog.Info("Prefab API", $"Loaded &2{_allPrefabs.Count}&r / &1{NetworkClient.prefabs.Count}&r prefabs!");
                }

                return _allPrefabs;
            }
        }

        #region Other Prefabs
        public static PrefabDefinition Player { get; } = new PrefabDefinition("Player");
        public static PrefabDefinition AmnesticCloud { get; } = new PrefabDefinition("Amnestic Cloud Hazard");
        public static PrefabDefinition Tantrum { get; } = new PrefabDefinition("TantrumObj");
        #endregion

        #region Other Pickups
        public static PrefabDefinition PainKillers { get; } = new PrefabDefinition("PainkillersPickup");
        public static PrefabDefinition Adrenaline { get; } = new PrefabDefinition("AdrenalinePrefab");
        public static PrefabDefinition Medkit { get; } = new PrefabDefinition("MedkitPickup");

        public static PrefabDefinition ChaosKeycard { get; } = new PrefabDefinition("ChaosKeycardPickup");
        public static PrefabDefinition RegularKeycard { get; } = new PrefabDefinition("RegularKeycardPickup");

        public static PrefabDefinition Coin { get; } = new PrefabDefinition("CoinPickup");
        public static PrefabDefinition Radio { get; } = new PrefabDefinition("RadioPickup");

        public static PrefabDefinition Flashlight { get; } = new PrefabDefinition("FlashlightPickup");
        public static PrefabDefinition Lantern { get; } = new PrefabDefinition("LanternPickup");
        #endregion

        #region Ammo Pickups
        public static PrefabDefinition Ammo12ga { get; } = new PrefabDefinition("Ammo12gaPickup");
        public static PrefabDefinition Ammo44cal { get; } = new PrefabDefinition("Ammo44calPickup");
        public static PrefabDefinition Ammo556mm { get; } = new PrefabDefinition("Ammo556mmPickup");
        public static PrefabDefinition Ammo762mm { get; } = new PrefabDefinition("Ammo762mmPickup");
        public static PrefabDefinition Ammo9mm { get; } = new PrefabDefinition("Ammo9mmPickup");
        #endregion

        #region Scp Items
        public static PrefabDefinition Scp207 { get; } = new PrefabDefinition("SCP207Pickup");
        public static PrefabDefinition Scp244a { get; } = new PrefabDefinition("SCP244APickup Variant");
        public static PrefabDefinition Scp244b { get; } = new PrefabDefinition("SCP244BPickup Variant");
        public static PrefabDefinition Scp268 { get; } = new PrefabDefinition("SCP268Pickup");
        public static PrefabDefinition Scp330 { get; } = new PrefabDefinition("Scp330Pickup");
        public static PrefabDefinition Scp500 { get; } = new PrefabDefinition("SCP500Pickup");
        public static PrefabDefinition Scp1344 { get; } = new PrefabDefinition("SCP1344Pickup");
        public static PrefabDefinition Scp1576 { get; } = new PrefabDefinition("SCP1576Pickup");
        public static PrefabDefinition Scp1853 { get; } = new PrefabDefinition("SCP1853Pickup");
        public static PrefabDefinition AntiScp207 { get; } = new PrefabDefinition("AntiSCP207Pickup");
        #endregion

        #region Scp Item Pedestals
        public static PrefabDefinition Scp2176Pedestal { get; } = new PrefabDefinition("Scp2176PedestalStructure Variant");
        public static PrefabDefinition Scp1853Pedestal { get; } = new PrefabDefinition("Scp1853PedestalStructure Variant");
        public static PrefabDefinition Scp1576Pedestal { get; } = new PrefabDefinition("Scp1576PedestalStructure Variant");
        public static PrefabDefinition Scp1344Pedestal { get; } = new PrefabDefinition("Scp1344PedestalStructure Variant");
        public static PrefabDefinition Scp500Pedestal { get; } = new PrefabDefinition("Scp500PedestalStructure Variant");
        public static PrefabDefinition Scp268Pedestal { get; } = new PrefabDefinition("Scp268PedestalStructure Variant");
        public static PrefabDefinition Scp244Pedestal { get; } = new PrefabDefinition("Scp244PedestalStructure Variant");
        public static PrefabDefinition Scp207Pedestal { get; } = new PrefabDefinition("Scp207PedestalStructure Variant");
        public static PrefabDefinition Scp018Pedestal { get; } = new PrefabDefinition("Scp018PedestalStructure Variant");
        public static PrefabDefinition AntiScp207Pedestal { get; } = new PrefabDefinition("AntiScp207PedestalStructure Variant");
        #endregion

        #region Armor Pickups
        public static PrefabDefinition CombatArmor { get; } = new PrefabDefinition("Combat Armor Pickup");
        public static PrefabDefinition LightArmor { get; } = new PrefabDefinition("Light Armor Pickup");
        public static PrefabDefinition HeavyArmor { get; } = new PrefabDefinition("Heavy Armor Pickup");
        #endregion

        #region Weapons
        public static PrefabDefinition Jailbird { get; } = new PrefabDefinition("JailbirdPickup");
        public static PrefabDefinition MicroHid { get; } = new PrefabDefinition("MicroHidPickup");
        public static PrefabDefinition Firearm { get; } = new PrefabDefinition("FirearmPickup");

        public static PrefabDefinition ExplosiveGrenadeItem { get; } = new PrefabDefinition("HegPickup");
        public static PrefabDefinition ExplosiveGrenadePickup { get; } = new PrefabDefinition("HegProjectile");

        public static PrefabDefinition FlashGrenadeItem { get; } = new PrefabDefinition("FlashbangPickup");
        public static PrefabDefinition FlashGrenadeProjectile { get; } = new PrefabDefinition("FlashbangProjectile");

        public static PrefabDefinition Scp2176Projectile { get; } = new PrefabDefinition("Scp2176Projectile");
        public static PrefabDefinition Scp018Projectile { get; } = new PrefabDefinition("Scp018Projectile");
        #endregion

        #region Spawnable Map Prefabs
        public static PrefabDefinition SportTarget { get; } = new PrefabDefinition("sportTargetPrefab");
        public static PrefabDefinition ClassDTarget { get; } = new PrefabDefinition("dboyTargetPrefab");
        public static PrefabDefinition BinaryTarget { get; } = new PrefabDefinition("binaryTargetPrefab");

        public static PrefabDefinition Primitive { get; } = new PrefabDefinition("PrimitiveObjectToy");
        public static PrefabDefinition LightSource { get; } = new PrefabDefinition("LightSourceToy");
        public static PrefabDefinition Speaker { get; } = new PrefabDefinition("SpeakerToy");

        public static PrefabDefinition EntranceZoneDoor { get; } = new PrefabDefinition("EZ BreakableDoor");
        public static PrefabDefinition LightZoneDoor { get; } = new PrefabDefinition("LCZ BreakableDoor");
        public static PrefabDefinition HeavyZoneDoor { get; } = new PrefabDefinition("HCZ BreakableDoor");
        public static PrefabDefinition HeavyZoneBulkDoor { get; } = new PrefabDefinition("HCZ BulkDoor");

        public static PrefabDefinition HealthBox { get; } = new PrefabDefinition("Player");
        public static PrefabDefinition AdrenalineBox { get; } = new PrefabDefinition("RegularMedkitStructure");

        public static PrefabDefinition Generator { get; } = new PrefabDefinition("GeneratorStructure");
        public static PrefabDefinition Workstation { get; } = new PrefabDefinition("Spawnable Work Station Structure");
        public static PrefabDefinition Locker { get; } = new PrefabDefinition("MiscLocker");

        public static PrefabDefinition ElevatorGates { get; } = new PrefabDefinition("ElevatorChamber Gates");
        public static PrefabDefinition ElevatorChamber { get; } = new PrefabDefinition("ElevatorChamber");
        public static PrefabDefinition NukeElevatorChamber { get; } = new PrefabDefinition("ElevatorChamberNuke");

        public static PrefabDefinition ExperimentalWeaponLocker { get; } = new PrefabDefinition("Experimental Weapon Locker");
        public static PrefabDefinition LargeGunLocker { get; } = new PrefabDefinition("LargeGunLockerStructure");
        public static PrefabDefinition RifleRack { get; } = new PrefabDefinition("RifleRackStructure");

        public static PrefabDefinition HczOneSided { get; } = new PrefabDefinition("HCZ OneSided");
        public static PrefabDefinition HczTwoSided { get; } = new PrefabDefinition("HCZ TwoSided");

        public static PrefabDefinition HczOpenHallway { get; } = new PrefabDefinition("OpenHallway");
        public static PrefabDefinition HczOpenHallwayConstructA { get; } = new PrefabDefinition("OpenHallway Construct A");
        public static PrefabDefinition HczOpenHallwayClutterA { get; } = new PrefabDefinition("OpenHallway Clutter A");
        public static PrefabDefinition HczOpenHallwayClutterB { get; } = new PrefabDefinition("OpenHallway Clutter B");
        public static PrefabDefinition HczOpenHallwayClutterC { get; } = new PrefabDefinition("OpenHallway Clutter C");
        public static PrefabDefinition HczOpenHallwayClutterD { get; } = new PrefabDefinition("OpenHallway Clutter D");
        public static PrefabDefinition HczOpenHallwayClutterE { get; } = new PrefabDefinition("OpenHallway Clutter E");
        public static PrefabDefinition HczOpenHallwayClutterF { get; } = new PrefabDefinition("OpenHallway Clutter F");
        public static PrefabDefinition HczOpenHallwayClutterG { get; } = new PrefabDefinition("OpenHallway Clutter G");
        #endregion
    }
}
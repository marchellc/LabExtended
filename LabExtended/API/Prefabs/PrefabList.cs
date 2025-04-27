using LabExtended.Core;
using LabExtended.Extensions;

using Mirror;

namespace LabExtended.API.Prefabs;

/// <summary>
/// Serves as a list of all known prefabs.
/// </summary>
public static class PrefabList
{
    private static Dictionary<string, PrefabDefinition> _allPrefabs = new();

    /// <summary>
    /// Gets a list of all prefabs keyed by property names.
    /// </summary>
    public static IReadOnlyDictionary<string, PrefabDefinition> AllPrefabs
    {
        get
        {
            if (_allPrefabs.Count < NetworkClient.prefabs.Count)
            {
                var allDict = _allPrefabs;
                var allProps = typeof(PrefabList).GetAllProperties();

                foreach (var prop in allProps)
                {
                    if (allDict.ContainsKey(prop.Name))
                        continue;

                    if (prop.PropertyType != typeof(PrefabDefinition))
                        continue;

                    var value = prop.GetValue(null);

                    if (value is not PrefabDefinition definition)
                        continue;

                    if (allDict.ContainsKey(definition.Name))
                        continue;

                    allDict.Add(prop.Name, definition);
                }

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

                ApiLog.Info("Prefab API",
                    $"Loaded &2{_allPrefabs.Count}&r / &1{NetworkClient.prefabs.Count}&r prefabs!");
            }

            return _allPrefabs;
        }
    }

    #region Other Prefabs

    /// <summary>
    /// Gets the prefab of a player.
    /// </summary>
    public static PrefabDefinition Player { get; } = new("Player");

    /// <summary>
    /// Gets the prefab of SCP-939's Amnestic Cloud.
    /// </summary>
    public static PrefabDefinition AmnesticCloud { get; } = new("Amnestic Cloud Hazard");

    /// <summary>
    /// Gets the prefab of SCP-173's Tantrum.
    /// </summary>
    public static PrefabDefinition Tantrum { get; } = new("TantrumObj");

    #endregion

    #region Other Pickups

    /// <summary>
    /// Gets the prefab of a painkillers bottle.
    /// </summary>
    public static PrefabDefinition PainKillers { get; } = new("PainkillersPickup");

    /// <summary>
    /// Gets the prefab of an adrenaline.
    /// </summary>
    public static PrefabDefinition Adrenaline { get; } = new("AdrenalinePrefab");

    /// <summary>
    /// Gets the prefab of a medkit.
    /// </summary>
    public static PrefabDefinition Medkit { get; } = new("MedkitPickup");

    /// <summary>
    /// Gets the prefab of a Chaos Insurgency keycard.
    /// </summary>
    public static PrefabDefinition ChaosKeycard { get; } = new("ChaosKeycardPickup");

    /// <summary>
    /// Gets the prefab of a regular keycard.
    /// </summary>
    public static PrefabDefinition RegularKeycard { get; } = new("RegularKeycardPickup");

    /// <summary>
    /// Gets the prefab of a coin.
    /// </summary>
    public static PrefabDefinition Coin { get; } = new("CoinPickup");

    /// <summary>
    /// Gets the prefab of a radio.
    /// </summary>
    public static PrefabDefinition Radio { get; } = new("RadioPickup");

    /// <summary>
    /// Gets the prefab of a flashlight.
    /// </summary>
    public static PrefabDefinition Flashlight { get; } = new("FlashlightPickup");

    /// <summary>
    /// Gets the prefab of a lantern.
    /// </summary>
    public static PrefabDefinition Lantern { get; } = new("LanternPickup");

    #endregion

    #region Ammo Pickups

    /// <summary>
    /// Gets the prefab of a 12-gauge ammo box.
    /// </summary>
    public static PrefabDefinition Ammo12ga { get; } = new("Ammo12gaPickup");

    /// <summary>
    /// Gets the prefab of a 44-cal ammo box.
    /// </summary>
    public static PrefabDefinition Ammo44cal { get; } = new("Ammo44calPickup");

    /// <summary>
    /// Gets the prefab of a 5.56mm ammo box.
    /// </summary>
    public static PrefabDefinition Ammo556mm { get; } = new("Ammo556mmPickup");

    /// <summary>
    /// Gets the prefab of a 7.62mm ammo box.
    /// </summary>
    public static PrefabDefinition Ammo762mm { get; } = new("Ammo762mmPickup");

    /// <summary>
    /// Gets the prefab of a 9mm ammo box.
    /// </summary>
    public static PrefabDefinition Ammo9mm { get; } = new("Ammo9mmPickup");

    #endregion

    #region Scp Items

    /// <summary>
    /// Gets the prefab of an SCP-207 bottle.
    /// </summary>
    public static PrefabDefinition Scp207 { get; } = new("SCP207Pickup");

    /// <summary>
    /// Gets the prefab of an SCP-244-A vase.
    /// </summary>
    public static PrefabDefinition Scp244a { get; } = new("SCP244APickup Variant");

    /// <summary>
    /// Gets the prefab of an SCP-244-B vase.
    /// </summary>
    public static PrefabDefinition Scp244b { get; } = new("SCP244BPickup Variant");

    /// <summary>
    /// Gets the prefab of an SCP-268 hat.
    /// </summary>
    public static PrefabDefinition Scp268 { get; } = new("SCP268Pickup");

    /// <summary>
    /// Gets the prefab of an SCP-330 candy bag.
    /// </summary>
    public static PrefabDefinition Scp330 { get; } = new("Scp330Pickup");

    /// <summary>
    /// Gets the prefab of an SCP-500 bottle.
    /// </summary>
    public static PrefabDefinition Scp500 { get; } = new("SCP500Pickup");

    /// <summary>
    /// Gets the prefab of an SCP-3114 goggles.
    /// </summary>
    public static PrefabDefinition Scp1344 { get; } = new("SCP1344Pickup");

    /// <summary>
    /// Gets the prefab of an SCP-1576 gramophone.
    /// </summary>
    public static PrefabDefinition Scp1576 { get; } = new("SCP1576Pickup");

    /// <summary>
    /// Gets the prefab of an SCP-1853.
    /// </summary>
    public static PrefabDefinition Scp1853 { get; } = new("SCP1853Pickup");

    /// <summary>
    /// Gets the prefab of an anti-SCP-207 bottle.
    /// </summary>
    public static PrefabDefinition AntiScp207 { get; } = new("AntiSCP207Pickup");

    #endregion

    #region Scp Item Pedestals

    /// <summary>
    /// Gets the prefab of an SCP-2176 pedestal.
    /// </summary>
    public static PrefabDefinition Scp2176Pedestal { get; } = new("Scp2176PedestalStructure Variant");

    /// <summary>
    /// Gets the prefab of an SCP-1853 pedestal.
    /// </summary>
    public static PrefabDefinition Scp1853Pedestal { get; } = new("Scp1853PedestalStructure Variant");

    /// <summary>
    /// Gets the prefab of SCP-1576 pedestal.
    /// </summary>
    public static PrefabDefinition Scp1576Pedestal { get; } = new("Scp1576PedestalStructure Variant");

    /// <summary>
    /// Gets the prefab of an SCP-1344 pedestal.
    /// </summary>
    public static PrefabDefinition Scp1344Pedestal { get; } = new("Scp1344PedestalStructure Variant");

    /// <summary>
    /// Gets the prefab of an SCP-500 pedestal.
    /// </summary>
    public static PrefabDefinition Scp500Pedestal { get; } = new("Scp500PedestalStructure Variant");

    /// <summary>
    /// Gets the prefab of an SCP-268 pedestal.
    /// </summary>
    public static PrefabDefinition Scp268Pedestal { get; } = new("Scp268PedestalStructure Variant");

    /// <summary>
    /// Gets the prefab of an SCP-244 pedestal.
    /// </summary>
    public static PrefabDefinition Scp244Pedestal { get; } = new("Scp244PedestalStructure Variant");

    /// <summary>
    /// Gets the prefab of an SCP-207 pedestal.
    /// </summary>
    public static PrefabDefinition Scp207Pedestal { get; } = new("Scp207PedestalStructure Variant");

    /// <summary>
    /// Gets the prefab of an SCP-018 pedestal.
    /// </summary>
    public static PrefabDefinition Scp018Pedestal { get; } = new("Scp018PedestalStructure Variant");

    /// <summary>
    /// Gets the prefab of an anti-SCP-207 pedestal.
    /// </summary>
    public static PrefabDefinition AntiScp207Pedestal { get; } = new("AntiScp207PedestalStructure Variant");

    #endregion

    #region Armor Pickups

    /// <summary>
    /// Gets the prefab of combat armor.
    /// </summary>
    public static PrefabDefinition CombatArmor { get; } = new PrefabDefinition("Combat Armor Pickup");

    /// <summary>
    /// Gets the prefab of light armor.
    /// </summary>
    public static PrefabDefinition LightArmor { get; } = new PrefabDefinition("Light Armor Pickup");

    /// <summary>
    /// Gets the prefab of heavy armor.
    /// </summary>
    public static PrefabDefinition HeavyArmor { get; } = new PrefabDefinition("Heavy Armor Pickup");

    #endregion

    #region Weapons

    /// <summary>
    /// Gets the prefab of a Jailbird.
    /// </summary>
    public static PrefabDefinition Jailbird { get; } = new("JailbirdPickup");

    /// <summary>
    /// Gets the prefab of a Micro-HID.
    /// </summary>
    public static PrefabDefinition MicroHid { get; } = new("MicroHidPickup");

    /// <summary>
    /// Gets the prefab of a firearm.
    /// </summary>
    public static PrefabDefinition Firearm { get; } = new("FirearmPickup");

    /// <summary>
    /// Gets the prefab of an explosive grenade.
    /// </summary>
    public static PrefabDefinition ExplosiveGrenadeItem { get; } = new("HegPickup");

    /// <summary>
    /// Gets the prefab of an explosive grenade projectile.
    /// </summary>
    public static PrefabDefinition ExplosiveGrenadePickup { get; } = new("HegProjectile");

    /// <summary>
    /// Gets the prefab of a flash grenade.
    /// </summary>
    public static PrefabDefinition FlashGrenadeItem { get; } = new("FlashbangPickup");

    /// <summary>
    /// Gets the prefab of a flash grenade projectile.
    /// </summary>
    public static PrefabDefinition FlashGrenadeProjectile { get; } = new("FlashbangProjectile");

    /// <summary>
    /// Gets the prefab of an SCP-2176 projectile.
    /// </summary>
    public static PrefabDefinition Scp2176Projectile { get; } = new("Scp2176Projectile");

    /// <summary>
    /// Gets the prefab of an SCP-018 projectile.
    /// </summary>
    public static PrefabDefinition Scp018Projectile { get; } = new("Scp018Projectile");

    #endregion

    #region Spawnable Map Prefabs

    /// <summary>
    /// Gets the sport version of the shooting target.
    /// </summary>
    public static PrefabDefinition SportTarget { get; } = new("sportTargetPrefab");

    /// <summary>
    /// Gets the prefab of the Class-D shooting target.
    /// </summary>
    public static PrefabDefinition ClassDTarget { get; } = new("dboyTargetPrefab");

    /// <summary>
    /// Gets the prefab of the binary shooting target.
    /// </summary>
    public static PrefabDefinition BinaryTarget { get; } = new("binaryTargetPrefab");

    /// <summary>
    /// Gets the prefab of the primitive object toy.
    /// </summary>
    public static PrefabDefinition Primitive { get; } = new("PrimitiveObjectToy");

    /// <summary>
    /// Gets the prefab of the light source toy.
    /// </summary>
    public static PrefabDefinition LightSource { get; } = new("LightSourceToy");

    /// <summary>
    /// Gets the prefab of the speaker toy.
    /// </summary>
    public static PrefabDefinition Speaker { get; } = new("SpeakerToy");

    /// <summary>
    /// Gets the prefab of the capybara toy.
    /// </summary>
    public static PrefabDefinition Capybara { get; } = new("CapybaraToy");
    
    /// <summary>
    /// Gets the prefab of the interactable toy.
    /// </summary>
    public static PrefabDefinition Interactable { get; } = new("InteractableToy");

    /// <summary>
    /// Gets the prefab of SCP-106's sinkhole.
    /// </summary>
    public static PrefabDefinition Sinkhole { get; } = new("Sinkhole");

    /// <summary>
    /// Gets the prefab of the Entrance Zone door. 
    /// </summary>
    public static PrefabDefinition EntranceZoneDoor { get; } = new("EZ BreakableDoor");

    /// <summary>
    /// Gets the prefab of the Light Containment Zone door.
    /// </summary>
    public static PrefabDefinition LightZoneDoor { get; } = new("LCZ BreakableDoor");

    /// <summary>
    /// Gets the prefab of the Heavy Containment Zone door.
    /// </summary>
    public static PrefabDefinition HeavyZoneDoor { get; } = new("HCZ BreakableDoor");

    /// <summary>
    /// Gets the prefab of the Heavy Containment Zone Bulk door.
    /// </summary>
    public static PrefabDefinition HeavyZoneBulkDoor { get; } = new("HCZ BulkDoor");

    /// <summary>
    /// Gets the prefab of the wall mounted health box.
    /// </summary>
    public static PrefabDefinition HealthBox { get; } = new("AdrenalineMedkitStructure");

    /// <summary>
    /// Gets the prefab of the wall mounted adrenaline box.
    /// </summary>
    public static PrefabDefinition AdrenalineBox { get; } = new("RegularMedkitStructure");

    /// <summary>
    /// Gets the prefab of a SCP-079 generator.
    /// </summary>
    public static PrefabDefinition Generator { get; } = new("GeneratorStructure");

    /// <summary>
    /// Gets the prefab of a workstation.
    /// </summary>
    public static PrefabDefinition Workstation { get; } = new("Spawnable Work Station Structure");

    /// <summary>
    /// Gets the prefab of a locker.
    /// </summary>
    public static PrefabDefinition Locker { get; } = new("MiscLocker");

    /// <summary>
    /// Gets the prefab of an elevator's gate.
    /// </summary>
    public static PrefabDefinition ElevatorGate { get; } = new("ElevatorChamber Gates");

    /// <summary>
    /// Gets the prefab of an elevator's chamber.
    /// </summary>
    public static PrefabDefinition ElevatorChamber { get; } = new("ElevatorChamber");

    /// <summary>
    /// Gets the prefab of the Alpha Warhead elevator.
    /// </summary>
    public static PrefabDefinition NukeElevatorChamber { get; } = new("ElevatorChamberNuke");

    /// <summary>
    /// Gets the prefab of the experimental weapon locker.
    /// </summary>
    public static PrefabDefinition ExperimentalWeaponLocker { get; } = new("Experimental Weapon Locker");

    /// <summary>
    /// Gets the prefab of the large weapon locker.
    /// </summary>
    public static PrefabDefinition LargeGunLocker { get; } = new("LargeGunLockerStructure");

    /// <summary>
    /// Gets the prefab of the rifle rack.
    /// </summary>
    public static PrefabDefinition RifleRack { get; } = new("RifleRackStructure");

    /// <summary>
    /// Gets the prefab of an open Heavy Containment Zone hallway.
    /// </summary>
    public static PrefabDefinition OpenHallway { get; } = new("OpenHallway");

    /// <summary>
    /// Gets the prefab of a broken electrical box.
    /// </summary>
    public static PrefabDefinition BrokenElectricalBox { get; } = new("Broken Electrical Box Open Connector");

    /// <summary>
    /// Gets the prefab of a simple box collection.
    /// </summary>
    public static PrefabDefinition SimpleBoxes { get; } = new("Simple Boxes Open Connector");
    
    /// <summary>
    /// Gets the prefab of short pipes.
    /// </summary>
    public static PrefabDefinition ShortPipes { get; } = new("Pipes Short Open Connector");
    
    /// <summary>
    /// Gets the prefab of long pipes.
    /// </summary>
    public static PrefabDefinition LongPipes { get; } = new("Pipes Long Open Connector");
    
    /// <summary>
    /// Gets the prefab of a box collection with a ladder.
    /// </summary>
    public static PrefabDefinition LadderBoxes { get; } = new("Boxes Ladder Open Connector");
    
    /// <summary>
    /// Gets the prefab of tank-supported shelf.
    /// </summary>
    public static PrefabDefinition Shelf { get; } = new("Tank-Supported Shelf Open Connector");
    
    /// <summary>
    /// Gets the prefab of angles fences.
    /// </summary>
    public static PrefabDefinition AngledFences { get; } = new("Angled Fences Open Connector");
    
    /// <summary>
    /// Gets the prefab of huge orange pipes.
    /// </summary>
    public static PrefabDefinition HugePipes { get; } = new("Huge Orange Pipes Open Connector");

    #endregion
}
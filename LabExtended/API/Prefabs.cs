using HarmonyLib;

using Interactables.Interobjects.DoorUtils;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Enums;

using LabExtended.Core;
using LabExtended.Extensions;
using MapGeneration;

using Mirror;

using PluginAPI.Core;

using UnityEngine;

namespace LabExtended.API
{
    public static class Prefabs
    {
        private static readonly LockedDictionary<PrefabType, GameObject> _prefabObjects = new LockedDictionary<PrefabType, GameObject>();
        private static readonly LockedDictionary<PrefabType, DoorVariant> _prefabDoors = new LockedDictionary<PrefabType, DoorVariant>();
        private static readonly LockedDictionary<PrefabType, string> _prefabNames = new LockedDictionary<PrefabType, string>();

        /// <summary>
        /// Gets a value indicating whether or not prefab names have been loaded.
        /// </summary>
        public static bool PrefabNamesLoaded => _prefabNames.Count > 0;

        /// <summary>
        /// Gets a value indicating whether or not door prefabs have been loaded.
        /// </summary>
        public static bool PrefabDoorsLoaded => _prefabDoors.Count > 0;

        /// <summary>
        /// Gets a value indicating whether or not prefabs have been loaded.
        /// </summary>
        public static bool PrefabObjectsLoaded => _prefabObjects.Count > 0;

        /// <summary>
        /// Gets the player prefab.
        /// </summary>
        public static GameObject PlayerPrefab => NetworkManager.singleton.playerPrefab;

        /// <summary>
        /// Gets the Light Containment Zone door prefab.
        /// </summary>
        public static DoorVariant LczDoorPrefab => _prefabDoors[PrefabType.LightContainmentZoneDoor];

        /// <summary>
        /// Gets the Heavy Containment Zone door prefab.
        /// </summary>
        public static DoorVariant HczDoorPrefab => _prefabDoors[PrefabType.HeavyContainmentZoneDoor];

        /// <summary>
        /// Gets the Entrance Zone door prefab.
        /// </summary>
        public static DoorVariant EzDoorPrefab => _prefabDoors[PrefabType.EntranceZoneDoor];

        /// <summary>
        /// Gets a new player instance.
        /// </summary>
        /// <returns>The <see cref="ReferenceHub"/> component of the newly created player instance.</returns>
        public static ReferenceHub GetNewPlayer()
            => UnityEngine.Object.Instantiate(PlayerPrefab).GetComponent<ReferenceHub>();

        /// <summary>
        /// Spawns a new door.
        /// </summary>
        /// <param name="doorType">The type of the door to spawn.</param>
        /// <param name="position">The position to spawn the door at.</param>
        /// <param name="scale">The scale of the door.</param>
        /// <param name="rotation">The rotation of the door.</param>
        /// <param name="name">The name to set to the door.</param>
        /// <param name="shouldSpawn">Whether or not to spawn the door.</param>
        /// <returns>The newly created <see cref="DoorVariant"/> instance of the spawned door.</returns>
        /// <exception cref="Exception"></exception>
        public static DoorVariant SpawnDoor(PrefabType doorType, Vector3 position, Vector3 scale, Quaternion rotation, string name = null, bool shouldSpawn = true)
        {
            if (!_prefabDoors.TryGetValue(doorType, out var doorPrefab))
                throw new Exception($"Failed to get a door prefab for type {doorType}");

            var doorInstance = UnityEngine.Object.Instantiate(doorPrefab);

            doorInstance.transform.position = position;
            doorInstance.transform.rotation = rotation;
            doorInstance.transform.localScale = scale;

            var room = RoomIdUtils.RoomAtPosition(position);

            if (room != null && room.ApiRoom != null)
                Facility.RegisterDoor(room.ApiRoom, doorInstance);

            if (!string.IsNullOrWhiteSpace(name))
                (doorInstance.GetComponent<DoorNametagExtension>() ?? doorInstance.gameObject.AddComponent<DoorNametagExtension>()).UpdateName(name);

            if (shouldSpawn)
                NetworkServer.Spawn(doorInstance.gameObject);

            return doorInstance;
        }

        /// <summary>
        /// Gets a prefab.
        /// </summary>
        /// <param name="type">The type of the prefab to get.</param>
        /// <param name="prefabObject">The instance of the prefab if found, otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the prefab was found, otherwise <see langword="false"/>.</returns>
        public static bool TryGetPrefab(PrefabType type, out GameObject prefabObject)
        {
            if (_prefabObjects.Count == 0)
                ReloadPrefabs();

            return _prefabObjects.TryGetValue(type, out prefabObject);
        }

        /// <summary>
        /// Reloads all prefabs <b>(except for door prefabs, those will be loaded once the round restarts!)</b>
        /// </summary>
        public static void ReloadPrefabs()
        {
            if (_prefabNames.Count == 0)
                SetPrefabNames();

            _prefabDoors.Clear();
            _prefabObjects.Clear();

            _prefabObjects[PrefabType.Player] = NetworkManager.singleton.playerPrefab;

            foreach (var prefab in NetworkClient.prefabs.Values)
            {
                if (prefab is null || string.IsNullOrWhiteSpace(prefab.name))
                    continue;

                if (prefab.name.Contains("Door"))
                    continue;

                if (!_prefabNames.TryGetKey(prefab.name, out var prefabType))
                {
                    ExLoader.Warn("Prefab API", $"Encountered an unknown prefab: &1{prefab.name}&r");
                    continue;
                }

                _prefabObjects[prefabType] = prefab;
            }

            foreach (var prefabName in _prefabNames.Keys)
            {
                if (_prefabObjects.ContainsKey(prefabName))
                    continue;

                ExLoader.Warn("Prefab API", $"Prefab &1{prefabName}&r has either been renamed or is missing.");
            }

            ExLoader.Info("Prefab API", $"Loaded &3{_prefabObjects.Count} / {_prefabNames.Count}&r prefabs!");
        }

        #region Prefab Names
        private static void SetPrefabNames()
        {
            _prefabNames[PrefabType.Player] = "Player";
            _prefabNames[PrefabType.AntiScp207] = "AntiSCP207Pickup";
            _prefabNames[PrefabType.Adrenaline] = "AdrenalinePrefab";
            _prefabNames[PrefabType.Ak] = "AkPickup";
            _prefabNames[PrefabType.A7] = "A7Pickup";
            _prefabNames[PrefabType.Ammo12ga] = "Ammo12gaPickup";
            _prefabNames[PrefabType.Ammo44cal] = "Ammo44calPickup";
            _prefabNames[PrefabType.Ammo556mm] = "Ammo556mmPickup";
            _prefabNames[PrefabType.Ammo762mm] = "Ammo762mmPickup";
            _prefabNames[PrefabType.Ammo9mm] = "Ammo9mmPickup";
            _prefabNames[PrefabType.ChaosKeycard] = "ChaosKeycardPickup";
            _prefabNames[PrefabType.Coin] = "CoinPickup";
            _prefabNames[PrefabType.Com15] = "Com15Pickup";
            _prefabNames[PrefabType.Com18] = "Com18Pickup";
            _prefabNames[PrefabType.Com45] = "Com45Pickup";
            _prefabNames[PrefabType.CombatArmor] = "Combat Armor Pickup";
            _prefabNames[PrefabType.Crossvec] = "CrossvecPickup";
            _prefabNames[PrefabType.Disruptor] = "DisruptorPickup";
            _prefabNames[PrefabType.Epsilon11SR] = "E11SRPickup";
            _prefabNames[PrefabType.FlashbangPickup] = "FlashbangPickup";
            _prefabNames[PrefabType.FlashbangProjectile] = "FlashbangProjectile";
            _prefabNames[PrefabType.Flashlight] = "FlashlightPickup";
            _prefabNames[PrefabType.Fsp9] = "Fsp9Pickup";
            _prefabNames[PrefabType.FrMg0] = "FRMG0Pickup";
            _prefabNames[PrefabType.HeavyArmor] = "Heavy Armor Pickup";
            _prefabNames[PrefabType.HegPickup] = "HegPickup";
            _prefabNames[PrefabType.HegProjectile] = "HegProjectile";
            _prefabNames[PrefabType.Jailbird] = "JailbirdPickup";
            _prefabNames[PrefabType.LightArmor] = "Light Armor Pickup";
            _prefabNames[PrefabType.Logicer] = "LogicerPickup";
            _prefabNames[PrefabType.Medkit] = "MedkitPickup";
            _prefabNames[PrefabType.MicroHid] = "MicroHidPickup";
            _prefabNames[PrefabType.Painkillers] = "PainkillersPickup";
            _prefabNames[PrefabType.Radio] = "RadioPickup";
            _prefabNames[PrefabType.RegularKeycard] = "RegularKeycardPickup";
            _prefabNames[PrefabType.Revolver] = "RevolverPickup";
            _prefabNames[PrefabType.Scp1576] = "SCP1576Pickup";
            _prefabNames[PrefabType.Scp1853] = "SCP1853Pickup";
            _prefabNames[PrefabType.Scp207] = "SCP207Pickup";
            _prefabNames[PrefabType.Scp244a] = "SCP244APickup Variant";
            _prefabNames[PrefabType.Scp244b] = "SCP244BPickup Variant";
            _prefabNames[PrefabType.Scp268] = "SCP268Pickup";
            _prefabNames[PrefabType.Scp500] = "SCP500Pickup";
            _prefabNames[PrefabType.Scp018] = "Scp018Projectile";
            _prefabNames[PrefabType.Scp2176] = "Scp2176Projectile";
            _prefabNames[PrefabType.Scp330] = "Scp330Pickup";
            _prefabNames[PrefabType.Shotgun] = "ShotgunPickup";
            _prefabNames[PrefabType.HealthBox] = "AdrenalineMedkitStructure";
            _prefabNames[PrefabType.Generator] = "GeneratorStructure";
            _prefabNames[PrefabType.LargeGunLocker] = "LargeGunLockerStructure";
            _prefabNames[PrefabType.MiscLocker] = "MiscLocker";
            _prefabNames[PrefabType.MedkitBox] = "RegularMedkitStructure";
            _prefabNames[PrefabType.RifleRack] = "RifleRackStructure";
            _prefabNames[PrefabType.Scp018Pedestal] = "Scp018PedestalStructure Variant";
            _prefabNames[PrefabType.Scp1853Pedestal] = "Scp1853PedestalStructure Variant";
            _prefabNames[PrefabType.Scp207Pedestal] = "Scp207PedestalStructure Variant";
            _prefabNames[PrefabType.Scp2176Pedestal] = "Scp2176PedestalStructure Variant";
            _prefabNames[PrefabType.Scp244Pedestal] = "Scp244PedestalStructure Variant";
            _prefabNames[PrefabType.Scp268Pedestal] = "Scp268PedestalStructure Variant";
            _prefabNames[PrefabType.Scp500Pedestal] = "Scp500PedestalStructure Variant";
            _prefabNames[PrefabType.Scp1576Pedestal] = "Scp1576PedestalStructure Variant";
            _prefabNames[PrefabType.AntiScp207Pedestal] = "AntiScp207PedestalStructure Variant";
            _prefabNames[PrefabType.AmnesticCloud] = "Amnestic Cloud Hazard";
            _prefabNames[PrefabType.WorkStation] = "Spawnable Work Station Structure";
            _prefabNames[PrefabType.Tantrum] = "TantrumObj";
            _prefabNames[PrefabType.SportTarget] = "sportTargetPrefab";
            _prefabNames[PrefabType.ClassDTarget] = "dboyTargetPrefab";
            _prefabNames[PrefabType.BinaryTarget] = "binaryTargetPrefab";
            _prefabNames[PrefabType.PrimitiveObject] = "PrimitiveObjectToy";
            _prefabNames[PrefabType.LightSource] = "LightSourceToy";
            _prefabNames[PrefabType.Lantern] = "LanternPickup";
            _prefabNames[PrefabType.Scp3114Ragdoll] = "Scp3114_Ragdoll";
            _prefabNames[PrefabType.Ragdoll1] = "Ragdoll_1";
            _prefabNames[PrefabType.Ragdoll4] = "Ragdoll_4";
            _prefabNames[PrefabType.Ragdoll6] = "Ragdoll_6";
            _prefabNames[PrefabType.Ragdoll7] = "Ragdoll_7";
            _prefabNames[PrefabType.Ragdoll8] = "Ragdoll_8";
            _prefabNames[PrefabType.Ragdoll10] = "Ragdoll_10";
            _prefabNames[PrefabType.Ragdoll12] = "Ragdoll_12";
            _prefabNames[PrefabType.Scp096Ragdoll] = "SCP-096_Ragdoll";
            _prefabNames[PrefabType.Scp106Ragdoll] = "SCP-106_Ragdoll";
            _prefabNames[PrefabType.Scp173Ragdoll] = "SCP-173_Ragdoll";
            _prefabNames[PrefabType.Scp939Ragdoll] = "SCP-939_Ragdoll";
            _prefabNames[PrefabType.TutorialRagdoll] = "Ragdoll_Tut";
        }
        #endregion

        [HarmonyPatch(typeof(DoorSpawnpoint), nameof(DoorSpawnpoint.SetupAllDoors))]
        private static bool Prefix()
        {
            _prefabDoors.Clear();

            foreach (var spawnpoint in DoorSpawnpoint.AllInstances)
            {
                if (spawnpoint.TargetPrefab is null)
                    continue;

                var prefabType = spawnpoint.TargetPrefab.name switch
                {
                    "LCZ BreakableDoor" => PrefabType.LightContainmentZoneDoor,
                    "HCZ BreakableDoor" => PrefabType.HeavyContainmentZoneDoor,
                    "EZ BreakableDoor" => PrefabType.EntranceZoneDoor,

                    _ => throw new ArgumentException($"Unknown door prefab: {spawnpoint.TargetPrefab.name}")
                };

                if (!_prefabDoors.ContainsKey(prefabType))
                    _prefabDoors.Add(prefabType, spawnpoint.TargetPrefab);
            }

            return true;
        }
    }
}
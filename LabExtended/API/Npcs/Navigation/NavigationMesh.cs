using LabExtended.API.Npcs.Navigation.API;
using LabExtended.Core;
using LabExtended.Extensions;

using UnityEngine;
using UnityEngine.AI;

namespace LabExtended.API.Npcs.Navigation
{
    /// <summary>
    /// Class used to manage navigation meshes.
    /// </summary>
    public static class NavigationMesh
    {
        private static NavMeshSurface _lightZone;
        private static NavMeshSurface _heavyZone;
        private static NavMeshSurface _entranceZone;
        private static NavMeshSurface _outsideZone;

        /// <summary>
        /// Whether or not all meshes have succesfully baked.
        /// </summary>
        public static bool IsBaked { get; internal set; }

        /// <summary>
        /// Gets the LCZ object.
        /// </summary>
        public static GameObject LightRoomsObject => GameObject.Find("LightRooms");

        /// <summary>
        /// Gets the HCZ object.
        /// </summary>
        public static GameObject HeavyRoomsObject => GameObject.Find("HeavyRooms");

        /// <summary>
        /// Gets the EZ object.
        /// </summary>
        public static GameObject EntranceRoomsObject => GameObject.Find("EntranceRooms");

        /// <summary>
        /// Gets the Surface object.
        /// </summary>
        public static GameObject OutsideObject => GameObject.Find("Outside");

        /// <summary>
        /// Gets the LCZ mesh.
        /// </summary>
        public static NavMeshSurface LightRoomsMesh => _lightZone;

        /// <summary>
        /// Gets the HCZ mesh.
        /// </summary>
        public static NavMeshSurface HeavyRoomsMesh => _heavyZone;

        /// <summary>
        /// Gets the EZ mesh.
        /// </summary>
        public static NavMeshSurface EntranceRoomsMesh => _entranceZone;

        /// <summary>
        /// Gets the Surface mesh.
        /// </summary>
        public static NavMeshSurface OutsideZoneMesh => _outsideZone;

        /// <summary>
        /// Bakes all meshes.
        /// </summary>
        public static void Prepare()
        {
            try
            {
                if (IsBaked)
                    return;

                var start = DateTime.Now;

                ApiLoader.Info("Navigation API", $"Baking navigation meshes ..");

                _lightZone = BakeMesh(LightRoomsObject);
                _heavyZone = BakeMesh(HeavyRoomsObject);
                _entranceZone = BakeMesh(EntranceRoomsObject);
                _outsideZone = BakeMesh(OutsideObject);

                IsBaked = true;

                ApiLoader.Info("Navigation API", $"Finished baking meshes in &3{(DateTime.Now - start).TotalMilliseconds} ms&r!");
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Navigation API", $"An error ocurred while baking navigation meshes!\n{ex.ToColoredString()}");
            }
        }

        // Gets called on waiting.
        internal static void Reset()
        {
            if (!IsBaked)
                return;

            IsBaked = false;

            try
            {
                UnityEngine.Object.Destroy(_lightZone);
                UnityEngine.Object.Destroy(_heavyZone);
                UnityEngine.Object.Destroy(_entranceZone);
                UnityEngine.Object.Destroy(_outsideZone);
            }
            catch { }
        }

        private static NavMeshSurface BakeMesh(GameObject gameObject)
        {
            var surface = gameObject.AddComponent<NavMeshSurface>();

            if (surface is null)
                throw new Exception($"An error occured while adding NavMeshSurface component to {gameObject.name}");

            surface.layerMask = new LayerMask() { value = 305624887 };
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.voxelSize = 0.08f;
            surface.BuildNavMesh();

            return surface;
        }
    }
}

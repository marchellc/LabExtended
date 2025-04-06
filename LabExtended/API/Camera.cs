using LabExtended.API.Wrappers;
using LabExtended.Attributes;
using LabExtended.Events;

using MapGeneration;

using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Cameras;

using UnityEngine;

using CameraType = LabExtended.API.Enums.CameraType;

namespace LabExtended.API
{
    /// <summary>
    /// Represents an in-game SCP-079 camera.
    /// </summary>
    public class Camera : Wrapper<Scp079Camera>
    {
        #region Types

        private static readonly Dictionary<string, CameraType> _cameraTypes = new()
        {
            ["CHKPT (EZ HALL)"] = CameraType.EzChkptHall,

            ["EZ CROSSING"] = CameraType.EzCrossing,
            ["EZ CURVE"] = CameraType.EzCurve,
            ["EZ HALLWAY"] = CameraType.EzHallway,
            ["EZ THREE-WAY"] = CameraType.EzThreeWay,

            ["GATE A"] = CameraType.EzGateA,
            ["GATE B"] = CameraType.EzGateB,

            ["INTERCOM BOTTOM"] = CameraType.EzIntercomBottom,
            ["INTERCOM HALL"] = CameraType.EzIntercomHall,
            ["INTERCOM PANEL"] = CameraType.EzIntercomPanel,
            ["INTERCOM STAIRS"] = CameraType.EzIntercomStairs,

            ["LARGE OFFICE"] = CameraType.EzLargeOffice,
            ["LOADING DOCK"] = CameraType.EzLoadingDock,
            ["MINOR OFFICE"] = CameraType.EzMinorOffice,
            ["TWO-STORY OFFICE"] = CameraType.EzTwoStoryOffice,

            ["049 OUTSIDE"] = CameraType.Hcz049Outside,
            ["049 CONT CHAMBER"] = CameraType.Hcz049ContChamber,
            ["049/173 TOP"] = CameraType.Hcz049ElevTop,
            ["049 HALLWAY"] = CameraType.Hcz049Hallway,

            ["173 OUTSIDE"] = CameraType.Hcz173Outside,

            ["049/173 BOTTOM"] = CameraType.Hcz049TopFloor,

            ["079 AIRLOCK"] = CameraType.Hcz079Airlock,
            ["079 CONT CHAMBER"] = CameraType.Hcz079ContChamber,
            ["079 HALLWAY"] = CameraType.Hcz079Hallway,
            ["079 KILL SWITCH"] = CameraType.Hcz079KillSwitch,

            ["096 CONT CHAMBER"] = CameraType.Hcz096ContChamber,

            ["106 BRIDGE"] = CameraType.Hcz106Bridge,
            ["106 CATWALK"] = CameraType.Hcz106Catwalk,
            ["106 RECONTAINMENT"] = CameraType.Hcz106Recontainment,

            ["CHKPT (EZ)"] = CameraType.HczChkptEz,
            ["CHKPT (HCZ)"] = CameraType.HczChkptHcz,

            ["H.I.D. CHAMBER"] = CameraType.HczHIDChamber,
            ["H.I.D. HALLWAY"] = CameraType.HczHIDHallway,

            ["HCZ 939"] = CameraType.Hcz939,

            ["HCZ ARMORY"] = CameraType.HczArmory,
            ["HCZ ARMORY INTERIOR"] = CameraType.HczArmoryInterior,

            ["HCZ CROSSING"] = CameraType.HczCrossing,
            ["HCZ CURVE"] = CameraType.HczCurve,

            ["HCZ ELEV SYS A"] = CameraType.HczElevSysA,
            ["HCZ ELEV SYS B"] = CameraType.HczElevSysB,

            ["HCZ HALLWAY"] = CameraType.HczHallway,
            ["HCZ THREE-WAY"] = CameraType.HczThreeWay,

            ["SERVERS BOTTOM"] = CameraType.HczServersBottom,
            ["SERVERS STAIRS"] = CameraType.HczServersStairs,
            ["SERVERS TOP"] = CameraType.HczServersTop,

            ["TESLA GATE"] = CameraType.HczTeslaGate,

            ["TESTROOM BRIDGE"] = CameraType.HczTestroomBridge,
            ["TESTROOM MAIN"] = CameraType.HczTestroomMain,
            ["TESTROOM OFFICE"] = CameraType.HczTestroomOffice,

            ["WARHEAD ARMORY"] = CameraType.HczWarheadArmory,
            ["WARHEAD CONTROL"] = CameraType.HczWarheadControl,
            ["WARHEAD HALLWAY"] = CameraType.HczWarheadHallway,
            ["WARHEAD TOP"] = CameraType.HczWarheadTop,

            ["173 BOTTOM"] = CameraType.Lcz173Bottom,
            ["173 HALL"] = CameraType.Lcz173Hall,

            ["914 AIRLOCK"] = CameraType.Lcz914Airlock,
            ["914 CONT CHAMBER"] = CameraType.Lcz914ContChamber,

            ["AIRLOCK"] = CameraType.LczAirlock,
            ["ARMORY"] = CameraType.LczArmory,

            ["CELLBLOCK BACK"] = CameraType.LczCellblockBack,
            ["CELLBLOCK ENTRY"] = CameraType.LczCellblockEntry,

            ["CHKPT A ENTRY"] = CameraType.LczChkptAEntry,
            ["CHKPT A INNER"] = CameraType.LczChkptAInner,

            ["CHKPT B ENTRY"] = CameraType.LczChkptBEntry,
            ["CHKPT B INNER"] = CameraType.LczChkptBInner,

            ["GLASSROOM"] = CameraType.LczGlassroom,
            ["GLASSROOM ENTRY"] = CameraType.LczGlassroomEntry,

            ["GREENHOUSE"] = CameraType.LczGreenhouse,

            ["LCZ CROSSING"] = CameraType.LczCrossing,
            ["LCZ CURVE"] = CameraType.LczCurve,

            ["LCZ ELEV SYS A"] = CameraType.LczElevSysA,
            ["LCZ ELEV SYS B"] = CameraType.LczElevSysB,

            ["LCZ HALLWAY"] = CameraType.LczHallway,
            ["LCZ THREE-WAY"] = CameraType.LczThreeWay,

            ["PC OFFICE"] = CameraType.LczPcOffice,
            ["RESTROOMS"] = CameraType.LczRestrooms,
            ["TC HALLWAY"] = CameraType.LczTcHallway,
            ["TEST CHAMBER"] = CameraType.LczTestChamber,

            ["EXIT PASSAGE"] = CameraType.ExitPassage,

            ["GATE A SURFACE"] = CameraType.GateASurface,
            ["GATE B SURFACE"] = CameraType.GateBSurface,

            ["MAIN STREET"] = CameraType.MainStreet,

            ["SURFACE AIRLOCK"] = CameraType.SurfaceAirlock,
            ["SURFACE BRIDGE"] = CameraType.SurfaceBridge,

            ["TUNNEL ENTRANCE"] = CameraType.TunnelEntrance,
        };
        #endregion

        /// <summary>
        /// Gets a list of all SCP-079 cameras.
        /// </summary>
        public static Dictionary<Scp079Camera, Camera> Lookup { get; } = new();

        /// <summary>
        /// Tries to find a wrapper by its base object.
        /// </summary>
        /// <param name="camera">The base object.</param>
        /// <param name="wrapper">The found wrapper instance.</param>
        /// <returns>true if the wrapper was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(Scp079Camera camera, out Camera wrapper)
        {
            if (camera is null)
                throw new ArgumentNullException(nameof(camera));
            
            return Lookup.TryGetValue(camera, out wrapper);
        }

        /// <summary>
        /// Tries to find a specific wrapper.
        /// </summary>
        /// <param name="predicate">The predicate used to search.</param>
        /// <param name="wrapper">The found wrapper instance.</param>
        /// <returns>true if the wrapper was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(Func<Camera, bool> predicate, out Camera? wrapper)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var pair in Lookup)
            {
                if (!predicate(pair.Value))
                    continue;
                
                wrapper = pair.Value;
                return true;
            }

            wrapper = null;
            return false;
        }

        /// <summary>
        /// Gets a wrapper by it's base object.
        /// </summary>
        /// <param name="camera">The base object.</param>
        /// <returns>The found wrapper instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static Camera Get(Scp079Camera camera)
        {
            if (camera is null)
                throw new ArgumentNullException(nameof(camera));

            if (!Lookup.TryGetValue(camera, out var wrapper))
                throw new KeyNotFoundException($"Could not find a camera of ID {camera.SyncId}");

            return wrapper;
        }

        /// <summary>
        /// Gets a wrapper by a predicate.
        /// </summary>
        /// <param name="predicate">The predicate used to search.</param>
        /// <returns>The found wrapper instance if found, otherwise null.</returns>
        /// <exception cref="Exception"></exception>
        public static Camera? Get(Func<Camera, bool> predicate)
            => TryGet(predicate, out var wrapper) ? wrapper : null;
        
        internal Camera(Scp079Camera baseValue) : base(baseValue)
        {
            if (_cameraTypes.TryGetValue(baseValue.Label, out var type))
            {
                Type = type;
                return;
            }

            if (baseValue.Room != null)
            {
                Type = baseValue.Room.Name switch
                {
                    RoomName.Hcz049 => baseValue.Label switch
                    {
                        "173 STAIRS" => CameraType.Hcz173Stairs,
                        "173 CONT CHAMBER" => CameraType.Hcz173ContChamber,

                        _ => CameraType.Unknown,
                    },

                    RoomName.Lcz173 => Name switch
                    {
                        "173 STAIRS" => CameraType.Lcz173Stairs,
                        "173 CONT CHAMBER" => CameraType.Lcz173ContChamber,

                        _ => CameraType.Unknown,
                    },

                    _ => CameraType.Unknown,
                };
            }
            else
            {
                Type = CameraType.Unknown;
            }
        }

        /// <summary>
        /// Gets the camera's game object.
        /// </summary>
        public GameObject GameObject => Base.gameObject;

        /// <summary>
        /// Gets the camera's transform.
        /// </summary>
        public Transform Transform => Base.transform;
        
        /// <summary>
        /// Gets the camera's anchor transform.
        /// </summary>
        public Transform Anchor => Base.CameraAnchor;
        
        /// <summary>
        /// Gets the camera's pivot transform.
        /// </summary>
        public Transform Pivot => Base.HorizontalAxis._pivot;

        /// <summary>
        /// Gets the camera's current room.
        /// </summary>
        public RoomIdentifier Room => Base.Room;
        
        /// <summary>
        /// Gets the player that is currently using this camera.
        /// </summary>
        public ExPlayer User => ExPlayer.Players.FirstOrDefault(x => x.Role.Is<Scp079Role>(out var scp) 
                                                                     && scp.CurrentCamera != null && scp.CurrentCamera == Base);

        /// <summary>
        /// Gets the camera's name.
        /// </summary>
        public string Name => Base.name;

        /// <summary>
        /// Gets the camera's synchronization ID.
        /// </summary>
        public ushort Id => Base.SyncId;

        /// <summary>
        /// Gets the camera's current zoom.
        /// </summary>
        public float Zoom => Base.ZoomAxis.CurValue;

        /// <summary>
        /// Gets the camera's current vertical axis.
        /// </summary>
        public float VerticalAxis => Base.VerticalAxis.CurValue;
        
        /// <summary>
        /// Gets the camera's current horizontal axis.
        /// </summary>
        public float HorizontalAxis => Base.HorizontalAxis.CurValue;

        /// <summary>
        /// Whether or not this camera is currently being used.
        /// </summary>
        public bool IsUsed => Base.IsActive;

        /// <summary>
        /// Gets the camera's position.
        /// </summary>
        public Vector3 Position => Base.transform.position;

        /// <summary>
        /// Gets the camera's rotation.
        /// </summary>
        public Quaternion Rotation => Base.transform.rotation;
        
        /// <summary>
        /// Gets the camera's anchor rotation.
        /// </summary>
        public Quaternion AnchorRotation => Anchor.rotation;
        
        /// <summary>
        /// Gets the camera's pivot rotation.
        /// </summary>
        public Quaternion PivotRotation => Pivot.localRotation;

        /// <summary>
        /// Gets the facility zone that this camera is located in.
        /// </summary>
        public FacilityZone Zone => Room?.Zone ?? FacilityZone.None;
        
        /// <summary>
        /// Gets the name of the room that this camera is located in.
        /// </summary>
        public RoomName RoomName => Room?.Name ?? RoomName.Unnamed;

        /// <summary>
        /// Gets the type of this camera.
        /// </summary>
        public CameraType Type { get; }
        
        // BROKEN

        /*
        public void LookAt(Transform transform)
            => LookAt(transform.position);

        public void LookAt(GameObject gameObject)
            => LookAt(gameObject.transform.position);

        public void LookAt(MonoBehaviour behaviour)
            => LookAt(behaviour.transform.position);

        public void LookAt(ExPlayer player)
            => LookAt(player.CameraTransform.position);

        public void LookAt(Vector3 position)
            => SetRotation(Quaternion.LookRotation(position - Base._cameraAnchor.position));

        public void SetRotation(Quaternion rotation)
            => SetRotation(rotation.eulerAngles);

        public void SetRotation(Vector3 eulerRotation)
        {
            var cameraRotation = Base.transform.rotation.eulerAngles;

            float vertical = (eulerRotation.x - cameraRotation.x + 360f) % 360f;
            float horizontal = (eulerRotation.y - cameraRotation.y + 360f) % 360f;

            if (vertical > 180f)
                vertical -= 360f;

            if (horizontal > 180f)
                horizontal -= 360f;

            SetRotation(horizontal, vertical);
        }

        public void SetRotation(float horizontal, float vertical)
        {
            if (_rotSync != null && _camSync != null)
            {
                if (_camSync.CurrentCamera is null || _camSync.CurrentCamera != Base)
                {
                    _camSync._clientSwitchRequest = Scp079CurrentCameraSync.ClientSwitchState.None;
                    _camSync._errorCode = Scp079HudTranslation.Zoom;

                    _camSync.CurrentCamera = Base;
                }

                Base.VerticalAxis.TargetValue = vertical;
                Base.HorizontalAxis.TargetValue = horizontal;

                Base.IsActive = true;
                Base.Update();

                _rotSync.ServerSendRpc(true);
            }
        }
        */

        private static void OnWaiting()
        {
            foreach (var interactable in Scp079InteractableBase.AllInstances)
            {
                if (interactable is not Scp079Camera camera)
                    continue;
                
                Lookup.Add(camera, new(camera));
            }
        }

        [LoaderInitialize(1)]
        private static void OnInit()
        {
            InternalEvents.OnRoundRestart += Lookup.Clear;
            InternalEvents.OnRoundWaiting += OnWaiting;
        }
    }
}
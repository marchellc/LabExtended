using LabExtended.API.Collections.Locked;
using LabExtended.API.Npcs;

using LabExtended.Core;
using LabExtended.Utilities;

using MapGeneration;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using PlayerRoles.PlayableScps.Scp079.GUI;

using UnityEngine;

using CameraType = LabExtended.API.Enums.CameraType;

namespace LabExtended.API
{
    public class Camera : Wrapper<Scp079Camera>
    {
        #region Types
        private static readonly LockedDictionary<string, CameraType> _cameraTypes = new LockedDictionary<string, CameraType>()
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

        internal static readonly LockedDictionary<Scp079Camera, Camera> _wrappers = new LockedDictionary<Scp079Camera, Camera>();

        internal static NpcHandler _camNpc;

        internal float? _verticalValue;
        internal float? _horizontalValue;

        internal static Scp079CameraRotationSync _rotSync;
        internal static Scp079CurrentCameraSync _camSync;

        public Camera(Scp079Camera baseValue) : base(baseValue)
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

        public GameObject GameObject => Base.gameObject;
        public Transform Transform => Base.transform;

        public RoomIdentifier Room => Base.Room;
        public ExPlayer User => ExPlayer.Players.FirstOrDefault(x => x.Role.Is<Scp079Role>(out var scp) && scp.CurrentCamera != null && scp.CurrentCamera == Base);

        public string Name => Base.name;

        public ushort Id => Base.SyncId;

        public Vector3 Position => Base.CameraPosition;

        public FacilityZone Zone => Room?.Zone ?? FacilityZone.None;
        public RoomName RoomName => Room?.Name ?? RoomName.Unnamed;

        public CameraType Type { get; }

        public bool IsUsed
        {
            get => Base.IsActive;
            set => Base.IsActive = value;
        }

        public float Zoom
        {
            get => Base.ZoomAxis.CurValue;
            set => SetZoom(value);
        }

        public Quaternion Rotation
        {
            get => Base._cameraAnchor.rotation;
            set => SetRotation(value.eulerAngles);
        }

        public void LookAt(Transform transform)
            => LookAt(transform.position);

        public void LookAt(GameObject gameObject)
            => LookAt(gameObject.transform.position);

        public void LookAt(MonoBehaviour behaviour)
            => LookAt(behaviour.transform.position);

        public void LookAt(ExPlayer player)
            => LookAt(player.Position);

        public void LookAt(Vector3 position)
            => SetRotation(RotationUtils.LookAt(Position - position, Vector3.up));

        #region Helper Methods
        private void SetZoom(float zoom)
        {
            if (_rotSync != null && _camSync != null)
            {
                if (_camSync.CurrentCamera is null || _camSync.CurrentCamera != Base)
                {
                    ExLoader.Debug("Camera API", $"Setting current camera to {Name} ({Id} - {RoomName})");

                    _camSync._auxManager.CurrentAux = _camSync._auxManager.MaxAux;
                    _camSync.ClientSwitchTo(Base);

                    ExLoader.Debug("Camera API", $"Switch result: error={_camSync._errorCode} req={_camSync._clientSwitchRequest}");
                }

                var prev = Base.ZoomAxis.TargetValue;

                Scp079CursorManager.LockCameras = false;

                Base.IsActive = true;
                Base.ZoomAxis.TargetValue = zoom;

                Base.Update();

                ExLoader.Debug("Camera API", $"Setting ROTATION to {zoom} (prev: {prev})");

                _rotSync.ServerSendRpc(true);
            }
        }

        private void SetRotation(Vector3 value)
        {
            if (_rotSync != null && _camSync != null)
            {
                if (_camSync.CurrentCamera is null || _camSync.CurrentCamera != Base)
                {
                    ExLoader.Debug("Camera API", $"Setting current camera to {Name} ({Id} - {RoomName})");

                    _camSync._auxManager.CurrentAux = _camSync._auxManager.MaxAux;
                    _camSync.ClientSwitchTo(Base);

                    ExLoader.Debug("Camera API", $"Switch result: error={_camSync._errorCode} req={_camSync._clientSwitchRequest}");
                }

                var prevH = Base.HorizontalAxis.TargetValue;
                var prevV = Base.VerticalAxis.TargetValue;

                Scp079CursorManager.LockCameras = false;

                Base.IsActive = true;

                Base.HorizontalAxis.TargetValue = value.y;
                Base.VerticalAxis.TargetValue = value.x;

                _horizontalValue = value.y;
                _verticalValue = value.x;

                Base.Update();

                ExLoader.Debug("Camera API", $"Setting ROTATION to {value.y}/{value.x} (prev: {prevH}/{prevV})");

                _rotSync.ServerSendRpc(true);

                _horizontalValue = null;
                _verticalValue = null;
            }
        }
        #endregion

        #region Static Methods
        internal static void OnRestart()
        {
            _rotSync = null;
            _camNpc = null;
        }

        internal static void OnRoundStart()
            => NpcHandler.Spawn("Camera NPC", RoleTypeId.Scp079, null, null, null, npc =>
            {
                _camNpc = npc;

                _rotSync = npc.Player.Role.GetRoutine<Scp079CameraRotationSync>();
                _camSync = npc.Player.Role.GetRoutine<Scp079CurrentCameraSync>();

                npc.Player.Switches.IsVisibleInRemoteAdmin = false;
                npc.Player.Switches.IsVisibleInSpectatorList = false;

                ExLoader.Debug("Camera API", $"Spawned camera NPC");
            });
        #endregion
    }
}
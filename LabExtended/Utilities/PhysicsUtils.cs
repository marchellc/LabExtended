using UnityEngine;

using PlayerRoles.FirstPersonControl;

using LabExtended.API;

namespace LabExtended.Utilities;

/// <summary>
/// Utilities related to Unity Engine physics.
/// </summary>
public static class PhysicsUtils
{
    /// <summary>
    /// The layer mask for player collisions.
    /// </summary>
    public static readonly LayerMask PlayerCollisionMask =
        LayerMask.GetMask("Default", "Glass", "InvisibleCollider", "Door");

    /// <summary>
    /// The layer mask for collisions with visible objects.
    /// </summary>
    public static readonly LayerMask VisibleMask =
        LayerMask.GetMask("Default", "Hitbox", "CCTV", "Door", "InteractableNoPlayerCollision");

    #region GroundPosition
    public static bool TryGetGroundPosition(ExPlayer player, out Vector3 groundPosition, bool mustBeGrounded = true)
        => TryGetGroundPosition(player.ReferenceHub, out groundPosition, mustBeGrounded);

    public static bool TryGetGroundPosition(ReferenceHub hub, out Vector3 groundPosition, bool mustBeGrounded = true)
    {
        groundPosition = Vector3.zero;

        if (!(hub.roleManager.CurrentRole is IFpcRole fpcRole))
            return false;

        var controller = fpcRole.FpcModule.CharController;

        if (mustBeGrounded && !controller.isGrounded)
            return false;

        Vector3 castOrigin = fpcRole.FpcModule.Position; // Always 0.96f above ground, on horizontal plane
        castOrigin.y -= 0.54f; // Constant, hopefully never changes
        float castDistance = 0.7f;

        if (!Physics.SphereCast(castOrigin, controller.radius, Vector3.down, out RaycastHit hit, castDistance,
                PlayerCollisionMask.value))
            return false;

        groundPosition = hit.point;
        return true;
    }

    #endregion

    #region LookingAt
    public static bool IsDirectlyLookingAtPlayer(ExPlayer player, out ExPlayer? target, float distance = 1000)
    {
        target = null;
        return IsDirectlyLookingAtPlayer(player.ReferenceHub, out var hub, distance) &&
               ExPlayer.TryGet(hub, out target);
    }

    public static bool IsDirectlyLookingAtPlayer(Vector3 position, Vector3 direction, out ExPlayer? target,
        float distance = 1000)
    {
        target = null;
        return IsDirectlyLookingAtPlayer(position, direction, out ReferenceHub hub, distance) &&
               ExPlayer.TryGet(hub, out target);
    }

    public static bool IsDirectlyLookingAtPlayer(ReferenceHub hub, out ReferenceHub target, float distance = 1000)
    {
        var cameraTransform = hub.PlayerCameraReference;

        SetHitboxes(hub, false);

        var result = IsDirectlyLookingAtPlayer(cameraTransform.position, cameraTransform.forward, out target, distance);

        SetHitboxes(hub, !hub.isLocalPlayer);
        return result;
    }

    public static bool IsDirectlyLookingAtPlayer(Vector3 position, Vector3 direction, out ReferenceHub target,
        float distance = 1000)
    {
        target = null;

        if (!Physics.Raycast(position, direction, out var hitInfo, distance, VisibleMask.value) ||
            !hitInfo.collider.TryGetComponent<IDestructible>(out var component) ||
            !(component is HitboxIdentity hitboxIdentity))
        {
            return false;
        }

        target = hitboxIdentity.TargetHub;
        return true;
    }

    #endregion

    #region Hitboxes
    public static void SetHitboxes(this ExPlayer target, bool state)
    {
        SetHitboxes(target.ReferenceHub, state);
    }

    public static void SetHitboxes(this ReferenceHub target, bool state)
    {
        if (target.roleManager.CurrentRole is IFpcRole fpcRole)
        {
            HitboxIdentity[] hitboxes = fpcRole.FpcModule.CharacterModelInstance.Hitboxes;
            for (int i = 0; i < hitboxes.Length; i++)
            {
                hitboxes[i].SetColliders(state);
            }
        }
    }

    #endregion
}
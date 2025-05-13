using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

using Mirror;

using PlayerStatsSystem;

using UnityEngine;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.API.Toys
{
    /// <summary>
    /// Wrapper for shooting targets.
    /// </summary>
    public class TargetToy : AdminToy, IWrapper<ShootingTarget>
    {
        /// <summary>
        /// Spawns a new shooting target.
        /// </summary>
        /// <param name="position">The target's position.</param>
        /// <param name="rotation">The target's rotation.</param>
        public TargetToy(Vector3? position = null, Quaternion? rotation = null)
            : base(PrefabList.BinaryTarget.CreateInstance().GetComponent<ShootingTarget>())
        {
            Base = base.Base as ShootingTarget;
            
            if (Base is null)
                throw new Exception("Could not spawn shooting target.");
            
            Base.SpawnerFootprint = ExPlayer.Host.Footprint;
            
            Base.NetworkPosition = position ?? Vector3.zero;
            Base.NetworkRotation = rotation ?? Quaternion.identity;
            
            Base.transform.SetPositionAndRotation(Base.NetworkPosition, Base.NetworkRotation);
            
            NetworkServer.Spawn(Base.gameObject);
        }
        
        internal TargetToy(ShootingTarget baseValue) : base(baseValue)
            => Base = baseValue;

        /// <summary>
        /// Gets the shooting target.
        /// </summary>
        public new ShootingTarget Base { get; }

        /// <summary>
        /// Gets the target's bulls eye position.
        /// </summary>
        public Vector3 BullsEye => Base.CenterOfMass;
        
        /// <summary>
        /// Gets the target's bulls eye bounds.
        /// </summary>
        public Vector3[] BullsEyeBounds => Base._bullsEyeBounds;

        /// <summary>
        /// Gets the target's bulls eye radius.
        /// </summary>
        public float BullsEyeRadius => Base._bullsEyeRadius;

        /// <summary>
        /// Gets or sets the target's health.
        /// </summary>
        public float Health
        {
            get => Base._hp;
            set => Base._hp = value;
        }

        /// <summary>
        /// Gets or sets the target's max health.
        /// </summary>
        public int MaxHealth
        {
            get => Base._maxHp;
            set => Base.RpcSendInfo(Base._maxHp = value, Base._autoDestroyTime);
        }

        /// <summary>
        /// Gets or sets the target's reset time.
        /// </summary>
        public int ResetTime
        {
            get => Base._autoDestroyTime;
            set => Base.RpcSendInfo(Base._maxHp, Base._autoDestroyTime = value);
        }

        /// <summary>
        /// Whether or not to sync values to non-attackers.
        /// </summary>
        public bool SyncToNonAttackers
        {
            get => Base.Network_syncMode;
            set => Base.Network_syncMode = value;
        }

        /// <summary>
        /// Damages the shooting target.
        /// </summary>
        /// <param name="damage">The amount of damage.</param>
        /// <param name="hitPosition">The position of the hit.</param>
        public void Damage(float damage, Vector3 hitPosition)
            => Damage(damage, ExPlayer.Host, hitPosition);

        /// <summary>
        /// Damages the shooting target.
        /// </summary>
        /// <param name="damage">The amount of damage.</param>
        /// <param name="attacker">The attacking player.</param>
        /// <param name="hitPosition">The position of the hit.</param>
        public void Damage(float damage, ExPlayer attacker, Vector3 hitPosition)
        {
            var wasSync = SyncToNonAttackers;

            attacker ??= ExPlayer.Host;

            if (attacker.IsServer && !SyncToNonAttackers)
                SyncToNonAttackers = true;

            Base.Damage(damage, new ExplosionDamageHandler(attacker.Footprint, Vector3.zero, damage, 0, ExplosionType.Grenade), hitPosition);

            if (SyncToNonAttackers != wasSync)
                SyncToNonAttackers = wasSync;
        }

        /// <summary>
        /// Damages the shooting target.
        /// </summary>
        /// <param name="damage">The amount of damage.</param>
        /// <param name="damageHandler">The damage handler.</param>
        /// <param name="hitPosition">The position of the hit.</param>
        public void Damage(float damage, AttackerDamageHandler damageHandler, Vector3 hitPosition)
            => Base.Damage(damage, damageHandler, hitPosition);

        /// <summary>
        /// Doubles the target's max health.
        /// </summary>
        public void IncreaseMaxHealth()
            => MaxHealth = Mathf.Clamp(MaxHealth * 2, 1, 255);

        /// <summary>
        /// Decreases the target's max health by half.
        /// </summary>
        public void DecreaseMaxHealth()
            => MaxHealth /= 2;

        /// <summary>
        /// Increases the target's reset time by a second.
        /// </summary>
        public void IncreaseResetTime()
            => ResetTime = Mathf.Min(ResetTime + 1, 10);

        /// <summary>
        /// Decreases the target's reset time by a second.
        /// </summary>
        public void DecreaseResetTime()
            => ResetTime = Mathf.Min(ResetTime - 1, 0);

        /// <summary>
        /// Resets the target.
        /// </summary>
        public void ClearTarget()
        {
            Base.ClearTarget();
            Base.RpcSendInfo(Base._maxHp, Base._autoDestroyTime);
        }

        /// <summary>
        /// Invokes an action on the target.
        /// </summary>
        /// <param name="button">The action to do.</param>
        public void Use(ShootingTarget.TargetButton button)
        {
            switch (button)
            {
                case ShootingTarget.TargetButton.DecreaseHP:
                    DecreaseMaxHealth();
                    break;

                case ShootingTarget.TargetButton.IncreaseHP:
                    IncreaseMaxHealth();
                    break;

                case ShootingTarget.TargetButton.IncreaseResetTime:
                    IncreaseResetTime();
                    break;

                case ShootingTarget.TargetButton.DecreaseResetTime:
                    DecreaseResetTime();
                    break;

                case ShootingTarget.TargetButton.ManualReset:
                    ClearTarget();
                    break;

                case ShootingTarget.TargetButton.Remove:
                    Delete();
                    break;

                case ShootingTarget.TargetButton.GlobalResults:
                    SyncToNonAttackers = !SyncToNonAttackers;
                    break;
            }
        }
    }
}
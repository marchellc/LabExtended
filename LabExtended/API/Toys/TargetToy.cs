using AdminToys;

using LabExtended.API.Interfaces;

using PlayerStatsSystem;

using UnityEngine;

namespace LabExtended.API.Toys
{
    public class TargetToy : AdminToy, IWrapper<ShootingTarget>
    {
        public TargetToy(ShootingTarget baseValue) : base(baseValue)
            => Base = baseValue;

        public new ShootingTarget Base { get; }

        public Vector3 BullsEye => Base.CenterOfMass;
        public Vector3[] BullsEyeBounds => Base._bullsEyeBounds;

        public float BullsEyeRadius => Base._bullsEyeRadius;

        public float Health
        {
            get => Base._hp;
            set => Base._hp = value;
        }

        public int MaxHealth
        {
            get => Base._maxHp;
            set => Base.RpcSendInfo(Base._maxHp = value, Base._autoDestroyTime);
        }

        public int ResetTime
        {
            get => Base._autoDestroyTime;
            set => Base.RpcSendInfo(Base._maxHp, Base._autoDestroyTime = value);
        }

        public bool SyncToNonAttackers
        {
            get => Base.Network_syncMode;
            set => Base.Network_syncMode = value;
        }

        public void Damage(float damage, Vector3 hitPosition)
            => Damage(damage, ExPlayer.Host, hitPosition);

        public void Damage(float damage, ExPlayer attacker, Vector3 hitPosition)
        {
            var wasSync = SyncToNonAttackers;

            attacker ??= ExPlayer.Host;

            if (attacker.IsServer && !SyncToNonAttackers)
                SyncToNonAttackers = true;

            Base.Damage(damage, new ExplosionDamageHandler(attacker.Footprint, Vector3.zero, damage, 0), hitPosition);

            if (SyncToNonAttackers != wasSync)
                SyncToNonAttackers = wasSync;
        }

        public void Damage(float damage, AttackerDamageHandler damageHandler, Vector3 hitPosition)
            => Base.Damage(damage, damageHandler, hitPosition);

        public void IncreaseMaxHealth()
            => MaxHealth = Mathf.Clamp(MaxHealth * 2, 1, 255);

        public void DecreaseMaxHealth()
            => MaxHealth /= 2;

        public void IncreaseResetTime()
            => ResetTime = Mathf.Min(ResetTime + 1, 10);

        public void DecreaseResetTime()
            => ResetTime = Mathf.Min(ResetTime - 1, 0);

        public void ClearTarget()
        {
            Base.ClearTarget();
            Base.RpcSendInfo(Base._maxHp, Base._autoDestroyTime);
        }

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
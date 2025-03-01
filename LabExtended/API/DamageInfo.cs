using LabExtended.API.Enums;
using LabExtended.Extensions;

using PlayerStatsSystem;

using UnityEngine;

namespace LabExtended.API
{
    public struct DamageInfo
    {
        public DamageInfo(StandardDamageHandler baseValue)
            => Base = baseValue;

        public StandardDamageHandler Base { get; set; }

        public bool IsScp096 => Type.IsScp096();
        public bool IsScp939 => Type.IsScp939();
        public bool IsScp3114 => Type.IsScp3114();

        public bool IsScp => Base is ScpDamageHandler;
        public bool IsAttacker => Base is AttackerDamageHandler attackerDamageHandler && attackerDamageHandler.Attacker.Hub != null;

        public bool UseHumanHitboxes => Base is FirearmDamageHandler firearmDamageHandler ? firearmDamageHandler._useHumanHitboxes : false;

        public float AbsorbedByAhp => Base.AbsorbedAhpDamage;
        public float AbsorbedByHumeShield => Base.AbsorbedHumeDamage;

        public float DealtDamage => Base.DealtHealthDamage;

        public float FirearmPenetration => Base is FirearmDamageHandler firearmDamageHandler ? firearmDamageHandler._penetration : 0f;

        public string Reason => Base.ServerLogsText;

        public DamageType Type => Base.GetDamageType();

        public Vector3 Scp018ImpactVelocity => Base is Scp018DamageHandler scp018DamageHandler ? scp018DamageHandler._ballImpactVelocity : Vector3.zero;
        public Vector3 JailbirdMoveDirection => Base is JailbirdDamageHandler jailbirdDamageHandler ? jailbirdDamageHandler._moveDirection : Vector3.zero;

        public DamageHandlerBase.CassieAnnouncement CassieAnnouncement => Base.CassieDeathAnnouncement;

        public string CustomReason
        {
            get => (Base as CustomReasonDamageHandler)?._deathReason ?? string.Empty;
            set => (Base as CustomReasonDamageHandler)!._deathReason = value;
        }

        public float Damage
        {
            get => Base.Damage;
            set => Base.Damage = value;
        }

        public HitboxType HitboxType
        {
            get => Base.Hitbox;
            set => Base.Hitbox = value;
        }

        public ItemType FirearmType
        {
            get => Base is FirearmDamageHandler firearmDamageHandler ? firearmDamageHandler.WeaponType : ItemType.None;
            set => (Base as FirearmDamageHandler)!.WeaponType = value;
        }

        public Vector3 Velocity
        {
            get => Base.StartVelocity;
            set => Base.StartVelocity = value;
        }

        public ExPlayer Attacker
        {
            get => Base is AttackerDamageHandler attackerDamageHandler ? ExPlayer.Get(attackerDamageHandler.Attacker) : null;
            set => (Base as AttackerDamageHandler).Attacker = value?.Footprint ?? ExPlayer.Host.Footprint;
        }
    }
}
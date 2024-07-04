﻿using Common.Values;

using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp106;

using PlayerStatsSystem;

namespace LabExtended.API
{
    public class PlayerStats
    {
        internal StatusValue<float> _maxHealthOverride;

        internal PlayerStats(PlayerStatsSystem.PlayerStats playerStats)
        {
            Stats = playerStats;

            Ahp = Get<AhpStat>();
            Vigor = Get<VigorStat>();
            Health = Get<HealthStat>();
            Stamina = Get<StaminaStat>();
            AdminFlags = Get<AdminFlagsStat>();
            HumeShield = Get<HumeShieldStat>();

            _maxHealthOverride = new StatusValue<float>(() => Health.CurValue);
        }

        public PlayerStatsSystem.PlayerStats Stats { get; }

        public StatBase[] AllStats => Stats._statModules;

        public AhpStat Ahp { get; }
        public VigorStat Vigor { get; }
        public HealthStat Health { get; }
        public StaminaStat Stamina { get; }
        public AdminFlagsStat AdminFlags { get; }
        public HumeShieldStat HumeShield { get; }

        public float MinAhp => Ahp.MinValue;
        public float MaxAhp => Ahp.MaxValue;

        public float MinVigor => Vigor.MinValue;
        public float MaxVigor => Vigor.MaxValue;

        public float MinStamina => Stamina.MinValue;
        public float MaxStamina => Stamina.MaxValue;

        public float MinHealth => Health.MinValue;

        public float MinHumeShield => HumeShield.MinValue;
        public float MaxHumeShield => HumeShield.MaxValue;

        public float CurAhp
        {
            get => Ahp.CurValue;
            set => Ahp.CurValue = value;
        }

        public float CurVigor
        {
            get => Vigor.CurValue;
            set => Vigor.CurValue = value;
        }

        public float CurStamina
        {
            get => Stamina.CurValue;
            set => Stamina.CurValue = value;
        }

        public float CurHealth
        {
            get => Health.CurValue;
            set => Health.CurValue = value;
        }

        public float CurHumeShield
        {
            get => HumeShield.CurValue;
            set => HumeShield.CurValue = value;
        }

        public float MaxHealth
        {
            get => _maxHealthOverride.Value;
            set => _maxHealthOverride.Value = value;
        }

        public bool KeepMaxHealthOnRoleChange { get; set; }

        public bool UsesStamina { get; set; } = true;

        public bool UsesHumeShield => Stats._hub.roleManager.CurrentRole is IHumeShieldedRole;

        public T Get<T>() where T : StatBase
            => Stats.GetModule<T>();

        public bool TryGet<T>(out T stat) where T : StatBase
            => Stats.TryGetModule<T>(out stat);
    }
}
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp106;

using PlayerStatsSystem;

namespace LabExtended.API.Containers
{
    public class StatsContainer
    {
        internal StatsContainer(PlayerStats playerStats)
        {
            Stats = playerStats;

            Ahp = Get<AhpStat>();
            Vigor = Get<VigorStat>();
            Health = Get<HealthStat>();
            Stamina = Get<StaminaStat>();
            AdminFlags = Get<AdminFlagsStat>();
            HumeShield = Get<HumeShieldStat>();
        }

        public PlayerStats Stats { get; }

        public StatBase[] AllStats => Stats._statModules;

        public AhpStat Ahp { get; }
        public VigorStat Vigor { get; }
        public HealthStat Health { get; }
        public StaminaStat Stamina { get; }
        public AdminFlagsStat AdminFlags { get; }
        public HumeShieldStat HumeShield { get; }

        public float MinAhp => Ahp.MinValue;
        public float MinVigor => Vigor.MinValue;
        public float MinHealth => Health.MinValue;
        public float MinStamina => Stamina.MinValue;
        public float MinHumeShield => HumeShield.MinValue;

        public float MaxAhp
        {
            get => Ahp.MaxValue;
            set => Ahp.MaxValue = value;
        }

        public float MaxVigor
        {
            get => Vigor.MaxValue;
            set => Vigor.MaxValue = value;
        }

        public float MaxHealth
        {
            get => Health.MaxValue;
            set => Health.MaxValue = value;
        }
        
        public float MaxStamina
        {
            get => Stamina.MaxValue;
            set => Stamina.MaxValue = value;
        }

        public float MaxHumeShield
        {
            get => HumeShield.MaxValue;
            set => HumeShield.MaxValue = value;
        }

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

        public bool UsesHumeShield => Stats._hub.roleManager.CurrentRole is IHumeShieldedRole;

        public T Get<T>() where T : StatBase
            => Stats.GetModule<T>();

        public bool TryGet<T>(out T stat) where T : StatBase
            => Stats.TryGetModule(out stat);
    }
}
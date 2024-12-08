using LabExtended.API.Enums;
using LabExtended.Core;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.PlayableScps.Scp939;

using PlayerStatsSystem;

namespace LabExtended.Extensions
{
    public static class DamageExtensions
    {
        public static readonly DamageType[] Translations = new DamageType[]
        {
            DamageType.Recontained,
            DamageType.Warhead,
            DamageType.Scp049,
            DamageType.Unknown,
            DamageType.Asphyxiated,
            DamageType.Bleeding,
            DamageType.Falldown,
            DamageType.PocketDecay,
            DamageType.Decontamination,
            DamageType.Poisoned,
            DamageType.Scp207,
            DamageType.SeveredHands,
            DamageType.MicroHid,
            DamageType.Tesla,
            DamageType.Explosion,
            DamageType.Scp096,
            DamageType.Scp173,
            DamageType.Scp939Lunge,
            DamageType.Zombie,
            DamageType.BulletWounds,
            DamageType.Crushed,
            DamageType.Scp106Bait,
            DamageType.FriendlyFireDetector,
            DamageType.Hypothermia,
            DamageType.CardiacArrest,
            DamageType.Scp939,
            DamageType.Scp3114,
            DamageType.MarshmallowMan
        };

        public static CustomReasonDamageHandler CreateCustomHandler(string customReason, float damage, string cassieAnnouncement = "")
            => new CustomReasonDamageHandler(customReason, damage, cassieAnnouncement);

        public static WarheadDamageHandler CreateWarheadHandler(float damage)
        {
            var handler = new WarheadDamageHandler();

            handler.Damage = damage;
            return handler;
        }

        public static UniversalDamageHandler CreateUniversalHandler(DamageType type, float damage)
            => new UniversalDamageHandler(damage, type.GetDeathTranslation());

        public static bool IsScp939(this DamageType damageType)
            => damageType is DamageType.Scp939 || damageType is DamageType.Scp939Claw || damageType is DamageType.Scp939Lunge;

        public static bool IsScp3114(this DamageType damageType)
            => damageType is DamageType.Scp3114 || damageType is DamageType.Scp3114Slap || damageType is DamageType.Scp3114Strangulation;

        public static bool IsScp096(this DamageType damageType)
            => damageType is DamageType.Scp096 || damageType is DamageType.Scp096Charge || damageType is DamageType.Scp096GateKill || damageType is DamageType.Scp096SlapLeft || damageType is DamageType.Scp096SlapRight;

        public static bool IsThrowable(this DamageType damageType)
            => damageType is DamageType.Explosion || damageType is DamageType.Scp018;

        public static DamageType GetDamageType(this DamageHandlerBase damageHandlerBase)
        {
            if (damageHandlerBase is null)
                return DamageType.Unknown;

            if (damageHandlerBase is CustomReasonDamageHandler)
                return DamageType.Custom;

            if (damageHandlerBase is DisruptorDamageHandler)
                return DamageType.Disruptor;

            if (damageHandlerBase is JailbirdDamageHandler)
                return DamageType.Jailbird;

            if (damageHandlerBase is MicroHidDamageHandler)
                return DamageType.MicroHid;

            if (damageHandlerBase is Scp018DamageHandler)
                return DamageType.Scp018;

            if (damageHandlerBase is ExplosionDamageHandler)
                return DamageType.Explosion;

            if (damageHandlerBase is FirearmDamageHandler)
                return DamageType.Firearm;

            if (damageHandlerBase is RecontainmentDamageHandler)
                return DamageType.Recontained;

            if (damageHandlerBase is WarheadDamageHandler)
                return DamageType.Warhead;

            if (damageHandlerBase is Scp049DamageHandler scp049DamageHandler)
            {
                if (scp049DamageHandler.DamageSubType is Scp049DamageHandler.AttackType.CardiacArrest)
                    return DamageType.CardiacArrest;

                if (scp049DamageHandler.DamageSubType is Scp049DamageHandler.AttackType.Instakill)
                    return DamageType.Scp049;

                if (scp049DamageHandler.DamageSubType is Scp049DamageHandler.AttackType.Scp0492)
                    return DamageType.Zombie;

                ApiLog.Warn("Damage API", $"Unknown SCP-049 damage handler: {scp049DamageHandler.DamageSubType}");
                return DamageType.Scp049;
            }

            if (damageHandlerBase is Scp096DamageHandler scp096DamageHandler)
            {
                if (scp096DamageHandler._attackType is Scp096DamageHandler.AttackType.GateKill)
                    return DamageType.Scp096GateKill;

                if (scp096DamageHandler._attackType is Scp096DamageHandler.AttackType.SlapLeft)
                    return DamageType.Scp096SlapLeft;

                if (scp096DamageHandler._attackType is Scp096DamageHandler.AttackType.SlapRight)
                    return DamageType.Scp096SlapRight;

                if (scp096DamageHandler._attackType is Scp096DamageHandler.AttackType.Charge)
                    return DamageType.Scp096Charge;

                ApiLog.Warn("Damage API", $"Unknown SCP-096 damage handler: {scp096DamageHandler._attackType}");
                return DamageType.Scp096;
            }

            if (damageHandlerBase is Scp939DamageHandler scp939DamageHandler)
            {
                if (scp939DamageHandler._damageType is Scp939DamageType.Claw)
                    return DamageType.Scp939Claw;

                if (scp939DamageHandler._damageType is Scp939DamageType.LungeSecondary || scp939DamageHandler._damageType is Scp939DamageType.LungeTarget)
                    return DamageType.Scp939Lunge;

                if (scp939DamageHandler._damageType is Scp939DamageType.None)
                    return DamageType.Scp939;

                ApiLog.Warn("Damage API", $"Unknown SCP-939 damage handler: {scp939DamageHandler._damageType}");
                return DamageType.Scp939;
            }

            if (damageHandlerBase is Scp3114DamageHandler scp3114DamageHandler)
            {
                if (scp3114DamageHandler.Subtype is Scp3114DamageHandler.HandlerType.SkinSteal)
                    return DamageType.Scp3114;

                if (scp3114DamageHandler.Subtype is Scp3114DamageHandler.HandlerType.Slap)
                    return DamageType.Scp3114Slap;

                if (scp3114DamageHandler.Subtype is Scp3114DamageHandler.HandlerType.Strangulation)
                    return DamageType.Scp3114Strangulation;

                ApiLog.Warn("Damage API", $"Unknown SCP-3114 damage handler: {scp3114DamageHandler.Subtype}");
                return DamageType.Scp3114;
            }

            if (damageHandlerBase is ScpDamageHandler scpDamageHandler)
            {
                if (scpDamageHandler.Attacker.Role is RoleTypeId.Scp049)
                    return DamageType.Scp049;

                if (scpDamageHandler.Attacker.Role is RoleTypeId.Scp0492)
                    return DamageType.Zombie;

                if (scpDamageHandler.Attacker.Role is RoleTypeId.Scp096)
                    return DamageType.Scp096;

                if (scpDamageHandler.Attacker.Role is RoleTypeId.Scp173)
                    return DamageType.Scp173;

                if (scpDamageHandler.Attacker.Role is RoleTypeId.Scp3114)
                    return DamageType.Scp3114;

                if (scpDamageHandler.Attacker.Role is RoleTypeId.Scp939)
                    return DamageType.Scp939;

                ApiLog.Warn("Damage API", $"Unknown SCP damage handler: {scpDamageHandler.Attacker.Role}");
                return DamageType.Unknown;
            }

            if (damageHandlerBase is UniversalDamageHandler universalDamageHandler && DeathTranslations.TranslationsById.TryGetValue(universalDamageHandler.TranslationId, out var deathTranslation))
                return deathTranslation.GetDamageType();

            ApiLog.Warn("Damage API", $"Unknown damage handler: {damageHandlerBase.GetType().FullName}");
            return DamageType.Unknown;
        }

        public static DamageType GetDamageType(this DeathTranslation translation)
        {
            if (translation.Id < Translations.Length)
                return Translations[translation.Id];

            ApiLog.Warn("Damage API", $"Out-of-range translation ID: {translation.Id} ({translation.LogLabel} | {translation._deathTranId} | {translation._ragdollTranId})");
            return DamageType.Unknown;
        }

        public static DeathTranslation GetDeathTranslation(this DamageType damageType)
        {
            var translationId = Translations.IndexOf(damageType);

            if (translationId < 0 || translationId >= Translations.Length)
                throw new InvalidOperationException($"Damage type {damageType} is not a valid death translation type.");

            if (!DeathTranslations.TranslationsById.TryGetValue((byte)translationId, out var deathTranslation))
                return DeathTranslations.Unknown;

            return deathTranslation;
        }

        public static bool IsUniversalType(this DamageType damageType, out DeathTranslation deathTranslation)
        {
            deathTranslation = DeathTranslations.Unknown;

            var translationId = Translations.IndexOf(damageType);

            if (translationId < 0 || translationId >= Translations.Length)
                return false;

            return DeathTranslations.TranslationsById.TryGetValue((byte)translationId, out deathTranslation);
        }
    }
}
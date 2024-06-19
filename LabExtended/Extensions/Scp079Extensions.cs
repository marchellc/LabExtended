using LabExtended.API.Enums;

using PlayerRoles.PlayableScps.Scp079.Pinging;

namespace LabExtended.Extensions
{
    public static class Scp079Extensions
    {
        public static Scp079PingType GetPingType(this IPingProcessor pingProcessor)
        {
            if (pingProcessor is null)
                throw new ArgumentNullException(nameof(pingProcessor));

            var pingIndex = Scp079PingAbility.PingProcessors.IndexOf(pingProcessor);

            if (pingIndex < 0 || pingIndex >= Scp079PingAbility.PingProcessors.Length)
                throw new Exception($"Unknown ping processor: {pingProcessor.GetType().FullName}");

            return (Scp079PingType)pingIndex;
        }
    }
}
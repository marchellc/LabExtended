using LabExtended.API.Enums;

using PlayerRoles.PlayableScps.Scp079.Pinging;

namespace LabExtended.Extensions
{
    /// <summary>
    /// Class that holds extensions specific to the SCP-079 role.
    /// </summary>
    public static class Scp079Extensions
    {
        /// <summary>
        /// Converts a <see cref="IPingProcessor"/> instance to it's type.
        /// </summary>
        /// <param name="pingProcessor">The instance to convert.</param>
        /// <returns>The processor's ping type.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
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
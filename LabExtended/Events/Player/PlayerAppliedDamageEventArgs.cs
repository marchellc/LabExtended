using LabExtended.API;

using PlayerStatsSystem;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when after <see cref="StandardDamageHandler.ApplyDamage(ReferenceHub)"/> gets called.
    /// </summary>
    public class PlayerAppliedDamageEventArgs : EventArgs
    {
        /// <summary>
        /// The damage being applied.
        /// </summary>
        public DamageInfo Info { get; }

        /// <summary>
        /// The result of the damage application.
        /// </summary>
        public DamageHandlerBase.HandlerOutput Output { get; set; }

        /// <summary>
        /// The target that the damage is being applied on.
        /// </summary>
        public ExPlayer? Target { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerAppliedDamageEventArgs"/> instance.
        /// </summary>
        /// <param name="damageInfo">Information about the damage.</param>
        /// <param name="output">The output of the damage handler.</param>
        /// <param name="target">The target.</param>
        public PlayerAppliedDamageEventArgs(DamageInfo damageInfo, DamageHandlerBase.HandlerOutput output, ExPlayer? target)
        {
            Info = damageInfo;
            Output = output;
            Target = target;
        }
    }
}
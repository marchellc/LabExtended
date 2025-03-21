using System.Reflection;
using System.Reflection.Emit;

using CentralAuth;

using HarmonyLib;

using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.API;
using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Utilities.Transpilers;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements the <see cref="ExPlayerEvents.Verified"/> event.
/// </summary>
public static class PlayerVerifiedPatch
{
    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.ProcessAuthenticationResponse))]
    public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator,
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        // Returns instructions modified by the delegate.
        return generator.RunTranspiler(instructions, original, ctx =>
        {
            // Sets the insertion index of our code before LabAPI's own event call.
            ctx.FindIndex(OpCodes.Newobj,
                AccessTools.Constructor(typeof(PlayerJoinedEventArgs), [typeof(ReferenceHub)]));
            
            // Get the _hub field value - PlayerAuthenticationManager instance (this) is loaded automatically.
            ctx.LoadFieldValue(typeof(PlayerAuthenticationManager),
                nameof(PlayerAuthenticationManager._hub));

            ctx.Call(typeof(ExPlayer), nameof(ExPlayer.Get), false,
                [typeof(ReferenceHub)]); // Get the ExPlayer instance from the loaded ReferenceHub
            ctx.Call(typeof(InternalEvents), nameof(InternalEvents.HandlePlayerVerified)); // Call the event
        });
    }
}
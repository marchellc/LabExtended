using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using LabExtended.Core;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Disables the server version check when a player connects.
/// </summary>
public static class PlayerVersionCheckPatch
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport), nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        MethodBase originalMethod, ILGenerator generator)
    {
        return generator.RunTranspiler(instructions, originalMethod, ctx =>
        {
            ctx.FindIndex(OpCodes.Call, AccessTools.Method(typeof(GameCore.Version), nameof(GameCore.Version.CompatibilityCheck)));
            ctx.ReplaceInstruction(CodeInstruction.Call(typeof(PlayerVersionCheckPatch), nameof(AlteredVersionCheck)));
        });
    }

    private static bool AlteredVersionCheck(byte sMajor, byte sMinor, byte sRevision, byte cMajor, byte cMinor,
        byte cRevision, bool cBackwardEnabled, byte cBackwardRevision)
    {
        if (!ApiLoader.BaseConfig.SkipClientCompatibility) 
            return GameCore.Version.CompatibilityCheck(sMajor, sMinor, sRevision, cMajor, cMinor, cRevision,
                cBackwardEnabled, cBackwardRevision);
            
        return true;
    }
}
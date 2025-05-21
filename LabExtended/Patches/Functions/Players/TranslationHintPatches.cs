using System.Reflection;
using System.Reflection.Emit;

using Christmas.Scp2536;

using HarmonyLib;

using Hints;

using InventorySystem.Searching;
using LabExtended.API;
using LabExtended.API.Hints;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Attributes;

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Implements translation hints.
/// </summary>
public static class TranslationHintPatches
{
    private static MethodInfo showMethod = AccessTools.Method(typeof(TranslationHintPatches), nameof(Show));
    private static HarmonyMethod transpilerMethod = AccessTools.Method(typeof(TranslationHintPatches), nameof(Transpiler));
    
    private static List<MethodInfo> targetMethods = new()
    {
        AccessTools.Method(typeof(Scp2536GiftController), nameof(Scp2536GiftController.ServerInteract)),
        
        AccessTools.Method(typeof(AmmoSearchCompletor), nameof(AmmoSearchCompletor.Complete)),
        AccessTools.Method(typeof(AmmoSearchCompletor), nameof(AmmoSearchCompletor.ValidateAny)),
        
        AccessTools.Method(typeof(ArmorSearchCompletor), nameof(ArmorSearchCompletor.ValidateAny)),
        
        AccessTools.Method(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.CheckCategoryLimitHint)),
        AccessTools.Method(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.ValidateAny)),
        
        AccessTools.Method(typeof(Scp330SearchCompletor), nameof(Scp330SearchCompletor.ShowOverloadHint))
    };
    
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator, MethodBase originalMethod)
    {
        return generator.RunTranspiler(instructions, originalMethod, ctx =>
        {
            ctx.FindIndex(OpCodes.Callvirt, AccessTools.Method(typeof(HintDisplay), nameof(HintDisplay.Show)));
            ctx.RemoveInstruction();
            ctx.Call(showMethod);
        });
    }

    private static void Show(HintDisplay display, Hint hint)
    {
        if (!ExPlayer.TryGet(display.connectionToClient, out var player))
            return;

        player.ShowHint(hint);
    }
    
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        targetMethods.ForEach(method =>
        {
            try
            {
                var patchMethod = ApiPatcher.Harmony.Patch(method, null, null, transpilerMethod);

                if (patchMethod != null)
                    ApiPatcher.Patches[patchMethod] = method;
            }
            catch (Exception ex)
            {
                ApiLog.Error("TranslationHintPatches", $"Could not apply patch for method &3{method.GetMemberName()}&r:\n{ex}");
            }
        });
    }
}
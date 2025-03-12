using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using LabExtended.Core;
using LabExtended.Utilities.Transpilers;

namespace LabExtended.Extensions;

/// <summary>
/// Transpiler-specific extensions.
/// </summary>
public static class TranspilerExtensions
{
    /// <summary>
    /// Runs a transpiler builder.
    /// </summary>
    /// <param name="generator">The method's ILGenerator.</param>
    /// <param name="instructions">The method's original instructions.</param>
    /// <param name="method">The original method.</param>
    /// <param name="builder">The delegate used to build the transpiler.</param>
    /// <returns>The modified method instruction list.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<CodeInstruction> RunTranspiler(this ILGenerator generator,
        IEnumerable<CodeInstruction> instructions, MethodBase method,
        Action<TranspilerContext> builder)
    {
        if (generator is null)
            throw new ArgumentNullException(nameof(generator));
        
        if (instructions is null)
            throw new ArgumentNullException(nameof(instructions));
        
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));
        
        if (method is null)
            throw new ArgumentNullException(nameof(method));

        var ctx = new TranspilerContext(generator, method, instructions);
        
        builder(ctx);
        
        if (ApiPatcher.TranspilerDebug)
            ctx.Print();
        
        return ctx.ReturnInstructions();
    }
}
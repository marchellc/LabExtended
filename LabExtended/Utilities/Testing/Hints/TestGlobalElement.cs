#define ENABLE_TEST_GLOBAL_HINT

using LabExtended.API;
using LabExtended.API.Hints;
using LabExtended.Attributes;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Utilities.Testing.Hints;

/// <summary>
/// A testing global hint element.
/// </summary>
public class TestGlobalElement : HintElement
{
    /// <inheritdoc cref="HintElement.OnDraw"/>>
    public override bool OnDraw(ExPlayer player)
    {
        Builder.AppendLine("Test Global");
        return true;
    }

#if ENABLE_TEST_GLOBAL_HINT
    [LoaderInitialize(1)]
    private static void OnInit()
        => HintController.AddHintElement<TestGlobalElement>();
#endif
}
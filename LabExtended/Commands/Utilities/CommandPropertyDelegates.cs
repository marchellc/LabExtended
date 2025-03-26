using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Commands.Tokens;

namespace LabExtended.Commands.Utilities;

/// <summary>
/// Registers all built-in command properties.
/// </summary>
public static class CommandPropertyDelegates
{
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        PropertyToken.RegisterProperty<ExPlayer>("context", "sender", ctx => ctx.Sender);
    }
}
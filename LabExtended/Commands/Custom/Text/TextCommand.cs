using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.TextToy;

/// <summary>
/// Provides server-side commands for managing Text Toys.
/// </summary>
/// <remarks>This class defines the entry point for text-related commands that can be executed on the server. It
/// is typically used to register and handle operations related to Text Toys within the command framework.</remarks>
[Command("text", "Commands used to manage Text Toys.")]
public partial class TextCommand : CommandBase, IServerSideCommand { }
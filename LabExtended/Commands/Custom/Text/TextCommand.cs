using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.TextToy;

[Command("text", "Commands used to manage Text Toys.")]
public partial class TextCommand : CommandBase, IServerSideCommand { }
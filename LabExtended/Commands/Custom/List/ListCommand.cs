using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LabExtended.Commands.Custom.List;

[Command("list", "Lists all kind of stuff.")]
public partial class ListCommand : CommandBase, IAllCommand { }
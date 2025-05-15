using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Reset;

[Command("reset all", "Resets a property of all players.")]
public partial class ResetAllCommand : CommandBase, IServerSideCommand { }
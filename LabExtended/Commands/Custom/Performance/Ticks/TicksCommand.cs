using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Performance;

public partial class PerformanceCommand
{
    [CommandOverload("ticks", "Displays the current & target amount of ticks.")]
    public void InvokeTicks()
        => Ok($"Running at {ExServer.Tps} ticks per second (target: {Application.targetFrameRate})");
}
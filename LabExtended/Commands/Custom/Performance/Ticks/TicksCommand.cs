using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Performance;

public partial class PerformanceCommand
{
    /// <summary>
    /// Displays the current and target tick rates for the server.
    /// </summary>
    /// <remarks>This method outputs the server's current ticks per second and the configured target frame
    /// rate. Use this to monitor server performance or diagnose timing issues.</remarks>
    [CommandOverload("ticks", "Displays the current & target amount of ticks.", null)]
    public void InvokeTicks()
        => Ok($"Running at {ExServer.Tps} ticks per second (target: {Application.targetFrameRate})");
}
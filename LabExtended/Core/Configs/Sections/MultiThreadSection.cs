using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections;

public class MultiThreadSection
{
    [Description("The maximum allowed amount of waiting jobs in a single thread handle before starting a new one.")]
    public int MultiThreadHandleMaxSize { get; set; } = 10;

    [Description("How many pending operations can be executed on main thread per tick.")]
    public int MainThreadMaxQueueSize { get; set; } = 50;

    [Description("How long can the queue update take per tick (in milliseconds).")]
    public int MainThreadMaxQueueTime { get; set; } = 200;
}
#define ENABLE_PROFILER

using System.Diagnostics;
using LabExtended.API;
using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities.Update;
using NorthwoodLib.Pools;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;

namespace LabExtended.Utilities;

/// <summary>
/// Utilities targeting Unity's profiler.
/// </summary>
public static class ProfilerUtils
{
    /// <summary>
    /// Contains the result of the profiler.
    /// </summary>
    public class ProfilerResult
    {
        /// <summary>
        /// Gets the recorder for each profiler category.
        /// </summary>
        public Dictionary<string, ProfilerRecorder> Categories { get; } = new()
        {
            ["PlayerLoop"] = default,
            ["GC.Collect"] = default,  
        };
        
        /// <summary>
        /// Gets the size of the mono runtime heap memory.
        /// </summary>
        public long MonoHeapSize { get; internal set; } = 0;
        
        /// <summary>
        /// Gets the size of used memory by the mono runtime.
        /// </summary>
        public long MonoUsedSize { get; internal set; } = 0;
        
        /// <summary>
        /// Gets the total amount of memory allocated to this process.
        /// </summary>
        public long TotalAllocatedMemory { get; internal set; } = 0;
        
        /// <summary>
        /// Gets the total amount of memory reserved to this process.
        /// </summary>
        public long TotalReservedMemory { get; internal set; } = 0;
        
        /// <summary>
        /// Gets the total amount of unused reserved memory for this process.
        /// </summary>
        public long TotalUnusedMemory { get; internal set; } = 0;
    }

    private static bool allEnabled;
    
    /// <summary>
    /// Whether or not the profiler is enabled.
    /// </summary>
    public static bool IsEnabled => Time.IsRunning;

    /// <summary>
    /// How long the profiler should run for (milliseconds).
    /// </summary>
    public static int Duration;

    /// <summary>
    /// Measures the time of the profiler running.
    /// </summary>
    public static readonly Stopwatch Time = new();

    /// <summary>
    /// Gets the result of the profiler.
    /// </summary>
    public static readonly ProfilerResult Result = new();

    /// <summary>
    /// Gets called once the profiler finishes.
    /// </summary>
    public static event Action? Finished;

    /// <summary>
    /// Runs the profiler.
    /// </summary>
    /// <param name="time">The amount of milliseconds to run the profiler for.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void Run(int time)
    {
        if (time < 0)
            throw new ArgumentOutOfRangeException(nameof(time));
        
        if (!allEnabled)
        {
            foreach (var category in Result.Categories.Keys.ToList())
                Result.Categories[category] = new(category, ushort.MaxValue,
                    ProfilerRecorderOptions.WrapAroundWhenCapacityReached | ProfilerRecorderOptions.SumAllSamplesInFrame);

            PlayerUpdateHelper.OnUpdate += Update;
            
            allEnabled = true;
        }

        if (IsEnabled)
        {
            if (!Profiler.enabled)
                Profiler.enabled = true;
            
            if (time == 0)
                Duration = 0;
            else
                Duration += time;

            return;
        }

        Duration = time;
        
        Time.Restart();
        
        foreach (var area in EnumUtils<ProfilerArea>.Values)
        {
            if (Profiler.GetAreaEnabled(area))
                continue;
                
            Profiler.SetAreaEnabled(area, true);
        }
        
        foreach (var profiler in Result.Categories)
            profiler.Value.Start();
        
        Profiler.enabled = true;
    }

    /// <summary>
    /// Stops the profiler.
    /// </summary>
    public static void Stop()
    {
        if (!IsEnabled)
            return;

        Duration = 0;
        
        Time.Stop();
        
        Profiler.enabled = false;
        
        Result.MonoHeapSize = Profiler.GetMonoHeapSizeLong();
        Result.MonoUsedSize = Profiler.GetMonoUsedSizeLong();

        Result.TotalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong();
        Result.TotalReservedMemory = Profiler.GetTotalReservedMemoryLong();
        Result.TotalUnusedMemory = Profiler.GetTotalUnusedReservedMemoryLong();
        
        LogResult();
        
        foreach (var area in EnumUtils<ProfilerArea>.Values)
        {
            if (!Profiler.GetAreaEnabled(area))
                continue;
                
            Profiler.SetAreaEnabled(area, false);
        }
        
        foreach (var profiler in Result.Categories)
            profiler.Value.Stop();
        
        Finished?.InvokeSafe();
    }

    /// <summary>
    /// Logs the profiler result to the console.
    /// </summary>
    public static void LogResult()
    {
        ApiLog.Info("Profiler Utils", StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine();
            x.AppendLine($"Profiler results after running for &1{Time.Elapsed}&r:");

            x.AppendLine($">- &3Mono Heap Memory Size&r: &6{Mirror.Utils.PrettyBytes(Result.MonoHeapSize)}&r");
            x.AppendLine($">- &3Mono Used Memory Size&r: &6{Mirror.Utils.PrettyBytes(Result.MonoUsedSize)}&r");
            x.AppendLine(
                $">- &3Total Allocated Memory&r: &6{Mirror.Utils.PrettyBytes(Result.TotalAllocatedMemory)}&r");
            x.AppendLine($">- &3Total Reserved Memory&r: &6{Mirror.Utils.PrettyBytes(Result.TotalReservedMemory)}&r");
            x.AppendLine($">- &3Total Unused Reserved Memory&r: &6{Mirror.Utils.PrettyBytes(Result.TotalUnusedMemory)}&r");
            x.AppendLine($">- &3Ticks Per Second&r: &6{ExServer.Tps}&r / &1{ExServer.TargetFrameRate}&r");
            x.AppendLine($">- &3Delta Time&r: &6{UnityEngine.Time.deltaTime * 1000}&r ms");
            
            x.AppendLine();
            x.AppendLine($">- &3Profiler(s) ({Result.Categories.Count}&r):");

            foreach (var pair in Result.Categories)
            {
                if (pair.Value.Count < 1 || !pair.Value.Valid)
                    continue;
                
                x.AppendLine($" <- &2{pair.Key}&r: &6{pair.Value.UnitType.ToString((long)pair.Value.ToArray().Average(y => y.Value))}&r");
            }
        }));
    }

    /// <summary>
    /// Converts the specified unit type and it's value to string.
    /// </summary>
    /// <param name="unit">The data type.</param>
    /// <param name="value">The data value.</param>
    /// <returns>The converted value.</returns>
    public static string ToString(this ProfilerMarkerDataUnit unit, long value)
    {
        if (unit is ProfilerMarkerDataUnit.Bytes)
            return Mirror.Utils.PrettyBytes(value);

        if (unit is ProfilerMarkerDataUnit.TimeNanoseconds)
            return $"{TimeSpan.FromTicks(value / 100).TotalMilliseconds} ms";

        if (unit is ProfilerMarkerDataUnit.Percent)
            return $"{value}%";

        if (unit is ProfilerMarkerDataUnit.FrequencyHz)
            return $"{value} Hz";

        return value.ToString();
    }

    private static void Update()
    {
        if (!IsEnabled)
            return;

        if (Duration == 0)
            return;
        
        if (Time.ElapsedMilliseconds >= Duration)
            Stop();
    }
}
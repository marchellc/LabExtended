using System.Diagnostics;
using System.Reflection;

using LabExtended.API;
using LabExtended.API.Enums;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Attributes;

using LabExtended.Utilities.Unity;

using NorthwoodLib.Pools;

using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LabExtended.Utilities.Update;

/// <summary>
/// A helper class used for player updates.
/// </summary>
public static class PlayerUpdateHelper
{
    /// <summary>
    /// Exposes the update loop.
    /// </summary>
    public struct PlayerUpdateLoop { }
    
    private static long longestUpdate = -1;
    private static long shortestUpdate = -1;

    private static long previousLongest = -1;
    private static long previousShortest = -1;

    private static int updatesCount = 0;
    
    private static bool isStatsPaused = true;
    
    private static Stopwatch timeWatch = new();

    /// <summary>
    /// Gets called once per every frame.
    /// </summary>
    public static event Action? OnUpdate;

    /// <summary>
    /// Gets called once per every millisecond on a background thread.
    /// </summary>
    public static event Func<Task>? OnThreadUpdate;
    
    /// <summary>
    /// Gets an <b>approximate</b> count of registered update methods.
    /// </summary>
    public static int Count => updatesCount;

    /// <summary>
    /// Gets a list of all update methods assigned to <see cref="OnUpdate"/>.
    /// <remarks>This method creates a new instance of <see cref="List{T}"/> every time it's called.</remarks>
    /// </summary>
    /// <returns></returns>
    public static List<Action> GetUpdates()
    {
        var updates = new List<Action>();
        
        if (OnUpdate is null)
            return updates;

        foreach (var updateDelegate in OnUpdate.GetInvocationList())
        {
            if (updateDelegate is not Action action)
                continue;
            
            updates.Add(action);
        }
        
        return updates;
    }

    /// <summary>
    /// Registers all update methods in an assembly.
    /// <remarks>You <b>NEED</b> to make sure that this is called only <b>ONCE</b>, as there aren't any
    /// checks in place for duplicate update methods.</remarks>
    /// <para>Static fields that end with Reference and start with the target method name (and match the
    /// <see cref="PlayerUpdateReference"/> type) will get their reference assigned.</para>
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static int RegisterUpdates(this Assembly assembly)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));

        var count = 0;
        
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetAllMethods())
            {
                if (!method.IsStatic || method.ReturnType != typeof(void))
                    continue;
                
                if (!method.HasAttribute<PlayerUpdateAttribute>(out var playerUpdateAttribute))
                    continue;
                
                if (method.GetAllParameters().Any())
                    continue;
                
                if (method.CreateDelegate(typeof(Action)) is not Action updateMethod)
                    continue;

                var updateReference = updateMethod.RegisterUpdate(
                    playerUpdateAttribute.TimeDelay > 0f ? playerUpdateAttribute.TimeDelay : null,
                    playerUpdateAttribute.BlacklistedRoundStates != RoundState.Unknown ? playerUpdateAttribute.BlacklistedRoundStates : null,
                    playerUpdateAttribute.WhitelistedRoundStates != RoundState.Unknown ? playerUpdateAttribute.WhitelistedRoundStates : null);
                
                if (updateReference is null)
                    continue;

                var updateFieldName = $"{method.Name}Reference";
                var updateField = type.FindField(f =>
                    f.Name == updateFieldName && f.FieldType == typeof(PlayerUpdateReference) 
                                              && f is { IsStatic: true, IsInitOnly: false });
                
                updateField?.SetValue(null, updateReference);
                
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Registers a new update method.
    /// </summary>
    /// <param name="onUpdate">The method to register.</param>
    /// <param name="timeDelay">The delay between each execution (in seconds).</param>
    /// <param name="blacklistedStates">The round states at which this method will not be called.</param>
    /// <param name="whitelistedStates">The only round states at which this method will be called.</param>
    /// <returns>The reference to this update method.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static PlayerUpdateReference RegisterUpdate(this Action onUpdate, float? timeDelay = null,
        RoundState? blacklistedStates = null, RoundState? whitelistedStates = null)
    {
        if (onUpdate is null)
            throw new ArgumentNullException(nameof(onUpdate));

        var reference = new PlayerUpdateReference();

        reference.IsEnabled = true;

        if (timeDelay is > 0f)
            reference.DelayTime = timeDelay.Value;
        
        reference.TargetUpdate = onUpdate;

        reference.WhitelistedStates = whitelistedStates;
        reference.BlacklistedStates = blacklistedStates;
        
        reference.OnUpdate = () =>
        {
            if (!reference.IsEnabled)
                return;

            if (reference.TargetUpdate is null)
            {
                reference.IsEnabled = false;
                
                OnUpdate -= reference.OnUpdate;
                return;
            }

            if (reference.DelayTime > 0f)
            {
                reference.RemainingTime -= Time.deltaTime;

                if (reference.RemainingTime > 0f)
                    return;

                reference.RemainingTime = reference.DelayTime;
            }

            if (reference.WhitelistedStates.HasValue
                && (ExRound.State & reference.WhitelistedStates.Value) != reference.WhitelistedStates.Value)
                return;

            if (reference.BlacklistedStates.HasValue
                && (ExRound.State & reference.BlacklistedStates.Value) == reference.BlacklistedStates.Value)
                return;

            reference.TargetUpdate.InvokeSafe();
        };

        isStatsPaused = false;

        OnUpdate += reference.OnUpdate;

        updatesCount++;
        return reference;
    }
    
    private static void Update()
    {
        if (isStatsPaused && OnUpdate != null)
            isStatsPaused = false;
        
        if (!isStatsPaused)
            timeWatch.Restart();

#pragma warning disable CS8604 // Possible null reference argument.
        OnUpdate.InvokeSafe();
#pragma warning restore CS8604 // Possible null reference argument.

        if (isStatsPaused) 
            return;
        
        var elapsed = timeWatch.ElapsedTicks;

        if (elapsed > longestUpdate || longestUpdate == -1)
            longestUpdate = elapsed;

        if (elapsed < shortestUpdate || shortestUpdate == -1)
            shortestUpdate = elapsed;
    }

    private static async Task ThreadUpdateAsync()
    {
        while (true)
        {
            await Task.Delay(1);
            
            OnThreadUpdate?.InvokeSafe();
        }
    }

    private static void OnWaiting()
    {
        if (isStatsPaused)
            return;
        
        ApiLog.Debug("Player Update Helper", StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine();
            
            var longestSpan = TimeSpan.FromTicks(longestUpdate);
            var shortestSpan = TimeSpan.FromTicks(shortestUpdate);

            if (previousLongest != -1 && longestUpdate > previousLongest)
            {
                var previousLongestSpan = TimeSpan.FromTicks(longestUpdate - previousLongest);

                x.AppendLine(
                    $"&1Longest Update&r: &3{longestSpan.TotalMilliseconds} ms&r (&1+ {previousLongestSpan.TotalMilliseconds} ms&r)");
            }
            else if (previousLongest != -1 && longestUpdate < previousLongest)
            {
                var previousLongestSpan = TimeSpan.FromTicks(previousLongest - longestUpdate);

                x.AppendLine(
                    $"&1Longest Update&r: &3{longestSpan.TotalMilliseconds} ms&r (&2- {previousLongestSpan.TotalMilliseconds} ms&r)");
            }
            else
            {
                x.AppendLine($"&1Longest Update&r: &3{longestSpan.TotalMilliseconds} ms&r");
            }
            
            if (previousShortest != -1 && shortestUpdate > previousShortest)
            {
                var previousShortestSpan = TimeSpan.FromTicks(shortestUpdate - previousShortest);

                x.AppendLine(
                    $"&2Shortest Update&r: &3{shortestSpan.TotalMilliseconds} ms&r (&1+ {previousShortestSpan.TotalMilliseconds} ms&r)");
            }
            else if (previousShortest != -1 && shortestUpdate < previousShortest)
            {
                var previousShortestSpan = TimeSpan.FromTicks(previousShortest - shortestUpdate);

                x.AppendLine(
                    $"&2Shortest Update&r: &3{shortestSpan.TotalMilliseconds} ms&r (&2- {previousShortestSpan.TotalMilliseconds} ms&r)");
            }
            else
            {
                x.AppendLine($"&2Shortest Update&r: &3{longestSpan.TotalMilliseconds} ms&r");
            }

            previousLongest = longestUpdate;
            previousShortest = shortestUpdate;
            
            longestUpdate = -1;
            shortestUpdate = -1;
        }));
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        InternalEvents.OnRoundWaiting += OnWaiting;

        PlayerLoopHelper.ModifySystem(x =>
        {
            if (!x.InjectAfter<TimeUpdate.WaitForLastPresentationAndUpdateTime>(Update, typeof(PlayerUpdateLoop)))
                return null;

            return x;
        });

        Task.Run(ThreadUpdateAsync);
    }
}
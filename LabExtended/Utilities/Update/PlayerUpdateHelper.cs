using System.Reflection;

using LabExtended.API;
using LabExtended.API.Enums;

using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.Utilities.Update;

/// <summary>
/// A helper class used for player updates.
/// </summary>
public static class PlayerUpdateHelper
{
    /// <summary>
    /// Gets the main <see cref="PlayerUpdateComponent"/> instance.
    /// </summary>
    public static PlayerUpdateComponent Component { get; } = PlayerUpdateComponent.Create();

    /// <summary>
    /// Occurs when a frame is rendered.
    /// </summary>
    public static event Action? OnUpdate;

    /// <summary>
    /// Occurs after the standard update cycle has completed, allowing subscribers to perform actions that should run
    /// late in the frame.
    /// </summary>
    /// <remarks>Use this event to execute logic that must happen after all regular update processing.
    /// Subscribers should ensure their handlers are efficient, as all listeners are invoked sequentially. This event is
    /// static and affects all instances.</remarks>
    public static event Action? OnLateUpdate;

    /// <summary>
    /// Occurs when a fixed update cycle is processed, allowing subscribers to perform actions at a consistent interval.
    /// </summary>
    /// <remarks>This event is typically raised at a fixed time step, such as in physics or simulation loops,
    /// to ensure consistent updates regardless of frame rate. Subscribers should avoid long-running operations to
    /// prevent delays in the update cycle.</remarks>
    public static event Action? OnFixedUpdate;

    /// <summary>
    /// Gets called once per every millisecond on a background thread.
    /// </summary>
    public static event Func<Task>? OnThreadUpdate;

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
                
                Component.OnUpdate -= reference.OnUpdate;
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

        Component.OnUpdate += reference.OnUpdate;
        return reference;
    }

    private static void Update()
        => OnUpdate?.InvokeSafe();

    private static void LateUpdate()
        => OnLateUpdate?.InvokeSafe();

    private static void FixedUpdate()
        => OnFixedUpdate?.InvokeSafe();

    private static async Task ThreadUpdateAsync()
    {
        while (true)
        {
            await Task.Delay(1);

            var task = OnThreadUpdate?.InvokeSafe();

            if (task != null)
                await task;
        }
    }

    internal static void Internal_Init()
    {
        Component.OnUpdate += Update;
        Component.OnLateUpdate += LateUpdate;
        Component.OnFixedUpdate += FixedUpdate;

        Task.Run(ThreadUpdateAsync);
    }
}
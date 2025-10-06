using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Utilities.Update;

using UnityEngine;

namespace LabExtended.Utilities;

/// <summary>
/// Utilities for MEC coroutines and execution timing.
/// </summary>
public static class TimingUtils
{
    /// <summary>
    /// Represents a timed call (surprisingly).
    /// </summary>
    public class TimedCall : IEquatable<TimedCall>
    {
        internal float remainingTime;
        
        internal int remainingFrames;
        internal int remainingRepeats;
        
        /// <summary>
        /// The call ID.
        /// </summary>
        public readonly ulong Id;
        
        /// <summary>
        /// The target method.
        /// </summary>
        public readonly Action Target;

        /// <summary>
        /// The target callback.
        /// </summary>
        public readonly Action? Callback;

        /// <summary>
        /// The target condition.
        /// </summary>
        public readonly Func<bool>? Condition;

        /// <summary>
        /// How many times to repeat the call.
        /// </summary>
        public readonly int? RepeatCount;
        
        /// <summary>
        /// The amount of frames to wait.
        /// </summary>
        public readonly int? WaitForFrames;

        /// <summary>
        /// The amount of seconds to wait.
        /// </summary>
        public readonly float? WaitForSeconds;

        /// <summary>
        /// Whether or not to remove the call if the condition fails.
        /// </summary>
        public readonly bool RemoveOnConditionFail;

        /// <summary>
        /// Whether or not this is a while condition call.
        /// </summary>
        public readonly bool IsWhile;
        
        internal TimedCall(ulong id, float? waitSeconds, int? waitFrames, int? repeatCount, bool removeOnConditionFail, bool isWhile,
            Action target, Action? callback, Func<bool>? condition)
        {
            Id = id;
            Target = target;
            Callback = callback;
            Condition = condition;
            IsWhile = isWhile;
            WaitForFrames = waitFrames;
            WaitForSeconds = waitSeconds;
            RepeatCount = repeatCount;
            RemoveOnConditionFail = removeOnConditionFail;
            
            if (waitSeconds.HasValue)
                remainingTime = waitSeconds.Value;
            
            if (waitFrames.HasValue)
                remainingFrames = waitFrames.Value;
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(TimedCall other)
             => Id == other.Id;

        /// <inheritdoc cref="IEquatable{T}.Equals(object)"/>
        public override bool Equals(object? obj)
            => obj is TimedCall other && Equals(other);

        /// <inheritdoc cref="ValueType.GetHashCode"/>
        public override int GetHashCode()
            => Id.GetHashCode();

        /// <summary>
        /// Compares two TimedCall instances.
        /// </summary>
        public static bool operator ==(TimedCall left, TimedCall right)
             => left.Equals(right);

        /// <summary>
        /// Compares two TimedCall instances.
        /// </summary>
        public static bool operator !=(TimedCall left, TimedCall right)
             => !left.Equals(right);
    }

    private static ulong callId = 0;

    private static List<TimedCall> inserts = new();
    private static List<ulong> indexes = new();
    
    /// <summary>
    /// Gets a list of all active calls.
    /// </summary>
    public static List<TimedCall> Calls { get; } = new();
    
    /// <summary>
    /// Executes a delegate once a condition delegate returns false.
    /// </summary>
    /// <param name="action">The target delegate.</param>
    /// <param name="condition">The target condition.</param>
    /// <param name="callback">The delegate to invoke once the target delegate is invoked.</param>
    public static void OnFalse(this Action action, Func<bool> condition, Action? callback = null)
        => Queue(null, null, 1, false, false, action, callback, () => !condition());

    /// <summary>
    /// Executes a delegate once a condition delegate returns true.
    /// </summary>
    /// <param name="action">The target delegate.</param>
    /// <param name="condition">The target condition.</param>
    /// <param name="callback">The delegate to invoke once the target delegate is invoked.</param>
    public static void OnTrue(this Action action, Func<bool> condition, Action? callback = null)
        => Queue(null, null, 1, false, false, action, callback, condition);
    
    /// <summary>
    /// Continuously executes a delegate while a condition delegate returns false.
    /// </summary>
    /// <param name="action">The target delegate.</param>
    /// <param name="condition">The required condition.</param>
    /// <param name="secondsDelay">The amount of seconds to wait between each loop.</param>
    /// <param name="framesDelay">The amount of frames to wait between each loop.</param>
    /// <param name="callback">The delegate to invoke every time the target delegate is invoked.</param>
    public static void WhileFalse(this Action action, Func<bool> condition, float? secondsDelay, int? framesDelay, Action? callback = null)
        => Queue(secondsDelay, framesDelay, -1, false, true, action, callback, () => !condition());
    
    /// <summary>
    /// Continuously executes a delegate while a condition delegate returns true.
    /// </summary>
    /// <param name="action">The target delegate.</param>
    /// <param name="condition">The required condition.</param>
    /// <param name="secondsDelay">The amount of seconds to wait between each loop.</param>
    /// <param name="framesDelay">The amount of frames to wait between each loop.</param>
    /// <param name="callback">The delegate to invoke every time the target delegate is invoked.</param>
    public static void WhileTrue(this Action action, Func<bool> condition, float? secondsDelay, int? framesDelay, Action? callback = null)
        => Queue(secondsDelay, framesDelay, -1, false, true, action, callback, condition);

    /// <summary>
    /// Waits a specific amount of frames and executes a specific delegate.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="frames">The amount of frames to wait for.</param>
    /// <param name="callback">The delegate to invoke once the target delegate is invoked.</param>
    public static void AfterFrames(this Action action, int frames, Action? callback = null)
        => Queue(null, frames, null, false, false, action, callback, null);

    /// <summary>
    /// Waits a specific amount of seconds and executes a specific delegate.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="seconds">The amount of seconds to wait for.</param>
    /// <param name="callback">The delegate to invoke once the target delegate is invoked.</param>
    public static void AfterSeconds(this Action action, float seconds, Action? callback = null)
        => Queue(seconds, null, null, false, false,action, callback, null);

    private static void Queue(float? waitSeconds, int? waitFrames, int? repeatCount, bool removeOnConditionFail, bool isWhile,
        Action target, Action? callback, Func<bool>? condition)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        inserts.Add(new(callId++, waitSeconds, waitFrames, repeatCount ?? 1, removeOnConditionFail, isWhile, target, callback, condition));
    }

    private static void Update()
    {
        if (inserts.Count > 0)
        {
            Calls.AddRange(inserts);
            
            inserts.Clear();
        }
        
        if (Calls.Count > 0)
        {
            for (var i = 0; i < Calls.Count; i++)
            {
                var call = Calls[i];
                
                if (call.WaitForSeconds.HasValue)
                {
                    call.remainingTime -= Time.deltaTime;

                    if (call.remainingTime > 0f)
                        continue;
                }

                if (call.WaitForFrames.HasValue)
                {
                    call.remainingFrames--;
                    
                    if (call.remainingFrames > 0)
                        continue;
                }

                if (call.Condition != null && !call.Condition.InvokeSafe())
                {
                    if (call.RemoveOnConditionFail)
                        indexes.Add(call.Id);
                    
                    continue;
                }
                
                call.Target.InvokeSafe();
                call.Callback?.InvokeSafe();

                if (!call.IsWhile)
                {
                    call.remainingRepeats--;

                    if (call.remainingRepeats > 0)
                    {
                        if (call.WaitForFrames.HasValue)
                            call.remainingFrames = call.WaitForFrames.Value;

                        if (call.WaitForSeconds.HasValue)
                            call.remainingTime = call.WaitForSeconds.Value;

                        continue;
                    }

                    indexes.Add(call.Id);
                }
                else
                {
                    if (call.WaitForFrames.HasValue)
                        call.remainingFrames = call.WaitForFrames.Value;

                    if (call.WaitForSeconds.HasValue)
                        call.remainingTime = call.WaitForSeconds.Value;
                }
            }

            if (indexes.Count > 0)
            {
                Calls.RemoveAll(call => indexes.Contains(call.Id));
                
                indexes.Clear();
            }
        }
    }

    internal static void Internal_Init()
    {
        PlayerUpdateHelper.Component.OnUpdate += Update;
    }
}
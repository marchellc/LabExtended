using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Extensions;

using LabExtended.Utilities.Update;

using UnityEngine;

namespace LabExtended.Utilities;

/// <summary>
/// Utilities for MEC coroutines and execution timing.
/// </summary>
public static class TimingUtils
{
    private enum CallType
    {
        AfterFrames,
        AfterTime,
        
        RepeatCall
    }
    
    private struct CallDataNoReturn
    {
        public Action targetMethod;
        public Action? targetCallback;

        public int remainingFrames;
        public int remainingRepeats;
        
        public float remainingTime;
        public float? repeatDelay;

        public CallType type;
    }
    
    private struct ConditionalCallData
    {
        public Action targetMethod;
        public Func<bool> targetCondition;

        public float remainingTime;
        public float? repeatDelay;

        public bool endOnCondition;
        public bool callOnce;
    }

    private static Queue<CallDataNoReturn> voidCalls = new();
    private static Queue<ConditionalCallData> conditionalCalls = new();

    /// <summary>
    /// Gets the amount of calls in the queue.
    /// </summary>
    public static int QueueSize => voidCalls.Count;

    /// <summary>
    /// Invokes the specified delegate once the condition delegate returns TRUE.
    /// </summary>
    /// <param name="action">The target delegate.</param>
    /// <param name="condition">The target condition.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void OnTrue(this Action action, Func<bool> condition)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (condition is null)
            throw new ArgumentNullException(nameof(condition));
        
        conditionalCalls.Enqueue(new()
        {
            targetMethod = action,
            targetCondition = condition,
            
            endOnCondition = false,
            callOnce = true
        });
    }

    /// <summary>
    /// Calls the specified delegate each frame while the specified condition returns TRUE.
    /// </summary>
    /// <param name="action">The target delegate.</param>
    /// <param name="condition">The target condition delegate.</param>
    /// <param name="delay">Delay between each call.</param>
    /// <param name="endOnConditionFalse">Whether or not the call should be stopped if the condition returns false (or just paused).</param>
    public static void WhileTrue(this Action action, Func<bool> condition, float? delay = null,
        bool endOnConditionFalse = true)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (condition is null)
            throw new ArgumentNullException(nameof(condition));
        
        conditionalCalls.Enqueue(new()
        {
            targetMethod = action,
            targetCondition = condition,
            
            remainingTime = delay ?? 0f,
            repeatDelay = delay,
            
            endOnCondition = endOnConditionFalse
        });
    }
    
    /// <summary>
    /// Calls the specified method on the next frame.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="callback">The delegate to invoke once the target delegate (action) is invoked</param>
    public static void NextFrame(this Action action, Action? callback = null)
    {
        AfterFrames(action, 1, callback);
    }

    /// <summary>
    /// Calls the target delegate after a specific amount of frames has passed.
    /// </summary>
    /// <param name="action">The target delegate.</param>
    /// <param name="frameCount">The amount of frames to wait for.</param>
    /// <param name="callback">The method to invoke when the target delegate is invoked.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void AfterFrames(this Action action, int frameCount, Action? callback = null)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (frameCount < 1)
            throw new ArgumentOutOfRangeException(nameof(frameCount));
        
        voidCalls.Enqueue(new()
        {
            targetMethod = action,
            targetCallback = callback,
            
            remainingFrames = frameCount,
            
            type = CallType.AfterFrames
        });
    }

    /// <summary>
    /// Calls the target delegate after a specific amount of seconds has passed.
    /// </summary>
    /// <param name="action">The target delegate.</param>
    /// <param name="seconds">The amount of seconds to wait for.</param>
    /// <param name="callback">The delegate to invoke when the target delegate is invoked.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void AfterSeconds(this Action action, float seconds, Action? callback = null)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (seconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(seconds));
        
        voidCalls.Enqueue(new()
        {
            targetMethod = action,
            targetCallback = callback,
            
            remainingTime = seconds,
            
            type = CallType.AfterTime
        });
    }

    /// <summary>
    /// Calls the specified delegate for the specified amount of times.
    /// </summary>
    /// <param name="action">The delegate to invoke repeatedly.</param>
    /// <param name="repeatCount">How many times to call the delegate.</param>
    /// <param name="repeatDelay">How many seconds to wait before each call.</param>
    public static void Repeat(this Action action, int repeatCount, float? repeatDelay = null)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (repeatCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(repeatCount));
        
        voidCalls.Enqueue(new()
        {
            targetMethod = action,
            
            remainingRepeats = repeatCount,
            remainingTime = repeatDelay ?? 0f,
            
            repeatDelay = repeatDelay,
            
            type = CallType.RepeatCall
        });
    }

    private static void Update()
    {
        while (voidCalls.TryDequeue(out var callData))
        {
            if (callData.type is CallType.RepeatCall)
            {
                HandleRepeat(ref callData);
            }
            else if (callData.type is CallType.AfterFrames)
            {
                HandleFrames(ref callData);
            }
            else if (callData.type is CallType.AfterTime)
            {
                HandleTime(ref callData);
            }
            else
            {
                ApiLog.Warn("Timing API", $"Unknown call type: &3{callData.type}&r");
            }
        }

        while (conditionalCalls.TryDequeue(out var conditionalCallData))
        {
            HandleConditional(ref conditionalCallData);
        }
    }

    private static void HandleRepeat(ref CallDataNoReturn callData)
    {
        if (callData.repeatDelay.HasValue)
        {
            callData.remainingTime -= Time.deltaTime;

            if (callData.remainingTime > 0f)
            {
                voidCalls.Enqueue(callData);
                return;
            }
        }
        
        callData.targetMethod.InvokeSafe();
        callData.remainingRepeats--;

        if (callData.repeatDelay.HasValue)
            callData.remainingTime = callData.repeatDelay.Value;
        
        if (callData.remainingRepeats > 0)
            voidCalls.Enqueue(callData);
    }

    private static void HandleFrames(ref CallDataNoReturn callData)
    {
        callData.remainingFrames--;

        if (callData.remainingFrames > 0)
        {
            voidCalls.Enqueue(callData);
            return;
        }
        
        callData.targetMethod.InvokeSafe();
        callData.targetCallback?.InvokeSafe();
    }

    private static void HandleTime(ref CallDataNoReturn callData)
    {
        callData.remainingTime -= Time.deltaTime;

        if (callData.remainingTime > 0f)
        {
            voidCalls.Enqueue(callData);
            return;
        }
        
        callData.targetMethod.InvokeSafe();
        callData.targetCallback?.InvokeSafe();
    }

    private static void HandleConditional(ref ConditionalCallData callData)
    {
        if (callData.repeatDelay.HasValue)
        {
            callData.remainingTime -= Time.deltaTime;

            if (callData.remainingTime > 0f)
            {
                conditionalCalls.Enqueue(callData);
                return;
            }
            
            callData.remainingTime = callData.repeatDelay.Value;
        }
        
        if (callData.targetCondition.InvokeSafe())
        {
            callData.targetMethod.InvokeSafe();

            if (!callData.callOnce)
            {
                conditionalCalls.Enqueue(callData);
            }
        }
        else
        {
            if (!callData.endOnCondition)
            {
                conditionalCalls.Enqueue(callData);
            }
        }
    }

    [LoaderInitialize(1)]
    private static void Init()
    {
        PlayerUpdateHelper.OnUpdate += Update;
    }
}
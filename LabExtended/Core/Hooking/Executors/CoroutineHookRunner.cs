using LabExtended.Core.Hooking.Interfaces;

using MEC;

namespace LabExtended.Core.Hooking.Executors
{
    public class CoroutineHookRunner : IHookRunner
    {
        public void OnEvent(object eventObject, HookInfo hook, IHookBinder binder, Action<bool, bool, Exception, object> callback)
        {
            if (!binder.BindArgs(eventObject, out var methodArgs))
            {
                ExLoader.Warn("Hooking API", $"Binder &3{binder.GetType().FullName}&r failed to bind args!");

                callback(true, false, null, null);
                return;
            }

            try
            {
                var result = default(object);

                if (hook.Method.IsStatic)
                    result = hook.Dynamic.InvokeStatic(methodArgs);
                else
                    result = hook.Dynamic.Invoke(hook.Instance, methodArgs);

                if (result is null)
                {
                    callback(false, false, null, null);
                    return;
                }

                if (result is CoroutineHandle coroutineHandle)
                {
                    if (Timing.IsAliveAndPaused(coroutineHandle))
                        Timing.ResumeCoroutines(coroutineHandle);

                    if (hook.ShouldWait)
                    {
                        var maxWait = DateTime.Now.AddSeconds(1.5);

                        while (Timing.IsRunning(coroutineHandle))
                        {
                            if (DateTime.Now >= maxWait)
                            {
                                callback(true, true, null, null);
                                return;
                            }
                        }

                        callback(false, false, null, null);
                        return;
                    }
                    else
                    {
                        callback(false, false, null, null);
                        return;
                    }
                }

                if (result is IEnumerator<float> coroutine)
                {
                    var maxWait = DateTime.Now.AddSeconds(1.5);
                    var handle = Timing.RunCoroutine(coroutine);

                    if (hook.ShouldWait)
                    {
                        while (Timing.IsRunning(handle))
                        {
                            if (DateTime.Now >= maxWait)
                            {
                                callback(true, true, null, null);
                                return;
                            }
                        }
                    }
                    else
                    {
                        callback(false, false, null, null);
                        return;
                    }
                }

                callback(false, false, null, null);
            }
            catch (Exception ex)
            {
                callback(true, false, ex, null);
            }
        }
    }
}
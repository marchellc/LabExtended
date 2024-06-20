using Common.Extensions;

using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Core.Hooking.Executors
{
    public class SimpleHookRunner : IHookRunner
    {
        public void OnEvent(object eventObject, HookInfo hook, IHookBinder binder, Action<bool, bool, Exception, object> callback)
        {
            if (!binder.BindArgs(eventObject, out var methodArgs))
            {
                ExLoader.Error("Hooking API", $"Binder &3{binder.GetType().Name}&r failed to bind args to method &3{hook.Method.ToName()}&r!");

                callback(true, false, null, null);
                return;
            }

            try
            {
                if (hook.Method.IsStatic)
                    callback(false, false, null, hook.Dynamic.InvokeStatic(methodArgs));
                else
                    callback(false, false, null, hook.Dynamic.Invoke(hook.Instance, methodArgs));
            }
            catch (Exception ex)
            {
                callback(true, false, ex, null);
            }

            binder.UnbindArgs(methodArgs);
        }
    }
}
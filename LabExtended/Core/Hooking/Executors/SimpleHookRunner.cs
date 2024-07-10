using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Core.Hooking.Executors
{
    public class SimpleHookRunner : IHookRunner
    {
        public object OnEvent(object eventObject, HookInfo hook, IHookBinder binder)
        {
            if (!binder.BindArgs(eventObject, out var methodArgs))
                throw new Exception("Argument binder failed to bind method arguments.");

            var methodResult = hook.Method.Invoke(hook.Instance, methodArgs);

            if (methodArgs != null)
                binder.UnbindArgs(methodArgs);

            return methodResult;
        }
    }
}
using LabExtended.Core.Hooking.Interfaces;

using MEC;

namespace LabExtended.Core.Hooking.Executors
{
    public class CoroutineHookRunner : IHookRunner
    {
        public object OnEvent(object eventObject, HookInfo hook, IHookBinder binder)
        {
            if (!binder.BindArgs(eventObject, out var methodArgs))
                throw new Exception($"Argument binder failed to bind method arguments.");

            var methodResult = default(object);

            if (hook.Method.IsStatic)
                methodResult = hook.Dynamic.InvokeStatic(methodArgs);
            else
                methodResult = hook.Dynamic.Invoke(hook.Instance, methodArgs);

            if (methodResult is null)
                throw new Exception("Method returned a null value.");

            if (methodResult is not IEnumerator<float> mecCoroutine)
                throw new Exception($"Method returned an unknown value: {methodResult.GetType().FullName}");

            Timing.RunCoroutine(mecCoroutine);

            if (methodArgs != null)
                binder.UnbindArgs(methodArgs);

            return null;
        }
    }
}
using LabExtended.Core.Hooking.Enums;
using LabExtended.Core.Hooking.Interfaces;

using LabExtended.Utilities;

using System.Reflection;

namespace LabExtended.Core.Hooking
{
    public class HookInfo
    {
        public IHookRunner Runner { get; }
        public IHookBinder Binder { get; }

        public HookPriority Priority { get; }

        public MethodInfo Method { get; }

        public object Instance { get; }

        public Func<object, object[], object> Invoker { get; }

        internal HookInfo(MethodInfo method, object instance, IHookRunner hookRunner, IHookBinder hookBinder, HookPriority hookPriority, bool useReflection)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (hookRunner is null)
                throw new ArgumentNullException(nameof(hookRunner));

            if (hookBinder is null)
                throw new ArgumentNullException(nameof(hookBinder));

            Runner = hookRunner;
            Binder = hookBinder;
            Priority = hookPriority;

            Method = method;
            Instance = instance;

            Invoker = useReflection ? method.Invoke : FastReflection.ForMethod(method);
        }

        internal object Run(object eventObject)
            => Runner.OnEvent(eventObject, this, Binder);
    }
}
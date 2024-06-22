using Common.Utilities.Dynamic;

using LabExtended.Core.Hooking.Enums;
using LabExtended.Core.Hooking.Interfaces;

using LabExtended.Core.Profiling;

using System.Reflection;

namespace LabExtended.Core.Hooking
{
    public class HookInfo
    {
        public ProfilerMarker Marker { get; }

        public bool IsMarkerActive { get; set; } = true;

        public IHookRunner Runner { get; }
        public IHookBinder Binder { get; }

        public HookPriority Priority { get; }

        public MethodInfo Method { get; }
        public DynamicMethod Dynamic { get; }

        public object Instance { get; }

        internal HookInfo(MethodInfo method, object instance, IHookRunner hookRunner, IHookBinder hookBinder, HookPriority hookPriority)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (hookRunner is null)
                throw new ArgumentNullException(nameof(hookRunner));

            if (hookBinder is null)
                throw new ArgumentNullException(nameof(hookBinder));

            Marker = new ProfilerMarker($"Hook Handler ({method.DeclaringType.FullName}::{method.Name})");

            Runner = hookRunner;
            Binder = hookBinder;
            Priority = hookPriority;

            Method = method;
            Instance = instance;

            Dynamic = DynamicMethod.Create(method);
        }

        internal object Run(object eventObject)
        {
            if (IsMarkerActive)
                Marker.MarkStart();

            var result = Runner.OnEvent(eventObject, this, Binder);

            if (IsMarkerActive)
                Marker.MarkEnd();

            return result;
        }
    }
}
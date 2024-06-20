using Common.Extensions;
using Common.Utilities;
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

        public bool ShouldWait { get; }

        internal HookInfo(MethodInfo method, object instance, IHookRunner hookRunner, IHookBinder hookBinder, HookPriority hookPriority, bool shouldWait)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (hookRunner is null)
                throw new ArgumentNullException(nameof(hookRunner));

            if (hookBinder is null)
                throw new ArgumentNullException(nameof(hookBinder));

            Marker = new ProfilerMarker($"Hook Handler ({method.ToName()})");

            Runner = hookRunner;
            Binder = hookBinder;
            Priority = hookPriority;

            Method = method;
            Instance = instance;

            ShouldWait = shouldWait;

            Dynamic = DynamicMethod.Create(method);
        }

        internal void Run(object eventObject, Action<bool, bool, Exception, object> callback)
        {
            if (IsMarkerActive)
                Marker.MarkStart();

            Runner.OnEvent(eventObject, this, Binder, (hasFailed, hasTimedOut, exception, result) =>
            {
                if (IsMarkerActive)
                    Marker.MarkEnd();

                callback(hasFailed, hasTimedOut, exception, result);
            });
        }
    }
}
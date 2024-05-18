using Common.Extensions;

using MEC;

using System.Reflection;

namespace LabExtended.Core.Hooking.Executors
{
    public class HookTaskExecutor : HookExecutor
    {
        public static readonly HookTaskExecutor Instance = new HookTaskExecutor();

        public readonly PropertyInfo IsCompletedProperty = typeof(Task).Property("IsCompleted");

        public override HookType Type { get; } = HookType.Task;

        public override void Execute(HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo, HookBinder binder, Action<HookResult> resultCallback)
        {
            var args = binder.BindArgs(hookRuntimeInfo, hookInfo);
            var result = hookInfo.Target.Call(hookInfo.Instance, args);

            if (result is null)
            {
                resultCallback(new HookResult(null, HookResultType.Success));
                return;
            }

            var type = result.GetType();
            var prop = IsCompletedProperty;

            if (type.IsGenericType || type.IsGenericTypeDefinition)
                prop = type.Property("IsCompleted");

            Timing.RunCoroutine(AwaitTask(result, prop, hookInfo, resultCallback));
        }

        private static IEnumerator<float> AwaitTask(object taskObject, PropertyInfo property, HookInfo hookInfo, Action<HookResult> resultCallback)
        {
            var start = DateTime.Now;
            var maxTime = DateTime.Now;
            var timeout = ExLoader.Loader.Config.Hooks.CoroutineTimeout;

            while (!property.Get<bool>(taskObject))
            {
                yield return Timing.WaitForOneFrame;

                if (DateTime.Now >= maxTime)
                {
                    resultCallback(new HookResult(null, HookResultType.TimedOut));
                    yield break;
                }
            }

            var resultProp = property.DeclaringType.Property("Result");
            var resultVal = resultProp.Get(taskObject);

            resultCallback(new HookResult(resultVal, HookResultType.Success));
        }
    }
}
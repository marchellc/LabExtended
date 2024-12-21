using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Extensions;

using Mirror;

using NorthwoodLib.Pools;

using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

using static UnityEngine.LowLevel.PlayerLoopSystem;

namespace LabExtended.Utilities.Unity
{
    public static class PlayerLoopHelper
    {
        private static Action mirrorRuntimeInitialize =
            typeof(NetworkLoop).FindMethod(x => x.Name == "RuntimeInitializeOnLoad")
                .CreateDelegate(typeof(Action)) as Action;
        
        public struct CustomBeforePlayerLoop { }
        public struct CustomAfterPlayerLoop { }

        public static event Action BeforeLoop;
        public static event Action AfterLoop;

        public static PlayerLoopSystem DefaultSystem => PlayerLoop.GetDefaultPlayerLoop();

        public static PlayerLoopSystem System
        {
            get => PlayerLoop.GetCurrentPlayerLoop();
            set => PlayerLoop.SetPlayerLoop(value);
        }

        public static void ResetSystem()
        {
            System = DefaultSystem;

            if (ApiLoader.ApiConfig.LoopSection.ModifyLoops)
            {
                ModifySystem(x =>
                {
                    x.InjectBefore<Initialization.ProfilerStartFrame>(InvokeBefore, typeof(CustomBeforePlayerLoop));
                    x.InjectAfter<PostLateUpdate.UpdateVideo>(InvokeAfter, typeof(CustomAfterPlayerLoop));

                    return x;
                });
            }

            mirrorRuntimeInitialize();
        }

        public static void ModifySystem(Func<PlayerLoopSystem, PlayerLoopSystem?> modifier)
        {
            if (modifier is null)
                throw new ArgumentNullException(nameof(modifier));

            var system = System;
            var newSystem = modifier(system);

            if (!newSystem.HasValue)
                return;

            System = newSystem.Value;
        }

        public static bool InjectBefore<TSystem>(this PlayerLoopSystem system, UpdateFunction method, Type systemType)
            => InjectBefore(system, method, typeof(TSystem), systemType);

        public static bool InjectBefore<TCustom, TSystem>(this PlayerLoopSystem system, UpdateFunction method)
            => InjectBefore<TSystem>(system, method, typeof(TCustom));

        public static bool InjectAfter<TSystem>(this PlayerLoopSystem system, UpdateFunction method, Type systemType)
            => InjectAfter(system, method, typeof(TSystem), systemType);

        public static bool InjectAfter<TCustom, TSystem>(this PlayerLoopSystem system, UpdateFunction method)
            => InjectAfter<TSystem>(system, method, typeof(TCustom));

        public static ref PlayerLoopSystem GetParentSystem<T>(this PlayerLoopSystem system)
            => ref GetParentSystem(system, typeof(T));

        public static ref PlayerLoopSystem GetSystem<T>(this PlayerLoopSystem system)
            => ref GetSystem(system, typeof(T));

        public static Type GetParentType<T>(this PlayerLoopSystem system)
            => GetParentType(system, typeof(T));

        public static Type GetParentType(this PlayerLoopSystem system, Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var systems = new Stack<PlayerLoopSystem>();

            systems.Push(system);

            while (systems.Count > 0) 
            {
                var parent = systems.Pop();

                if (parent.subSystemList != null)
                {
                    for (int i = 0; i < parent.subSystemList.Length; i++)
                    {
                        var subSystem = parent.subSystemList[i];

                        if (subSystem.type != null && subSystem.type == type)
                            return parent.type;

                        systems.Push(subSystem);
                    }
                }
            }

            return null;
        }

        public static ref PlayerLoopSystem GetParentSystem(this PlayerLoopSystem system, Type type)
        {
            var parentType = GetParentType(system, type);

            if (parentType != null)
                return ref GetSystem(system, parentType);

            throw new Exception($"Player loop parent type {type.FullName} doesn't exist");
        }

        public static ref PlayerLoopSystem GetSystem(this PlayerLoopSystem system, Type type)
        {
            var length = system.subSystemList.Length;

            for (int i = 0; i < length; i++)
            {
                ref var subSystemA = ref system.subSystemList[i];

                if (subSystemA.type == type)
                    return ref subSystemA;

                var lengthA = subSystemA.subSystemList?.Length;

                for (int a = 0; a < lengthA; a++)
                {
                    ref var subSystemB = ref subSystemA.subSystemList[a];

                    if (subSystemB.type == type)
                        return ref subSystemB;

                    var lengthB = subSystemB.subSystemList?.Length;

                    for (int b = 0; b < lengthB; b++)
                    {
                        ref var subSystemC = ref subSystemB.subSystemList[b];

                        if (subSystemC.type == type)
                            return ref subSystemC;

                        var lengthC = subSystemC.subSystemList?.Length;

                        for (int c = 0; c < lengthC; c++)
                        {
                            ref var subSystemD = ref subSystemC.subSystemList[c];

                            if (subSystemD.type == type)
                                return ref subSystemD;
                        }
                    }
                }
            }

            throw new Exception($"Player loop system type {type} doesn't exist");
        }

        public static bool InjectAfter(this PlayerLoopSystem system, UpdateFunction method, Type targetType, Type systemType)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (systemType is null)
                throw new ArgumentNullException(nameof(systemType));

            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            ref var parentSystem = ref GetParentSystem(system, targetType);

            for (int i = 0; i < parentSystem.subSystemList.Length; i++)
            {
                ref var subSystem = ref parentSystem.subSystemList[i];

                if (subSystem.type == systemType)
                    return false;
            }

            for (int i = 0; i < parentSystem.subSystemList.Length; i++)
            {
                ref var subSystem = ref parentSystem.subSystemList[i];

                if (subSystem.type == targetType)
                {
                    var customSystem = new PlayerLoopSystem();

                    customSystem.type = systemType;
                    customSystem.updateDelegate = method;

                    var subSystemList = ListPool<PlayerLoopSystem>.Shared.Rent(parentSystem.subSystemList);

                    subSystemList.Insert(i, customSystem);

                    parentSystem.subSystemList = ListPool<PlayerLoopSystem>.Shared.ToArrayReturn(subSystemList);
                    return true;
                }
            }

            return false;
        }

        public static bool InjectBefore(this PlayerLoopSystem system, UpdateFunction method, Type targetType, Type systemType)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (systemType is null)
                throw new ArgumentNullException(nameof(systemType));

            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            ref var parentSystem = ref GetParentSystem(system, targetType);

            for (int i = 0; i < parentSystem.subSystemList.Length; i++)
            {
                ref var subSystem = ref parentSystem.subSystemList[i];

                if (subSystem.type == systemType)
                    return false;
            }

            for (int i = -1; i < parentSystem.subSystemList.Length; i++)
            {
                if ((i + 1) < parentSystem.subSystemList.Length)
                {
                    ref var subSystem = ref parentSystem.subSystemList[i + 1];

                    if (subSystem.type == targetType)
                    {
                        var customSystem = new PlayerLoopSystem();

                        customSystem.type = systemType;
                        customSystem.updateDelegate = method;

                        var subSystemList = ListPool<PlayerLoopSystem>.Shared.Rent(parentSystem.subSystemList);

                        subSystemList.Insert(i < 0 ? 0 : i, customSystem);

                        parentSystem.subSystemList = ListPool<PlayerLoopSystem>.Shared.ToArrayReturn(subSystemList);
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool RemoveSystem(this PlayerLoopSystem system, Type targetType)
        {
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            ref var parentSystem = ref GetParentSystem(system, targetType);

            var copy = ListPool<PlayerLoopSystem>.Shared.Rent(parentSystem.subSystemList);

            for (int i = 0; i < parentSystem.subSystemList.Length; i++)
            {
                ref var subSystem = ref parentSystem.subSystemList[i];

                if (subSystem.type == targetType)
                {
                    copy.RemoveAt(i);

                    parentSystem.subSystemList = ListPool<PlayerLoopSystem>.Shared.ToArrayReturn(copy);
                    return true;
                }
            }

            ListPool<PlayerLoopSystem>.Shared.Return(copy);
            return false;
        }

        public static void RemoveSystems(this PlayerLoopSystem system, Predicate<PlayerLoopSystem> predicate)
        {
            var removeList = ListPool<Type>.Shared.Rent();

            for (int i = 0; i < system.subSystemList.Length; i++)
            {
                if (system.subSystemList[i].subSystemList is null || system.subSystemList[i].type is null)
                    continue;

                for (int y = 0; y < system.subSystemList[i].subSystemList.Length; y++)
                {
                    var sub = system.subSystemList[i].subSystemList[y];

                    if (sub.type is null)
                        continue;

                    if (!predicate(sub))
                        continue;

                    removeList.Add(sub.type);
                }

                if (removeList.Count > 0)
                {
                    var copyList = ListPool<PlayerLoopSystem>.Shared.Rent(system.subSystemList[i].subSystemList);

                    ref var copyRef = ref system.subSystemList[i];

                    copyList.RemoveAll(p => p.type != null && removeList.Contains(p.type));
                    copyRef.subSystemList = ListPool<PlayerLoopSystem>.Shared.ToArrayReturn(copyList);
                }
            }

            ListPool<Type>.Shared.Return(removeList);
        }

        public static string GetPlayerLoopNames(this PlayerLoopSystem system, string indent = "    ")
        {
            var builder = StringBuilderPool.Shared.Rent();

            if (system.subSystemList != null)
            {
                var list = new Stack<Tuple<int, PlayerLoopSystem>>();

                list.Push(new Tuple<int, PlayerLoopSystem>(0, system));

                while (list.Count > 0)
                {
                    var tuple = list.Pop();
                    var depth = tuple.Item1;

                    if (tuple.Item2.type != null)
                    {
                        for (int i = 0; i < depth; i++)
                            builder.Append(indent);

                        builder.Append(tuple.Item2.type.FullName);
                        builder.AppendLine();
                    }
                    else
                    {
                        depth--;
                    }

                    if (tuple.Item2.subSystemList != null)
                    {
                        for (int i = 0; i < tuple.Item2.subSystemList.Length; i++)
                        {
                            list.Push(new Tuple<int, PlayerLoopSystem>(depth + 1, tuple.Item2.subSystemList[i]));
                        }
                    }
                }
            }

            return StringBuilderPool.Shared.ToStringReturn(builder);
        }

        private static void InvokeAfter() => AfterLoop.InvokeSafe();
        private static void InvokeBefore() => BeforeLoop.InvokeSafe();

        [LoaderInitialize(0)]
        internal static void InternalLoad()
        {
            if (ApiLoader.ApiConfig.LoopSection.ModifyLoops)
            {
                ModifySystem(x =>
                {
                    x.InjectBefore<Initialization.ProfilerStartFrame>(InvokeBefore, typeof(CustomBeforePlayerLoop));
                    x.InjectAfter<PostLateUpdate.UpdateVideo>(InvokeAfter, typeof(CustomAfterPlayerLoop));

                    var config = ApiLoader.ApiConfig.LoopSection;

                    x.RemoveSystems(s => !config.RequiredLoops.Contains(s.type.FullName) && !config.RequiredLoops.Contains(s.type.Name));
                    return x;
                });
            }
        }
    }
}
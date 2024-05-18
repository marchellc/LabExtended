using Common.Extensions;

using LabExtended.Core;

using MEC;

namespace LabExtended.API.Modules
{
    public class ModuleParent
    {
        private readonly Dictionary<Type, ModuleContainer> _modules;
        private readonly object _coroutineLock;
        private readonly CoroutineHandle _coroutine;

        public virtual bool UpdateModules { get; }

        public ModuleParent()
        {
            _coroutineLock = new object();
            _modules = new Dictionary<Type, ModuleContainer>();
            _coroutine = Timing.RunCoroutine(UpdateAll());
        }

        public T AddModule<T>(bool autoStart = true) where T : Module
        {
            lock (_coroutineLock)
            {
                if (_modules.TryGetValue(typeof(T), out var activeModule))
                    return (T)activeModule.Module;

                var module = typeof(T).Construct<T>();
                var container = new ModuleContainer(module);

                _modules[typeof(T)] = container;

                module.Parent = this;

                if (autoStart)
                    module.Start();

                return module;
            }
        }

        public T GetModule<T>() where T : Module
        {
            lock (_coroutineLock)
            {
                if (!_modules.TryGetValue(typeof(T), out var moduleContainer))
                    throw new Exception($"Module of type '{typeof(T).FullName}' has not been added.");

                return (T)moduleContainer.Module;
            }
        }

        public bool RemoveModule<T>() where T : Module
        {
            lock (_coroutineLock)
            {
                if (!_modules.TryGetValue(typeof(T), out var moduleContainer))
                    return false;

                try
                {
                    moduleContainer.Module.Stop();
                    moduleContainer.Module.Parent = null;
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Module Parent", $"Failed to stop module '{typeof(T).FullName}' due to an exception:\n{ex}");
                }

                return _modules.Remove(typeof(T));
            }
        }

        public void RemoveAllModules()
        {
            lock (_coroutineLock)
            {
                foreach (var moduleContainer in _modules.Values)
                {
                    try
                    {
                        moduleContainer.Module.Stop();
                        moduleContainer.Module.Parent = null;
                    }
                    catch (Exception ex)
                    {
                        ExLoader.Error("Module Parent", $"Failed to stop module '{moduleContainer.Module.GetType().FullName}' due to an exception:\n{ex}");
                    }
                }

                _modules.Clear();
            }
        }

        public void StopModules()
        {
            RemoveAllModules();

            if (Timing.IsRunning(_coroutine))
                Timing.KillCoroutines(_coroutine);
        }

        private IEnumerator<float> UpdateAll()
        {
            while (true)
            {
                yield return Timing.WaitForOneFrame;

                if (!UpdateModules)
                    continue;

                lock (_coroutineLock)
                {
                    foreach (var pair in _modules)
                    {
                        if (pair.Value.TickStatus.CanTick())
                        {
                            pair.Value.TickStatus.PreTick();

                            try
                            {
                                pair.Value.Module.Tick();
                            }
                            catch (Exception ex)
                            {
                                ExLoader.Error("Module Parent", $"Caught an exception while ticking module '{pair.Key.FullName}':\n{ex}");
                            }

                            pair.Value.TickStatus.PostTick();
                        }
                    }
                }
            }
        }
    }
}
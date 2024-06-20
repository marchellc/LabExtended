using Common.Extensions;

using LabExtended.Core;
using LabExtended.Extensions;

using MEC;

namespace LabExtended.Modules
{
    /// <summary>
    /// A class used to manage modules.
    /// </summary>
    public class ModuleParent
    {
        internal readonly Dictionary<Type, ModuleContainer> _modules;

        private readonly object _coroutineLock;
        private readonly CoroutineHandle _coroutine;

        /// <summary>
        /// When overriden, gets a value indicating whether a tick can occur.
        /// </summary>
        public virtual bool UpdateModules { get; } = true;

        /// <summary>
        /// When overriden, gets a value indicating whether or not to keep transient modules.
        /// </summary>
        public virtual bool KeepTransientModules { get; } = true;

        /// <summary>
        /// Creates a new <see cref="ModuleParent"/> instance.
        /// </summary>
        public ModuleParent()
        {
            _coroutineLock = new object();
            _modules = new Dictionary<Type, ModuleContainer>();
            _coroutine = Timing.RunCoroutine(UpdateAll());
        }

        /// <summary>
        /// This method gets executed each frame.
        /// </summary>
        public virtual void OnSelfUpdate() { }

        /// <summary>
        /// Adds a new/gets the existing module.
        /// </summary>
        /// <typeparam name="T">The module type to add/get.</typeparam>
        /// <param name="autoStart">Whether or not to automatically start the module (if adding).</param>
        /// <returns>The added/retrieved module instance.</returns>
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
                {
                    module.Start();
                    module.IsActive = true;
                }

                ExLoader.Debug("Modules API", $"Added module &3{typeof(T).FullName}&r");
                return module;
            }
        }

        /// <summary>
        /// Retrieves a module instance.
        /// </summary>
        /// <typeparam name="T">The type of the module to retrieve.</typeparam>
        /// <returns>The module instance, if found.</returns>
        /// <exception cref="Exception">Occurs if no modules of that type were found.</exception>
        public T GetModule<T>() where T : Module
        {
            lock (_coroutineLock)
            {
                if (!_modules.TryGetValue(typeof(T), out var moduleContainer))
                    throw new Exception($"Module of type '{typeof(T).FullName}' has not been added.");

                return (T)moduleContainer.Module;
            }
        }

        /// <summary>
        /// Removes a module instance.
        /// </summary>
        /// <typeparam name="T">The type of the module to remove.</typeparam>
        /// <returns>A value indicating whether or not the module was succesfully removed.</returns>
        public bool RemoveModule<T>() where T : Module
        {
            lock (_coroutineLock)
            {
                if (!_modules.TryGetValue(typeof(T), out var moduleContainer))
                    return false;

                try
                {
                    if (moduleContainer.Module is TransientModule transientModule)
                        transientModule._isForced = true;

                    moduleContainer.Module.IsActive = false;
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

        /// <summary>
        /// Removes all active modules.
        /// </summary>
        public void RemoveAllModules()
        {
            lock (_coroutineLock)
            {
                var containers = _modules.Values.ToList();

                foreach (var moduleContainer in containers)
                {
                    try
                    {
                        moduleContainer.Module.IsActive = false;
                        moduleContainer.Module.Stop();
                        moduleContainer.Module.Parent = null;

                        if (moduleContainer.Module is TransientModule transientModule && transientModule.KeepActive)
                            transientModule.IsActive = true;
                    }
                    catch (Exception ex)
                    {
                        ExLoader.Error("Module Parent", $"Failed to stop module &3{moduleContainer.Module.GetType().FullName}&r due to an exception:\n{ex}");
                    }
                }

                _modules.Clear();
            }
        }

        /// <summary>
        /// Removes all modules and stops this module parent.
        /// </summary>
        public void StopModules()
        {
            RemoveAllModules();

            if (Timing.IsRunning(_coroutine))
                Timing.KillCoroutines(_coroutine);
        }

        /// <summary>
        /// Gets a value indicating whether or not a module is active.
        /// </summary>
        /// <typeparam name="T">The type of module to find.</typeparam>
        /// <returns>A value indicating whether or not a module is active.</returns>
        public bool HasModule<T>() where T : Module
            => _modules.Any(p => p.Value?.Module != null && p.Value.Module is T);

        /// <summary>
        /// Retrieves all <see cref="TransientModule"/> instances.
        /// </summary>
        /// <returns>All active <see cref="TransientModule"/> instances.</returns>
        public IEnumerable<TransientModule> GetTransient()
            => _modules.Values.Where(p => p.Module != null && p.Module is TransientModule).Select(p => (TransientModule)p.Module);

        private IEnumerator<float> UpdateAll()
        {
            while (true)
            {
                yield return Timing.WaitForOneFrame;

                try
                {
                    OnSelfUpdate();
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Module Parent", $"Caught an exception while executing self update!\n{ex.ToColoredString()}");
                }

                if (!UpdateModules)
                    continue;

                lock (_coroutineLock)
                {
                    foreach (var pair in _modules)
                    {
                        if (!pair.Value.Module.IsActive)
                            continue;

                        if (pair.Value.TickStatus is null)
                            continue;

                        if (pair.Value.TickStatus.CanTick())
                        {
                            pair.Value.TickStatus.PreTick();

                            try
                            {
                                pair.Value.Module.Tick();
                            }
                            catch (Exception ex)
                            {
                                ExLoader.Error("Module Parent", $"Caught an exception while ticking module &3'{pair.Key.FullName}'&r:\n{ex.ToColoredString()}");
                            }

                            pair.Value.TickStatus.PostTick();
                        }
                    }
                }
            }
        }
    }
}
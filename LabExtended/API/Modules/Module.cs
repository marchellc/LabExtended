using LabExtended.Core.Ticking;
using LabExtended.Core.Ticking.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.API.Modules
{
    /// <summary>
    /// A class that represents a custom module.
    /// </summary>
    public class Module
    {
        internal readonly Dictionary<Type, Module> _modules = new Dictionary<Type, Module>();
        internal readonly HashSet<Type> _cache = new HashSet<Type>();

        private TickHandle _handle;

        /// <summary>
        /// Gets all submodules.
        /// </summary>
        public IReadOnlyCollection<Module> Modules => _modules.Values;

        /// <summary>
        /// Gets all cached module types.
        /// </summary>
        public IReadOnlyCollection<Type> Cache => _cache;

        /// <summary>
        /// Gets the handle for the <see cref="OnTick"/> method.
        /// </summary>
        public TickHandle TickHandle => _handle;

        /// <summary>
        /// Gets or sets the options for the <see cref="OnTick"/> method.
        /// </summary>
        public virtual ITickOptions TickOptions { get; }

        /// <summary>
        /// Gets or sets the timer for the <see cref="OnTick"/> method.
        /// </summary>
        public virtual ITickTimer TickTimer { get; }

        /// <summary>
        /// Gets or sets the tick distributor type.
        /// </summary>
        public virtual Type TickType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this module is active.
        /// </summary>
        public bool IsActive { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this module's tick method is paused.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Gets the module's parent.
        /// </summary>
        public Module Parent { get; internal set; }

        public void StartModule()
        {
            IsActive = true;

            if (TickType != null)
            {
                var distributor = TickDistribution.GetDistributor(TickType);
                var handle = TickDistribution.CreateWith(TickModule, TickOptions, TickTimer);

                _handle = distributor.CreateHandle(handle);
            }

            OnStarted();
        }

        public void StopModule()
        {
            IsActive = false;

            if (_handle.IsActive)
            {
                _handle.Destroy();
                _handle = default;
            }

            foreach (var subModule in _modules)
            {
                subModule.Value.IsActive = false;
                subModule.Value.StopModule();

                OnModuleRemoved(subModule.Value);

                subModule.Value.Parent = null;
            }

            _modules.Clear();

            OnStopped();
        }

        public void AddCachedModules(bool clearCache = false)
        {
            if (_cache.Count < 1)
                return;

            foreach (var type in _cache)
                AddModule(type, false);

            if (clearCache)
                _cache.Clear();
        }

        public bool HasModule(Type moduleType)
            => _modules.ContainsKey(moduleType);

        public bool HasModule<T>() where T : Module
            => _modules.ContainsKey(typeof(T));

        public bool HasModule<T>(out T module) where T : Module
            => (_modules.TryGetValue(typeof(T), out var instance) ? module = (T)instance : module = null) != null;

        public bool HasModule(Type moduleType, out Module instance)
            => _modules.TryGetValue(moduleType, out instance);

        public T AddModule<T>(bool addToCache = true) where T : Module
        {
            if (HasModule<T>(out var module))
                return module;

            module = typeof(T).Construct<T>();

            if (!module.ValidateAdd(this))
                throw new Exception($"Module {typeof(T).FullName} cannot be added to module {GetType().FullName}");

            module.Parent = this;
            module.StartModule();

            _modules[typeof(T)] = module;

            if (addToCache)
                _cache.Add(typeof(T));

            OnModuleAdded(module);
            return module;
        }

        public T AddModule<T>(T module, bool addToCache = true) where T : Module
        {
            if (module is null)
                throw new ArgumentNullException(nameof(module));

            if (HasModule(module.GetType(), out var instance))
                return (T)instance;

            module.Parent?.RemoveModule(module);

            if (module.IsActive)
            {
                module.IsActive = false;
                module.StopModule();
            }

            if (!module.ValidateAdd(this))
                throw new Exception($"Module {typeof(T).FullName} cannot be added to module {GetType().FullName}");

            module.Parent = this;
            module.StartModule();

            if (addToCache)
                _cache.Add(module.GetType());

            OnModuleAdded(module);

            _modules[typeof(T)] = module;
            return module;
        }

        public Module AddModule(Type moduleType, bool addToCache = true)
        {
            if (moduleType is null)
                throw new ArgumentNullException(nameof(moduleType));

            if (HasModule(moduleType, out var module))
                return module;

            module = moduleType.Construct<Module>();

            if (!module.ValidateAdd(this))
                throw new Exception($"Module {moduleType.FullName} cannot be added to module {GetType().FullName}");

            module.Parent = this;
            module.StartModule();

            if (addToCache)
                _cache.Add(moduleType);

            OnModuleAdded(module);

            _modules[moduleType] = module;
            return module;
        }

        public T GetModule<T>() where T : Module
        {
            if (HasModule<T>(out var module))
                return module;

            return null;
        }

        public Module GetModule(Type moduleType)
        {
            if (HasModule(moduleType, out var instance))
                return instance;

            return null;
        }

        public bool RemoveModule<T>(bool removeFromCache = true) where T : Module
            => RemoveModule(typeof(T), removeFromCache);

        public bool RemoveModule<T>(T module, bool removeFromCache = true) where T : Module
        {
            if (module is null)
                throw new ArgumentNullException(nameof(module));

            if (!_modules.ContainsKey(module.GetType()))
                return false;

            module.StopModule();

            if (removeFromCache)
                _cache.Remove(typeof(T));

            OnModuleRemoved(module);

            module.Parent = null;
            return _modules.Remove(module.GetType());
        }

        public bool RemoveModule(Type moduleType, bool removeFromCache = true)
        {
            if (_modules.TryGetValue(moduleType, out var module))
            {
                module.StopModule();
                module.Parent = null;

                OnModuleRemoved(module);
            }

            if (removeFromCache)
                _cache.Remove(moduleType);

            return _modules.Remove(moduleType);
        }

        /// <summary>
        /// Method called repeatedly as configured in <see cref="TickSettings"/>.
        /// </summary>
        public virtual void OnTick() { }

        public virtual void OnStarted() { }
        public virtual void OnStopped() { }

        public virtual void OnModuleAdded(Module module) { }
        public virtual void OnModuleRemoved(Module module) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public virtual bool ValidateAdd(Module module)
            => module != null;

        internal void TickModule()
        {
            if (!IsActive || IsPaused)
                return;

            OnTick();
        }
    }
}
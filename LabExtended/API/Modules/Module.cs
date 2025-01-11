using LabExtended.API.Collections.Locked;
using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities.Unity;

using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LabExtended.API.Modules
{
    /// <summary>
    /// A class that represents a custom module.
    /// </summary>
    public class Module
    {
        public struct ModuleUpdateLoop { }
        
        private static LockedList<Module> _allModules = new LockedList<Module>();

        internal Dictionary<Type, Module> _modules = new Dictionary<Type, Module>();
        internal HashSet<Type> _cache = new HashSet<Type>();

        internal float _moduleTime = 0f;
        
        /// <summary>
        /// Gets all submodules.
        /// </summary>
        public IReadOnlyCollection<Module> Modules => _modules.Values;

        /// <summary>
        /// Gets all cached module types.
        /// </summary>
        public IReadOnlyCollection<Type> Cache => _cache;
        
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

        /// <summary>
        /// Gets the module's update delay.
        /// </summary>
        public virtual float UpdateDelay { get; } = -1f;

        public void StartModule()
        {
            IsActive = true;

            _allModules.Add(this);

            OnStarted();
        }

        public void StopModule()
        {
            IsActive = false;

            foreach (var subModule in _modules)
            {
                subModule.Value.IsActive = false;
                subModule.Value.StopModule();

                OnModuleRemoved(subModule.Value);

                subModule.Value.Parent = null;
            }

            _modules.Clear();
            _allModules.Remove(this);

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
        public virtual void Update() { }

        public virtual void OnStarted() { }
        public virtual void OnStopped() { }

        public virtual void OnModuleAdded(Module module) { }
        public virtual void OnModuleRemoved(Module module) { }

        public virtual bool ValidateAdd(Module module)
            => module != null;

        private static void OnUpdate()
        {
            foreach (var module in _allModules)
            {
                if (!module.IsActive || module.IsPaused || module.UpdateDelay < 0f)
                    continue;

                if (module.UpdateDelay > 0f)
                {
                    module._moduleTime -= Time.deltaTime;
                    
                    if (module._moduleTime > 0f)
                        continue;

                    module._moduleTime = module.UpdateDelay;
                }

                try
                {
                    module.Update();
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Module API", $"Failed to update module &1{module.GetType().Name}&r:\n{ex.ToColoredString()}");
                }
            }
        }

        [LoaderInitialize(1)]
        private static void Init()
        {
            PlayerLoopHelper.ModifySystem(x =>
                x.InjectAfter<TimeUpdate.WaitForLastPresentationAndUpdateTime>(OnUpdate, typeof(ModuleUpdateLoop)) ? x : null);
        }
    }
}
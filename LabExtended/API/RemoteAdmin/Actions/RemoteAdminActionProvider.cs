using LabExtended.Attributes;
using LabExtended.Extensions;
using LabExtended.Utilities;
using NetworkManagerUtils.Dummies;

using NorthwoodLib.Pools;
using Utils.NonAllocLINQ;

// ReSharper disable ValueParameterNotUsed

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.RemoteAdmin.Actions;

/// <summary>
/// Used to provide Dummy actions as a <see cref="IRootDummyActionProvider"/>.
/// </summary>
public class RemoteAdminActionProvider : IDisposable, IRootDummyActionProvider
{
    /// <summary>
    /// Gets a collection of found module types.
    /// </summary>
    public static HashSet<Type> ModuleTypes { get; } = new();
    
    /// <summary>
    /// Gets the target player.
    /// </summary>
    public ExPlayer Player { get; }

    /// <summary>
    /// Gets the list of Remote Admin Action modules.
    /// </summary>
    public List<RemoteAdminActionModule> Modules { get; internal set; } =
        ListPool<RemoteAdminActionModule>.Shared.Rent();

    /// <summary>
    /// Whether or not dummy action list has been modified.
    /// </summary>
    public bool DummyActionsDirty { get; set; } = true;

    internal RemoteAdminActionProvider(ExPlayer player)
    {
        Player = player;
        
        ModuleTypes.ForEach(type => AddModule(type));
    }
    
    /// <summary>
    /// Attempts to get a module.
    /// </summary>
    /// <param name="predicate">The filtering predicate.</param>
    /// <param name="module">The resolved module.</param>
    /// <returns>true if the module was found</returns>
    public bool TryGetModule<TModule>(Predicate<TModule> predicate, out TModule module) where TModule : RemoteAdminActionModule
    {
        for (var i = 0; i < Modules.Count; i++)
        {
            if (Modules[i] is not TModule target)
                continue;
            
            if (!predicate(target))
                continue;
            
            module = target;
            return true;
        }
        
        module = null;
        return false;
    }

    /// <summary>
    /// Attempts to get a module.
    /// </summary>
    /// <param name="predicate">The filtering predicate.</param>
    /// <param name="module">The resolved module.</param>
    /// <returns>true if the module was found</returns>
    public bool TryGetModule(Predicate<RemoteAdminActionModule> predicate, out RemoteAdminActionModule module)
    {
        for (var i = 0; i < Modules.Count; i++)
        {
            var target = Modules[i];
            
            if (!predicate(target))
                continue;
            
            module = target;
            return true;
        }
        
        module = null;
        return false;
    }
    
    /// <summary>
    /// Adds a new Remote Admin Action module.
    /// </summary>
    /// <typeparam name="TModule">The type of module to add.</typeparam>
    /// <returns>The added module instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TModule AddModule<TModule>() where TModule : RemoteAdminActionModule
        => (TModule)AddModule(typeof(TModule));
    
    /// <summary>
    /// Adds a new Remote Admin Action module.
    /// </summary>
    /// <param name="moduleType">The type of the module to add.</param>
    /// <returns>The added module instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public RemoteAdminActionModule AddModule(Type moduleType)
    {
        if (moduleType is null)
            throw new ArgumentNullException(nameof(moduleType));
        
        if (Activator.CreateInstance(moduleType) is not RemoteAdminActionModule module)
            throw new Exception($"Could not instantiate RemoteAdminActionModule {moduleType.FullName}");

        module.Player = Player;
        module.Provider = this;
        
        module.Initialize();
        
        Modules.Add(module);

        DummyActionsDirty = true;
        return module;
    }

    /// <summary>
    /// Removes a module.
    /// </summary>
    /// <param name="module">The module to remove.</param>
    /// <returns>true if the module was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveModule(RemoteAdminActionModule module)
    {
        if (module is null)
            throw new ArgumentNullException(nameof(module));

        if (!Modules.Remove(module))
            return false;
        
        module.Dispose();

        DummyActionsDirty = true;
        return true;
    }
    
    /// <summary>
    /// Populates a list of dummy actions.
    /// </summary>
    /// <param name="actionAdder">The delegate used to add actions to the list.</param>
    /// <param name="categoryAdder">The delegate used to add categories to the list.</param>
    public void PopulateDummyActions(Action<DummyAction> actionAdder, Action<string> categoryAdder)
    {
        if (actionAdder is null)
            throw new ArgumentNullException(nameof(actionAdder));
        
        if (categoryAdder is null)
            throw new ArgumentNullException(nameof(categoryAdder));

        for (var i = 0; i < Modules.Count; i++)
        {
            var module = Modules[i];

            categoryAdder(module.Name);

            for (var x = 0; x < module.Actions.Count; x++)
            {
                actionAdder(module.Actions[x].ToDummyAction());
            }
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        DummyActionsDirty = false;
        
        if (Modules != null)
        {
            Modules.ForEach(module => module.Dispose());
            
            ListPool<RemoteAdminActionModule>.Shared.Return(Modules);
        }
        
        Modules = null;
    }

    private static void OnDiscovered(Type type)
    {
        if (!typeof(RemoteAdminActionModule).IsAssignableFrom(type) 
            || type == typeof(RemoteAdminActionModule)
            || type.HasAttribute<LoaderIgnoreAttribute>())
            return;

        ModuleTypes.Add(type);
    }

    internal static void Internal_Init()
    {
        ReflectionUtils.Discovered += OnDiscovered;
    }
}
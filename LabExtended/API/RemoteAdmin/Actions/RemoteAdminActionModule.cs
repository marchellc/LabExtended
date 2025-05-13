using NorthwoodLib.Pools;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.RemoteAdmin.Actions;

/// <summary>
/// Base class for Remote Admin Action modules.
/// </summary>
public class RemoteAdminActionModule : IDisposable
{
    /// <summary>
    /// Gets the name of the module (displayed as category).
    /// </summary>
    public virtual string Name { get; } = string.Empty;

    /// <summary>
    /// Gets the parent player.
    /// </summary>
    public ExPlayer Player { get; internal set; }
    
    /// <summary>
    /// Gets the parent provider.
    /// </summary>
    public RemoteAdminActionProvider Provider { get; internal set; }

    /// <summary>
    /// Gets a list of registered actions.
    /// </summary>
    public List<RemoteAdminAction> Actions { get; private set; } = ListPool<RemoteAdminAction>.Shared.Rent();

    /// <summary>
    /// Attempts to get an action.
    /// </summary>
    /// <param name="predicate">The filtering predicate.</param>
    /// <param name="action">The resolved action.</param>
    /// <typeparam name="TAction">The type to cast the action to.</typeparam>
    /// <returns>true if the action was resolved</returns>
    public bool TryGetAction<TAction>(Predicate<TAction> predicate, out TAction action) where TAction : RemoteAdminAction
    {
        for (var i = 0; i < Actions.Count; i++)
        {
            if (Actions[i] is not TAction target)
                continue;
            
            if (!predicate(target))
                continue;
            
            action = target;
            return true;
        }
        
        action = null;
        return false;
    }
    
    /// <summary>
    /// Attempts to get an action.
    /// </summary>
    /// <param name="predicate">The filtering predicate.</param>
    /// <param name="action">The resolved action.</param>
    /// <returns>true if the action was resolved</returns>
    public bool TryGetAction(Predicate<RemoteAdminAction> predicate, out RemoteAdminAction action)
    {
        for (var i = 0; i < Actions.Count; i++)
        {
            var target = Actions[i];
            
            if (!predicate(target))
                continue;
            
            action = target;
            return true;
        }
        
        action = null;
        return false;
    }
    
    /// <summary>
    /// Adds a new action.
    /// </summary>
    /// <typeparam name="TAction">The type of action to add.</typeparam>
    /// <returns>The added action instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TAction AddAction<TAction>() where TAction : RemoteAdminAction
        => (TAction)AddAction(typeof(TAction));
    
    /// <summary>
    /// Adds a new action.
    /// </summary>
    /// <param name="actionType">The type of action to add.</param>
    /// <returns>The added action instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public RemoteAdminAction AddAction(Type actionType)
    {
        if (actionType is null)
            throw new ArgumentNullException(nameof(actionType));

        if (Activator.CreateInstance(actionType) is not RemoteAdminAction action)
            throw new Exception($"Could not instantiate RemoteAdminAction {actionType.FullName}");

        action.Player = Player;
        action.Module = this;
        
        action.Initialize();
        
        Actions.Add(action);

        Provider.DummyActionsDirty = true;
        return action;
    }

    /// <summary>
    /// Removes an action.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    /// <returns>true if the action was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveAction(RemoteAdminAction action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        if (!Actions.Remove(action))
            return false;
        
        action.Dispose();

        Provider.DummyActionsDirty = true;
        return true;
    }

    /// <summary>
    /// Used to initialize this module.
    /// </summary>
    public virtual void Initialize()
    {
        
    }

    /// <summary>
    /// Gets called once the parent player leaves.
    /// </summary>
    public virtual void Dispose()
    {
        if (Actions != null)
        {
            Actions.ForEach(action => action.Dispose());
            
            ListPool<RemoteAdminAction>.Shared.Return(Actions);
        }
        
        Actions = null;
    }
}
using HarmonyLib;

using LabExtended.Commands.Interfaces;
using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands;

/// <summary>
/// Represents a registered command.
/// </summary>
public class CommandInstance
{
    /// <summary>
    /// Gets the command's type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the command's static instance.
    /// <remarks>null if <see cref="IsStatic"/> is false</remarks>
    /// </summary>
    public CommandBase StaticInstance { get; }
    
    /// <summary>
    /// Gets the command's parent.
    /// </summary>
    public CommandInstance Parent { get; }
    
    /// <summary>
    /// Gets the command's instance pool.
    /// </summary>
    public List<CommandBase> DynamicPool { get; } = new();
    
    /// <summary>
    /// Gets the command's overloads.
    /// </summary>
    public List<CommandOverload> Overloads { get; } = new();
    
    /// <summary>
    /// Gets the command's children.
    /// </summary>
    public List<CommandInstance> Children { get; }
    
    /// <summary>
    /// Gets the constructor of the command's type.
    /// </summary>
    public Func<object[], object> Constructor { get; }
    
    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the command's full name (including it's parent).
    /// </summary>
    public string FullName { get; }
    
    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Gets the command's aliases (full aliases).
    /// </summary>
    public List<string> Aliases { get; }
    
    /// <summary>
    /// Whether or not this command can be used in the Remote Admin panel.
    /// </summary>
    public bool SupportsRemoteAdmin { get; }
    
    /// <summary>
    /// Whether or not this command can be used in the server console.
    /// </summary>
    public bool SupportsServer { get; }
    
    /// <summary>
    /// Whether or not this command can be used in the player console.
    /// </summary>
    public bool SupportsPlayer { get; }
    
    /// <summary>
    /// Whether or not this command should use a static instance.
    /// </summary>
    public bool IsStatic { get; }
    
    /// <summary>
    /// Whether or not this command supports continuations.
    /// </summary>
    public bool IsContinuable { get; }
    
    /// <summary>
    /// Creates a new <see cref="CommandInstance"/> instance.
    /// </summary>
    public CommandInstance(Type type, CommandInstance parent, List<CommandInstance> children, string name, 
        string description, string fullName, bool isStatic, List<string> aliases)
    {
        Name = name;
        Type = type;
        Parent = parent;
        Aliases = aliases;
        FullName = fullName;
        Children = children;
        Description = description;

        SupportsRemoteAdmin = type.InheritsType<IRemoteAdminCommand>();
        SupportsServer = type.InheritsType<IServerCommand>();
        SupportsPlayer = type.InheritsType<IPlayerCommand>();

        IsContinuable = type.InheritsType<ContinuableCommandBase>();
        IsStatic = isStatic && !IsContinuable;
        
        Constructor = FastReflection.ForConstructor(AccessTools.DeclaredConstructor(type));

        if (IsStatic)
            StaticInstance = (CommandBase)Constructor(Array.Empty<object>());
    }

    /// <summary>
    /// Retrieves a new command instance, either from the pool (if allowed), or a new instance (or a pooled one if allowed).
    /// </summary>
    /// <returns>The retrieved command instance.</returns>
    public CommandBase GetInstance()
    {
        if (IsStatic)
            return StaticInstance;

        if (ApiLoader.ApiConfig.CommandSection.AllowInstancePooling && DynamicPool.Count > 0)
            return DynamicPool.RemoveAndTake(0);

        if (Constructor([]) is not CommandBase instance)
            throw new Exception($"Could not construct type {Type.FullName}");

        return instance;
    }
}
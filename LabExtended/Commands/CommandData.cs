using HarmonyLib;

using LabExtended.Commands.Interfaces;

using LabExtended.Core;
using LabExtended.Core.Pooling;
using LabExtended.Utilities;
using LabExtended.Extensions;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands;

/// <summary>
/// Represents a registered command.
/// </summary>
public class CommandData
{
    /// <summary>
    /// Gets the command's type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the command's static instance.
    /// <remarks>null if <see cref="IsStatic"/> is false</remarks>
    /// </summary>
    public CommandBase? StaticInstance { get; }
    
    /// <summary>
    /// Gets the command's default overload (if any).
    /// </summary>
    public CommandOverload? DefaultOverload { get; internal set; }

    /// <summary>
    /// Gets the command's path.
    /// </summary>
    public List<string> Path { get; } = new();
    
    /// <summary>
    /// Gets the command's aliases.
    /// </summary>
    public List<string> Aliases { get; }

    /// <summary>
    /// Gets the command's instance pool.
    /// </summary>
    public InstancePool<CommandBase> Pool { get; }
    
    /// <summary>
    /// Gets the command's overloads.
    /// </summary>
    public Dictionary<string, CommandOverload> Overloads { get; } = new();
    
    /// <summary>
    /// Gets the constructor of the command's type.
    /// </summary>
    public Func<object[], object> Constructor { get; }
    
    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the permission required by this command.
    /// </summary>
    public string Permission { get; }
    
    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public string Description { get; }
    
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
    /// Whether or not this command should be hidden from Remote Admin's suggestions.
    /// </summary>
    public bool IsHidden { get; }
    
    /// <summary>
    /// Whether or not this command supports continuations.
    /// </summary>
    public bool IsContinuable { get; }
    
    /// <summary>
    /// Gets the timeout of a continuable command.
    /// </summary>
    public float? TimeOut { get; }
    
    /// <summary>
    /// Creates a new <see cref="CommandData"/> instance.
    /// </summary>
    public CommandData(Type type, string name, string permission, string description, bool isStatic, bool isHidden, float? timeOut, List<string> aliases)
    {
        Name = name;
        Type = type;
        TimeOut = timeOut;
        Aliases = aliases;
        Permission = permission;
        Description = description;

        SupportsRemoteAdmin = type.InheritsType<IRemoteAdminCommand>();
        SupportsServer = type.InheritsType<IServerConsoleCommand>();
        SupportsPlayer = type.InheritsType<IPlayerCommand>();

        IsContinuable = type.InheritsType<ContinuableCommandBase>();
        IsStatic = isStatic && !IsContinuable;
        IsHidden = isHidden;
        
        Constructor = FastReflection.ForConstructor(AccessTools.Constructor(type));

        if (IsStatic)
        {
            StaticInstance = (CommandBase)Constructor(Array.Empty<object>());

            if (StaticInstance is null)
                throw new Exception($"Could not construct static instance of {type.FullName}");
        }
        else
        {
            Pool = new();
        }
    }

    /// <summary>
    /// Retrieves a new command instance, either from the pool (if allowed), or a new instance (or a pooled one if allowed).
    /// </summary>
    /// <returns>The retrieved command instance.</returns>
    public CommandBase GetInstance()
    {
        if (IsStatic && StaticInstance != null)
            return StaticInstance;

        if (ApiLoader.ApiConfig.CommandSection.AllowInstancePooling && Pool != null)
            return Pool.Rent(() => Constructor([]) as CommandBase);

        if (Constructor([]) is not CommandBase instance)
            throw new Exception($"Could not construct type {Type.FullName}");
        
        return instance;
    }
}
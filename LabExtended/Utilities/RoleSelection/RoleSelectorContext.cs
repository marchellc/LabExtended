using LabExtended.API;

using PlayerRoles;

namespace LabExtended.Utilities.RoleSelection;

/// <summary>
/// Context for role selection.
/// </summary>
public class RoleSelectorContext
{
    /// <summary>
    /// Gets the human team queue.
    /// </summary>
    public Team[]? HumanQueue { get; internal set; }

    /// <summary>
    /// Gets the total role queue.
    /// </summary>
    public Team[]? TotalQueue { get; internal set; }
    
    /// <summary>
    /// Gets the context options.
    /// </summary>
    public RoleSelectorOptions Options { get; }

    /// <summary>
    /// The target result dictionary.
    /// </summary>
    public IDictionary<ExPlayer, RoleTypeId> Roles { get; }
    
    /// <summary>
    /// The source list of players.
    /// </summary>
    public List<ExPlayer> Players { get; }
    
    /// <summary>
    /// The filtering predicate.
    /// </summary>
    public Func<ExPlayer, bool> Predicate { get; }

    /// <summary>
    /// Creates a new <see cref="RoleSelectorContext"/> instance.
    /// </summary>
    /// <param name="targetDictionary">The target dictionary.</param>
    /// <param name="source">The source list of players.</param>
    /// <param name="predicate">The filtering predicate.</param>
    /// <param name="options">The selection options.</param>
    /// <param name="humanQueue">Human team queue.</param>
    /// <param name="totalQueue">Total team queue.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public RoleSelectorContext(IDictionary<ExPlayer, RoleTypeId> targetDictionary, List<ExPlayer> source, Func<ExPlayer, bool> predicate, 
        RoleSelectorOptions options, Team[] humanQueue, Team[] totalQueue)
    {
        Roles = targetDictionary ?? throw new ArgumentNullException(nameof(targetDictionary));
        Players = source ?? throw new ArgumentNullException(nameof(source));
        Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        HumanQueue = humanQueue ?? throw new ArgumentNullException(nameof(humanQueue));
        TotalQueue = totalQueue ?? throw new ArgumentNullException(nameof(totalQueue));
        Options = options;
    }
    
    /// <summary>
    /// Whether or not a specific option is enabled.
    /// </summary>
    /// <param name="options">The option to check.</param>
    /// <returns>true if the option's flag is present</returns>
    public bool HasOption(RoleSelectorOptions options)
        => (Options & options) == options;
}
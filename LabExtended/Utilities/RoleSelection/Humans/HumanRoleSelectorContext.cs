using LabExtended.API;

using NorthwoodLib.Pools;

using PlayerRoles;

namespace LabExtended.Utilities.RoleSelection.Humans;

/// <summary>
/// Context for human role selection.
/// </summary>
public class HumanRoleSelectorContext : IDisposable
{
    internal int roleClock = 0;
    internal int roleLength = 0;
    
    /// <summary>
    /// The human role queue.
    /// </summary>
    public RoleTypeId[]? Roles { get; set; }
    
    /// <summary>
    /// List of human player candidates.
    /// </summary>
    public List<ExPlayer> Candidates { get; private set; } = ListPool<ExPlayer>.Shared.Rent();

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (Candidates != null)
            ListPool<ExPlayer>.Shared.Return(Candidates);

        Roles = null;
        Candidates = null;
    }
}
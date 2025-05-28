using LabExtended.API;
using LabExtended.Core.Pooling.Pools;

using NorthwoodLib.Pools;

using PlayerRoles;
using PlayerRoles.RoleAssign;

namespace LabExtended.Utilities.RoleSelection.Scps;

/// <summary>
/// Context for SCP role selection.
/// </summary>
public class ScpRoleSelectorContext : IDisposable
{
    /// <summary>
    /// Gets the role selector context.
    /// </summary>
    public RoleSelectorContext Context { get; }

    /// <summary>
    /// Gets the role chances array.
    /// </summary>
    public float[] RoleChances { get; } = new float[ScpSpawner.SpawnableScps.Length];
    
    /// <summary>
    /// List of generated SCP players.
    /// </summary>
    public List<ExPlayer> Chosen { get; set; } = ListPool<ExPlayer>.Shared.Rent();
    
    /// <summary>
    /// List of enqueued SCP roles.
    /// </summary>
    public List<RoleTypeId> Roles { get; private set; } = ListPool<RoleTypeId>.Shared.Rent();

    /// <summary>
    /// List of backup SCP roles.
    /// </summary>
    public List<RoleTypeId> Backup { get; private set; } = ListPool<RoleTypeId>.Shared.Rent();
    
    /// <summary>
    /// Buffer for SCP player chances.
    /// </summary>
    public Dictionary<ExPlayer, float> Chances { get; private set; } = DictionaryPool<ExPlayer, float>.Shared.Rent();

    /// <summary>
    /// Buffer for selected SCP player chances.
    /// </summary>
    public Dictionary<ExPlayer, float> SelectedChances { get; private set; } = DictionaryPool<ExPlayer, float>.Shared.Rent();

    /// <summary>
    /// Creates a new <see cref="ScpRoleSelectorContext"/> instance.
    /// </summary>
    /// <param name="context">The role selection context.</param>
    public ScpRoleSelectorContext(RoleSelectorContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        
        Context = context;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (Chosen != null)
            ListPool<ExPlayer>.Shared.Return(Chosen);
        
        if (Roles != null)
            ListPool<RoleTypeId>.Shared.Return(Roles);
        
        if (Backup != null)
            ListPool<RoleTypeId>.Shared.Return(Backup);
        
        if (Chances != null)
            DictionaryPool<ExPlayer, float>.Shared.Return(Chances);
        
        if (SelectedChances != null)
            DictionaryPool<ExPlayer, float>.Shared.Return(SelectedChances);
        
        Chosen = null;
        Roles = null;
        Chances = null;
        Backup = null;
        SelectedChances = null;
    }
}
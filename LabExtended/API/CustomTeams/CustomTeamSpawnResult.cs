namespace LabExtended.API.CustomTeams;

/// <summary>
/// Contains information about a spawned wave.
/// </summary>
/// <typeparam name="TWave">The type of the wave.</typeparam>
public struct CustomTeamSpawnResult<TWave> where TWave : CustomTeamInstance
{
    /// <summary>
    /// Gets the spawned wave instance (or null if spawn failed).
    /// </summary>
    public TWave? SpawnedWave { get; }
    
    /// <summary>
    /// Gets the spawn fail reason (or null if spawn succeeded).
    /// </summary>
    public CustomTeamSpawnFail? SpawnFail { get; }

    /// <summary>
    /// Creates a new <see cref="CustomTeamSpawnResult{TWave}"/> instance.
    /// </summary>
    /// <param name="spawnedWave">The spawned wave (or null if failed).</param>
    /// <param name="spawnFail">The spawn fail reason (or null if none).</param>
    public CustomTeamSpawnResult(TWave? spawnedWave, CustomTeamSpawnFail? spawnFail)
    {
        SpawnedWave = spawnedWave;
        SpawnFail = spawnFail;
    }
}
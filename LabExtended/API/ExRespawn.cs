using LabExtended.Commands.Attributes;
using PlayerRoles;

using Respawning;
using Respawning.Waves;

namespace LabExtended.API;

[CommandPropertyAlias("respawn")]
public static class ExRespawn
{
    [CommandPropertyAlias("state")]
    public static WaveQueueState State => WaveManager.State;

    [CommandPropertyAlias("isSpawning")]
    public static bool IsSpawning => State is WaveQueueState.WaveSpawning;
    
    [CommandPropertyAlias("isSpawned")]
    public static bool IsSpawned => State is WaveQueueState.WaveSpawned;

    [CommandPropertyAlias("isSelected")]
    public static bool IsSelected => State is WaveQueueState.WaveSelected;
    
    [CommandPropertyAlias("isIdle")]
    public static bool IsIdle => State is WaveQueueState.Idle;
    
    [CommandPropertyAlias("ntf")]
    public static NtfSpawnWave NtfWave { get; } = WaveManager.TryGet<NtfSpawnWave>(out var spawnWave) ? spawnWave : default;
    
    [CommandPropertyAlias("ntfMini")]
    public static NtfMiniWave NtfMiniWave { get; } = WaveManager.TryGet<NtfMiniWave>(out var spawnWave) ? spawnWave : default;
    
    [CommandPropertyAlias("chaos")]
    public static ChaosSpawnWave ChaosWave { get; } = WaveManager.TryGet<ChaosSpawnWave>(out var spawnWave) ? spawnWave : default;
    
    [CommandPropertyAlias("chaosMini")]
    public static ChaosMiniWave ChaosMiniWave { get; } =  WaveManager.TryGet<ChaosMiniWave>(out var spawnWave) ? spawnWave : default;

    [CommandPropertyAlias("ntfTimer")]
    public static WaveTimer NtfTimer => NtfWave.Timer;
    
    [CommandPropertyAlias("ntfMiniTimer")]
    public static WaveTimer NtfMiniTimer => NtfMiniWave.Timer;

    [CommandPropertyAlias("chaosTimer")]
    public static WaveTimer ChaosTimer => ChaosWave.Timer;
    
    [CommandPropertyAlias("chaosMiniTimer")]
    public static WaveTimer ChaosMiniTimer => ChaosMiniWave.Timer;

    [CommandPropertyAlias("nextWave")]
    public static SpawnableWaveBase NextWave
    {
        get => WaveManager._nextWave;
        set => WaveManager._nextWave = value;
    }

    [CommandPropertyAlias("ntfTimeLeft")]
    public static TimeSpan NtfTimeLeft
    {
        get => TimeSpan.FromSeconds(NtfTimer.TimeLeft);
        set => NtfTimer.SetTime((float)value.TotalSeconds);
    }

    [CommandPropertyAlias("ntfMiniTimeLeft")]
    public static TimeSpan NtfMiniTimeLeft
    {
        get => TimeSpan.FromSeconds(NtfMiniTimer.TimeLeft);
        set => NtfMiniTimer.SetTime((float)value.TotalSeconds);
    }

    [CommandPropertyAlias("chaosTimeLeft")]
    public static TimeSpan ChaosTimeLeft
    {
        get => TimeSpan.FromSeconds(ChaosTimer.TimeLeft);
        set => ChaosTimer.SetTime((float)value.TotalSeconds);
    }

    [CommandPropertyAlias("chaosMiniTimeLeft")]
    public static TimeSpan ChaosMiniTimeLeft
    {
        get => TimeSpan.FromSeconds(ChaosMiniTimer.TimeLeft);
        set => ChaosMiniTimer.SetTime((float)value.TotalSeconds);
    }

    [CommandPropertyAlias("ntfInfluence")]
    public static float NtfInfluence
    {
        get => GetInfluence(Faction.FoundationStaff);
        set => SetInfluence(Faction.FoundationStaff, value);
    }

    [CommandPropertyAlias("chaosInfluence")]
    public static float ChaosInfluence
    {
        get => GetInfluence(Faction.FoundationEnemy);
        set => SetInfluence(Faction.FoundationEnemy, value);
    }

    public static bool Spawn<T>() where T : SpawnableWaveBase
    {
        if (!WaveManager.TryGet<T>(out var wave))
            return false;

        WaveManager.Spawn(wave);
        return true;
    }

    public static float GetInfluence(Faction team)
        => FactionInfluenceManager.Get(team);

    public static void SetInfluence(Faction team, float influence)
        => FactionInfluenceManager.Set(team, influence);

    public static void ResetInfluence()
        => FactionInfluenceManager.ServerResetInfluence();
}
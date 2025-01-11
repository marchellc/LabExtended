using PlayerRoles;

using Respawning;
using Respawning.Waves;

namespace LabExtended.API;

public static class ExRespawn
{
    public static WaveQueueState State => WaveManager.State;

    public static bool IsSpawning => State is WaveQueueState.WaveSpawning;
    public static bool IsSpawned => State is WaveQueueState.WaveSpawned;

    public static bool IsSelected => State is WaveQueueState.WaveSelected;
    public static bool IsIdle => State is WaveQueueState.Idle;
    
    public static NtfSpawnWave NtfWave { get; } = WaveManager.TryGet<NtfSpawnWave>(out var spawnWave) ? spawnWave : default;
    public static NtfMiniWave NtfMiniWave { get; } = WaveManager.TryGet<NtfMiniWave>(out var spawnWave) ? spawnWave : default;
    
    public static ChaosSpawnWave ChaosWave { get; } = WaveManager.TryGet<ChaosSpawnWave>(out var spawnWave) ? spawnWave : default;
    public static ChaosMiniWave ChaosMiniWave { get; } =  WaveManager.TryGet<ChaosMiniWave>(out var spawnWave) ? spawnWave : default;

    public static WaveTimer NtfTimer => NtfWave.Timer;
    public static WaveTimer NtfMiniTimer => NtfMiniWave.Timer;

    public static WaveTimer ChaosTimer => ChaosWave.Timer;
    public static WaveTimer ChaosMiniTimer => ChaosMiniWave.Timer;

    public static SpawnableWaveBase NextWave
    {
        get => WaveManager._nextWave;
        set => WaveManager._nextWave = value;
    }

    public static TimeSpan NtfTimeLeft
    {
        get => TimeSpan.FromSeconds(NtfTimer.TimeLeft);
        set => NtfTimer.SetTime((float)value.TotalSeconds);
    }

    public static TimeSpan NtfMiniTimeLeft
    {
        get => TimeSpan.FromSeconds(NtfMiniTimer.TimeLeft);
        set => NtfMiniTimer.SetTime((float)value.TotalSeconds);
    }

    public static TimeSpan ChaosTimeLeft
    {
        get => TimeSpan.FromSeconds(ChaosTimer.TimeLeft);
        set => ChaosTimer.SetTime((float)value.TotalSeconds);
    }

    public static TimeSpan ChaosMiniTimeLeft
    {
        get => TimeSpan.FromSeconds(ChaosMiniTimer.TimeLeft);
        set => ChaosMiniTimer.SetTime((float)value.TotalSeconds);
    }

    public static float NtfInfluence
    {
        get => GetInfluence(Faction.FoundationStaff);
        set => SetInfluence(Faction.FoundationStaff, value);
    }

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
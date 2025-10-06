using LabExtended.Events;

using LabExtended.Utilities;
using LabExtended.Utilities.Update;

using NorthwoodLib.Pools;

using UnityEngine;

using Random = UnityEngine.Random;

namespace LabExtended.API.CustomTeams;

/// <summary>
/// Base class for custom team wave timers.
/// </summary>
public abstract class CustomTeamTimer<TInstance> : IDisposable 
    where TInstance : CustomTeamInstance
{
    /// <summary>
    /// Describes the reason of a wave failing to spawn.
    /// </summary>
    public enum WaveFailureReason
    {
        /// <summary>
        /// The spawn did not fail.
        /// </summary>
        None,
        
        /// <summary>
        /// Spawn chance failed.
        /// </summary>
        ChanceNotMet,
        
        /// <summary>
        /// A random number between the minimum and maximum amount of players resulted in zero.
        /// </summary>
        RandomPlayerCountZero,
        
        /// <summary>
        /// A list of selected players was not enough to match the required player count.
        /// </summary>
        PlayerCountNotMet,
        
        /// <summary>
        /// An unknown failure in the parent team handler.
        /// </summary>
        Unknown
    }
    
    /// <summary>
    /// Minimum amount of waves to spawn.
    /// </summary>
    public abstract int MinWaveCount { get; }
    
    /// <summary>
    /// Maximum amount of waves to spawn.
    /// </summary>
    public abstract int MaxWaveCount { get; }
    
    /// <summary>
    /// Minimum number of players in a wave.
    /// </summary>
    public abstract int MinPlayers { get; }
    
    /// <summary>
    /// Maximum number of players in a wave.
    /// </summary>
    public abstract int MaxPlayers { get; }
    
    /// <summary>
    /// Minimum time of a wave spawn (in seconds).
    /// </summary>
    public abstract float MinWaveTime { get; }
    
    /// <summary>
    /// Maximum time of a wave spawn (in seconds).
    /// </summary>
    public abstract float MaxWaveTime { get; }
    
    /// <summary>
    /// The chance of a wave spawning if all conditions are met.
    /// </summary>
    public abstract float SpawnChance { get; }
    
    /// <summary>
    /// Whether or not the randomized player count must be met.
    /// </summary>
    public abstract bool RequirePlayerCount { get; }

    /// <summary>
    /// Gets the remaining amount of waves to spawn this round.
    /// </summary>
    public int WavesToSpawn { get; private set; }
    
    /// <summary>
    /// Gets the remaining time (in seconds) of a wave spawn.
    /// </summary>
    public float RemainingTime { get; private set; }
    
    /// <summary>
    /// A list of all spawned waves.
    /// </summary>
    public List<TInstance> SpawnedWaves { get; } = new();
    
    /// <summary>
    /// Gets the parent handler.
    /// </summary>
    public CustomTeamHandler<TInstance> Handler { get; internal set; }
    
    /// <summary>
    /// Gets called once a spawn fails.
    /// </summary>
    /// <param name="reason">The spawn failure reason.</param>
    public virtual void OnFailed(WaveFailureReason reason) { }
    
    /// <summary>
    /// Gets called once an instance is spawned.
    /// </summary>
    /// <param name="instance">The spawned instance.</param>
    public virtual void OnSpawned(TInstance instance) { }
    
    /// <summary>
    /// Gets called once the round starts.
    /// </summary>
    public virtual void OnRoundStarted() { }
    
    /// <summary>
    /// Gets called once the round ends.
    /// </summary>
    public virtual void OnRoundEnded() { }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    public virtual void Start()
    {
        InternalEvents.OnRoundStarted += OnStarted;
        InternalEvents.OnRoundEnded += OnEnding;
        
        PlayerUpdateHelper.Component.OnUpdate += Update;
    }
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public virtual void Dispose()
    {
        InternalEvents.OnRoundStarted -= OnStarted;
        InternalEvents.OnRoundEnded -= OnEnding;
        
        PlayerUpdateHelper.Component.OnUpdate -= Update;
        
        SpawnedWaves.Clear();
    }

    private void OnStarted()
    {
        if (MaxWaveCount < 1)
            return;

        if (MaxPlayers < 1)
            return;
        
        WavesToSpawn = Random.Range(MinWaveCount, MaxWaveCount);
        RemainingTime = Random.Range(MinWaveTime, MaxWaveTime);
        
        OnRoundStarted();
    }

    private void OnEnding()
    {
        SpawnedWaves.Clear();
        
        OnRoundEnded();
    }

    private void Update()
    {
        if (!ExRound.IsRunning || WavesToSpawn < 1)
            return;

        if (RemainingTime > 0f)
        {
            RemainingTime -= Time.deltaTime;
            return;
        }

        var spawnResult = TrySpawn();

        if (spawnResult != WaveFailureReason.None)
        {
            OnFailed(spawnResult);
        }
        else
        {
            WavesToSpawn--;
        }
        
        RemainingTime = Random.Range(MinWaveTime, MaxWaveTime);
    }

    private WaveFailureReason TrySpawn()
    {
        var chance = Mathf.Clamp(SpawnChance, 0f, 100f);

        if (chance == 0f)
            return WaveFailureReason.ChanceNotMet;

        if (chance != 100f && !WeightUtils.GetBool(chance, 100f - chance))
            return WaveFailureReason.ChanceNotMet;

        var playerCount = Random.Range(MinPlayers, MaxPlayers);

        if (playerCount < 1)
            return WaveFailureReason.RandomPlayerCountZero;

        var players = ListPool<ExPlayer>.Shared.Rent(playerCount);

        for (var i = 0; i < playerCount; i++)
        {
            for (var x = 0; x < ExPlayer.Count; x++)
            {
                var player = ExPlayer.Players[x];
                
                if (player?.ReferenceHub == null)
                    continue;
                
                if (!Handler.IsSpawnable(player))
                    continue;
                
                players.Add(player);
            }
        }

        if (players.Count < playerCount && RequirePlayerCount)
        {
            ListPool<ExPlayer>.Shared.Return(players);
            return WaveFailureReason.PlayerCountNotMet;
        }

        var instance = Handler.Spawn(players);
        
        ListPool<ExPlayer>.Shared.Return(players);

        if (instance.SpawnFail.HasValue)
            return instance.SpawnFail.Value is CustomTeamSpawnFail.NotEnoughPlayers
                ? WaveFailureReason.PlayerCountNotMet
                : WaveFailureReason.Unknown;

        instance.SpawnedWave.IsByTimer = true;
        
        SpawnedWaves.Add(instance.SpawnedWave);
        
        OnSpawned(instance.SpawnedWave);
        return WaveFailureReason.None;
    }
}
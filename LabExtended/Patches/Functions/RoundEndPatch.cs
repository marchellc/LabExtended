using CentralAuth;

using GameCore;

using HarmonyLib;

using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Utilities;

using MEC;

using PlayerRoles;
using PlayerStatsSystem;

using UnityEngine;

using Console = GameCore.Console;

namespace LabExtended.Patches.Functions;

/// <summary>
/// Implement's the custom Round Lock system.
/// </summary>
public static class RoundEndPatch
{
    /// <summary>
    /// Gets the reference to the RoundEnded event in RoundSummary.
    /// </summary>
    public static FastEvent<RoundSummary.RoundEnded> OnRoundEnded { get; } =
        FastEvents.DefineEvent<RoundSummary.RoundEnded>(typeof(RoundSummary), nameof(RoundSummary.OnRoundEnded));

    /// <summary>
    /// Gets or sets the coroutine used to check for round duration.
    /// </summary>
    public static IEnumerator<float> Coroutine { get; set; } = DefaultReplacement();

    /// <summary>
    /// Gets the instance of the RoundSummary component.
    /// </summary>
    public static RoundSummary Singleton => RoundSummary.singleton;

    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.Start))]
    private static bool Prefix(RoundSummary __instance)
    {
        RoundSummary.singleton = __instance;
        RoundSummary._singletonSet = true;
        RoundSummary.roundTime = 0;
        
        RoundSummary.Kills = 0;
        RoundSummary.KilledBySCPs = 0;
        RoundSummary.EscapedClassD = 0;
        RoundSummary.EscapedScientists = 0;

        __instance.IsRoundEnded = false;
        __instance.KeepRoundOnOne = !ConfigFile.ServerConfig.GetBool("end_round_on_one_player", true);

        Timing.RunCoroutine(Coroutine.CancelWith(__instance.gameObject), Segment.FixedUpdate);

        PlayerRoleManager.OnServerRoleSet += __instance.OnServerRoleSet;
        PlayerStats.OnAnyPlayerDied += __instance.OnAnyPlayerDied;

        return false;
    }

    private static IEnumerator<float> DefaultReplacement()
    {
        var time = Time.unscaledTime;
        
        while (Singleton != null)
        {
            yield return Timing.WaitForSeconds(2.5f);

            if (!Singleton.IsRoundEnded && (ExRound.IsRoundLocked
                || ReferenceHub.GetPlayerCount(ClientInstanceMode.ReadyClient) < 2) || !RoundSummary.RoundInProgress()
                || Time.unscaledTime - time < 15f)
                continue;

            var summary = new RoundSummary.SumInfo_ClassList();

            for (var i = 0; i < ExPlayer.Count; i++)
            {
                var player = ExPlayer.Players[i];

                switch (player.Role.Team)
                {
                    case Team.ClassD:
                        summary.class_ds++;
                        break;
                    
                    case Team.ChaosInsurgency:
                        summary.chaos_insurgents++;
                        break;
                    
                    case Team.FoundationForces:
                        summary.mtf_and_guards++;
                        break;
                    
                    case Team.Scientists:
                        summary.scientists++;
                        break;
                    
                    case Team.Flamingos:
                        summary.flamingos++;
                        break;

                    case Team.SCPs:
                    {
                        if (player.Role.IsScpButNotZombie)
                            summary.scps_except_zombies++;
                        else
                            summary.zombies++;

                        break;
                    }
                }

                yield return Timing.WaitForOneFrame;

                summary.warhead_kills = AlphaWarheadController.Detonated
                    ? AlphaWarheadController.Singleton.WarheadKills
                    : -1;
                
                yield return Timing.WaitForOneFrame;

                var foundationAndScientists = summary.mtf_and_guards + summary.scientists;
                var chaosAndClassD = summary.chaos_insurgents + summary.class_ds;
                var scps = summary.scps_except_zombies + summary.zombies;
                var classDCount = summary.class_ds + RoundSummary.EscapedClassD;
                var scientistCount = summary.scientists + RoundSummary.EscapedScientists;
                var flamingos = summary.flamingos;

                RoundSummary.SurvivingSCPs = summary.scps_except_zombies;
                
                var classDEscapes = Singleton.classlistStart.class_ds != 0
                    ? classDCount / Singleton.classlistStart.class_ds 
                    : 0;

                var scientistEscapes = Singleton.classlistStart.scientists != 0
                    ? scientistCount / Singleton.classlistStart.scientists
                    : 0;

                var survivingTeams = 0;

                if (foundationAndScientists > 0)
                    survivingTeams++;
                
                if (chaosAndClassD > 0)
                    survivingTeams++;

                if (scps > 0)
                    survivingTeams++;
                
                if (flamingos > 0)
                    survivingTeams++;
                
                if (Singleton._extraTargets > 0)
                    survivingTeams++;

                Singleton.IsRoundEnded = survivingTeams < 2;

                if (!Singleton.IsRoundEnded)
                    continue;

                var anyScientists = foundationAndScientists > 0;
                var anyClassD = chaosAndClassD > 0;
                var anyScps = scps > 0;
                var anyFlamingos = flamingos > 0;

                var winningTeam = RoundSummary.LeadingTeam.Draw;

                if (anyScientists)
                    winningTeam = RoundSummary.EscapedScientists < RoundSummary.EscapedClassD
                        ? RoundSummary.LeadingTeam.Draw
                        : RoundSummary.LeadingTeam.FacilityForces;
                else if (anyScps || (anyScps && anyClassD))
                    winningTeam = RoundSummary.EscapedClassD > RoundSummary.SurvivingSCPs
                        ? RoundSummary.LeadingTeam.ChaosInsurgency
                        : (RoundSummary.SurvivingSCPs > RoundSummary.EscapedScientists
                            ? RoundSummary.LeadingTeam.Anomalies
                            : RoundSummary.LeadingTeam.Draw); 
                else if (anyClassD)
                    winningTeam = RoundSummary.EscapedClassD >= RoundSummary.EscapedScientists
                        ? RoundSummary.LeadingTeam.ChaosInsurgency
                        : RoundSummary.LeadingTeam.Draw;
                else if (anyFlamingos)
                    winningTeam = RoundSummary.LeadingTeam.Flamingos;

                var endingArgs = new RoundEndingEventArgs(winningTeam);
                
                ServerEvents.OnRoundEnding(endingArgs);
                
                if (!endingArgs.IsAllowed)
                    continue;

                winningTeam = endingArgs.LeadingTeam;

                OnRoundEnded.InvokeEvent(null, winningTeam, summary);

                FriendlyFireConfig.PauseDetector = true;

                var log = $"Round finished! Anomalies: {scps} | Chaos: {chaosAndClassD} | Facility Forces: {foundationAndScientists} " +
                          $"| Class-D escape percentage: {classDEscapes} | Scientist escape percentage: {scientistEscapes}";
                
                Console.AddLog(log, Color.gray);
                ServerLogs.AddLog(ServerLogs.Modules.GameLogic, log, ServerLogs.ServerLogType.GameEvent);

                yield return Timing.WaitForSeconds(1.5f);

                var endedArgs = new RoundEndedEventArgs(winningTeam);
                
                ServerEvents.OnRoundEnded(endedArgs);

                var restartTime = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
                
                if (Singleton != null && endedArgs.ShowSummary)
                    Singleton.RpcShowRoundSummary(Singleton.classlistStart, summary, winningTeam, 
                        RoundSummary.EscapedClassD, RoundSummary.EscapedScientists, RoundSummary.KilledBySCPs,
                        restartTime, (int)RoundStart.RoundLength.TotalSeconds);

                Singleton._roundEndCoroutine =
                    Timing.RunCoroutine(Singleton.InitiateRoundEnd(restartTime), Segment.FixedUpdate);

                yield return Timing.WaitUntilDone(Singleton._roundEndCoroutine);
            }
        }
    }
}
﻿
using CentralAuth;

using GameCore;

using HarmonyLib;

using LabExtended.API;
using MEC;

using PlayerRoles;

using PlayerStatsSystem;

using PluginAPI.Core;

using PluginAPI.Events;

using Respawning;
using RoundRestarting;

using UnityEngine;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.Start))]
    public static class RoundEndPatch
    {
        public static IEnumerator<float> RoundProcessor { get; set; } = DefaultRoundProcessor();

        public static bool Prefix(RoundSummary __instance)
        {
            RoundSummary.singleton = __instance;
            RoundSummary._singletonSet = true;

            RoundSummary.roundTime = 0;

            __instance.KeepRoundOnOne = !ConfigFile.ServerConfig.GetBool("end_round_on_one_player", false);

            if (RoundProcessor != null)
                Timing.RunCoroutine(RoundProcessor, Segment.FixedUpdate);

            RoundSummary.KilledBySCPs = 0;
            RoundSummary.EscapedClassD = 0;
            RoundSummary.EscapedScientists = 0;
            RoundSummary.ChangedIntoZombies = 0;
            RoundSummary.Kills = 0;

            __instance.ChaosTargetCount = ReferenceHub.AllHubs.Count(hub => hub.GetTeam() == Team.ChaosInsurgency);

            PlayerRoleManager.OnServerRoleSet += __instance.OnServerRoleSet;
            RespawnManager.ServerOnRespawned += __instance.ServerOnRespawned;

            PlayerStatsSystem.PlayerStats.OnAnyPlayerDied += __instance.OnAnyPlayerDied;

            return false;
        }

        private static IEnumerator<float> DefaultRoundProcessor()
        {
            var time = Time.unscaledTime;
            var summary = RoundSummary.singleton;

            while (summary != null)
            {
                yield return Timing.WaitForSeconds(2f);

                if ((RoundSummary.RoundLock || ExRound.IsRoundLocked)
                    || (summary.KeepRoundOnOne && ReferenceHub.AllHubs.Count(x => x.authManager.InstanceMode != ClientInstanceMode.DedicatedServer) < 2) || !RoundSummary.RoundInProgress() || Time.unscaledTime - time < 15f)
                    continue;

                RoundSummary.SumInfo_ClassList newList = default(RoundSummary.SumInfo_ClassList);

                foreach (var player in ExPlayer.Players)
                {
                    if (!player.Switches.CanBlockRoundEnd)
                        continue;

                    switch (player.Role.Team)
                    {
                        case Team.ClassD:
                            newList.class_ds++;
                            break;

                        case Team.ChaosInsurgency:
                            newList.chaos_insurgents++;
                            break;

                        case Team.FoundationForces:
                            newList.mtf_and_guards++;
                            break;

                        case Team.Scientists:
                            newList.scientists++;
                            break;

                        case Team.SCPs:
                            if (player.Role.Type == RoleTypeId.Scp0492)
                                newList.zombies++;
                            else
                                newList.scps_except_zombies++;

                            break;
                    }
                }

                yield return float.NegativeInfinity;

                newList.warhead_kills = (AlphaWarheadController.Detonated ? AlphaWarheadController.Singleton.WarheadKills : (-1));

                yield return float.NegativeInfinity;

                var facilityForces = newList.mtf_and_guards + newList.scientists;
                var chaosInsurgency = newList.chaos_insurgents + newList.class_ds;
                var anomalies = newList.scps_except_zombies + newList.zombies;

                var num = newList.class_ds + RoundSummary.EscapedClassD;
                var num2 = newList.scientists + RoundSummary.EscapedScientists;

                RoundSummary.SurvivingSCPs = newList.scps_except_zombies;

                var dEscapePercentage = ((summary.classlistStart.class_ds != 0) ? (num / summary.classlistStart.class_ds) : 0);
                var sEscapePercentage = ((summary.classlistStart.scientists == 0) ? 1 : (num2 / summary.classlistStart.scientists));

                bool flag;

                if (newList.class_ds <= 0 && facilityForces <= 0 && summary.ChaosTargetCount == 0)
                {
                    flag = true;
                }
                else
                {
                    int num3 = 0;

                    if (facilityForces > 0)
                        num3++;

                    if (chaosInsurgency > 0)
                        num3++;

                    if (anomalies > 0)
                        num3++;

                    flag = num3 <= 1;
                }

                if (!summary._roundEnded)
                {
                    var cancellation = EventManager.ExecuteEvent<RoundEndConditionsCheckCancellationData>(new RoundEndConditionsCheckEvent(flag)).Cancellation;

                    if (cancellation != RoundEndConditionsCheckCancellationData.RoundEndConditionsCheckCancellation.ConditionsSatisfied)
                    {
                        if (cancellation == RoundEndConditionsCheckCancellationData.RoundEndConditionsCheckCancellation.ConditionsNotSatisfied && !summary._roundEnded)
                            continue;

                        if (flag)
                            summary._roundEnded = true;
                    }
                    else
                    {
                        summary._roundEnded = true;
                    }
                }

                if (!summary._roundEnded)
                {
                    continue;
                }

                var num4 = facilityForces > 0;

                var flag2 = chaosInsurgency > 0;
                var flag3 = anomalies > 0;

                var leadingTeam = RoundSummary.LeadingTeam.Draw;

                if (num4)
                    leadingTeam = ((RoundSummary.EscapedScientists < RoundSummary.EscapedClassD) ? RoundSummary.LeadingTeam.Draw : RoundSummary.LeadingTeam.FacilityForces);
                else if (flag3 || (flag3 && flag2))
                    leadingTeam = ((RoundSummary.EscapedClassD > RoundSummary.SurvivingSCPs) ? RoundSummary.LeadingTeam.ChaosInsurgency : ((RoundSummary.SurvivingSCPs > RoundSummary.EscapedScientists) ? RoundSummary.LeadingTeam.Anomalies : RoundSummary.LeadingTeam.Draw));
                else if (flag2)
                    leadingTeam = ((RoundSummary.EscapedClassD >= RoundSummary.EscapedScientists) ? RoundSummary.LeadingTeam.ChaosInsurgency : RoundSummary.LeadingTeam.Draw);

                var roundEndCancellationData = EventManager.ExecuteEvent<RoundEndCancellationData>(new RoundEndEvent(leadingTeam));

                while (roundEndCancellationData.IsCancelled)
                {
                    if (roundEndCancellationData.Delay <= 0f)
                        yield break;

                    yield return Timing.WaitForSeconds(roundEndCancellationData.Delay);

                    roundEndCancellationData = EventManager.ExecuteEvent<RoundEndCancellationData>(new RoundEndEvent(leadingTeam));
                }

                if (Statistics.FastestEndedRound.Duration > RoundStart.RoundLength)
                    Statistics.FastestEndedRound = new Statistics.FastestRound(leadingTeam, RoundStart.RoundLength, DateTime.Now);

                Statistics.CurrentRound.ClassDAlive = newList.class_ds;
                Statistics.CurrentRound.ScientistsAlive = newList.scientists;
                Statistics.CurrentRound.MtfAndGuardsAlive = newList.mtf_and_guards;
                Statistics.CurrentRound.ChaosInsurgencyAlive = newList.chaos_insurgents;
                Statistics.CurrentRound.ZombiesAlive = newList.zombies;
                Statistics.CurrentRound.ScpsAlive = newList.scps_except_zombies;
                Statistics.CurrentRound.WarheadKills = newList.warhead_kills;

                FriendlyFireConfig.PauseDetector = true;

                var text = "Round finished! Anomalies: " + anomalies + " | Chaos: " + chaosInsurgency + " | Facility Forces: " + facilityForces + " | D escaped percentage: " + dEscapePercentage + " | S escaped percentage: " + sEscapePercentage + ".";
                GameCore.Console.AddLog(text, Color.gray);

                ServerLogs.AddLog(ServerLogs.Modules.Logger, text, ServerLogs.ServerLogType.GameEvent);

                yield return Timing.WaitForSeconds(1.5f);

                var num5 = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);

                summary?.RpcShowRoundSummary(summary.classlistStart, newList, leadingTeam, RoundSummary.EscapedClassD, RoundSummary.EscapedScientists, RoundSummary.KilledBySCPs, num5, (int)RoundStart.RoundLength.TotalSeconds);

                yield return Timing.WaitForSeconds(num5 - 1);

                summary?.RpcDimScreen();

                yield return Timing.WaitForSeconds(1f);

                RoundRestart.InitiateRoundRestart();
            }
        }
    }
}

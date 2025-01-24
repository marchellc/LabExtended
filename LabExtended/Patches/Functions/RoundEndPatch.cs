using CentralAuth;

using GameCore;

using HarmonyLib;

using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API;
using LabExtended.Utilities;

using MEC;

using PlayerRoles;
using PlayerStatsSystem;

using RoundRestarting;

using UnityEngine;

namespace LabExtended.Patches.Functions
{
  public static class RoundEndPatch
  {
    public static FastEvent<RoundSummary.RoundEnded> OnRoundEnded { get; } =
      FastEvents.DefineEvent<RoundSummary.RoundEnded>(typeof(RoundSummary), nameof(RoundSummary.OnRoundEnded));

    public static Func<RoundSummary, IEnumerator<float>> GetRoundProcessor { get; set; } =
      x => DefaultRoundProcessor(x);

    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.Start))]
    public static bool Prefix(RoundSummary __instance)
    {
      RoundSummary.singleton = __instance;
      RoundSummary._singletonSet = true;

      RoundSummary.roundTime = 0;

      __instance.KeepRoundOnOne = !ConfigFile.ServerConfig.GetBool("end_round_on_one_player");

      if (GetRoundProcessor != null)
        Timing.RunCoroutine(GetRoundProcessor(__instance), Segment.FixedUpdate);

      RoundSummary.KilledBySCPs = 0;
      RoundSummary.EscapedClassD = 0;
      RoundSummary.EscapedScientists = 0;
      RoundSummary.ChangedIntoZombies = 0;
      RoundSummary.Kills = 0;

      PlayerRoleManager.OnServerRoleSet += __instance.OnServerRoleSet;
      PlayerStats.OnAnyPlayerDied += __instance.OnAnyPlayerDied;
      return false;
    }

    private static IEnumerator<float> DefaultRoundProcessor(RoundSummary summary)
    {
      var time = Time.unscaledTime;

      while (summary != null && summary)
      {
        yield return Timing.WaitForSeconds(2.5f);

        if ((summary._roundEnded || !ExRound.IsRoundLocked &&
              (!summary.KeepRoundOnOne || ReferenceHub.AllHubs.Count<ReferenceHub>(x =>
                x.authManager.InstanceMode != ClientInstanceMode.DedicatedServer) >= 2) &&
              RoundSummary.RoundInProgress())
            && (summary._roundEnded || Time.unscaledTime - time >= 15.0f))
        {
          var newList = new RoundSummary.SumInfo_ClassList();

          for (int i = 0; i < ExPlayer.Players.Count; i++)
          {
            var player = ExPlayer.Players[i];

            if (!player || !player.Switches.CanBlockRoundEnd)
              continue;

            switch (player.Role.Team)
            {
              case Team.SCPs:
                if (player.Role == RoleTypeId.Scp0492)
                {
                  ++newList.zombies;
                  continue;
                }

                ++newList.scps_except_zombies;
                continue;

              case Team.FoundationForces:
                ++newList.mtf_and_guards;
                continue;

              case Team.ChaosInsurgency:
                ++newList.chaos_insurgents;
                continue;

              case Team.Scientists:
                ++newList.scientists;
                continue;

              case Team.ClassD:
                ++newList.class_ds;
                continue;

              default:
                continue;
            }
          }

          yield return float.NegativeInfinity;

          newList.warhead_kills = AlphaWarheadController.Detonated ? AlphaWarheadController.Singleton.WarheadKills : -1;

          yield return float.NegativeInfinity;

          var num1 = newList.mtf_and_guards + newList.scientists;
          var num2 = newList.chaos_insurgents + newList.class_ds;
          var num3 = newList.scps_except_zombies + newList.zombies;
          var num4 = newList.class_ds + RoundSummary.EscapedClassD;
          var num5 = newList.scientists + RoundSummary.EscapedScientists;

          RoundSummary.SurvivingSCPs = newList.scps_except_zombies;

          var num6 = summary.classlistStart.class_ds == 0
            ? 0.0f
            : num4 / summary.classlistStart.class_ds;
          var num7 = summary.classlistStart.scientists == 0
            ? 1f
            : num5 / summary.classlistStart.scientists;

          int num8 = 0;

          if (num1 > 0)
            ++num8;

          if (num2 > 0)
            ++num8;

          if (num3 > 0)
            ++num8;

          if (num8 <= 0f)
            summary._roundEnded = true;

          if (summary.ExtraTargets > 0)
          {
            if (summary._roundEnded)
            {
              var num10 = num1 > 0 ? 1 : 0;

              var flag1 = num2 > 0;
              var flag2 = num3 > 0;

              var leadingTeam = RoundSummary.LeadingTeam.Draw;

              if (num10 != 0)
                leadingTeam = RoundSummary.EscapedScientists >= RoundSummary.EscapedClassD
                  ? RoundSummary.LeadingTeam.FacilityForces
                  : RoundSummary.LeadingTeam.Draw;
              else if (flag2 || flag2 & flag1)
                leadingTeam = RoundSummary.EscapedClassD > RoundSummary.SurvivingSCPs
                  ? RoundSummary.LeadingTeam.ChaosInsurgency
                  : (RoundSummary.SurvivingSCPs > RoundSummary.EscapedScientists
                    ? RoundSummary.LeadingTeam.Anomalies
                    : RoundSummary.LeadingTeam.Draw);
              else if (flag1)
                leadingTeam = RoundSummary.EscapedClassD >= RoundSummary.EscapedScientists
                  ? RoundSummary.LeadingTeam.ChaosInsurgency
                  : RoundSummary.LeadingTeam.Draw;

              var endingArgs = new RoundEndingEventArgs(leadingTeam);

              LabApi.Events.Handlers.ServerEvents.OnRoundEnding(endingArgs);

              if (endingArgs.IsAllowed)
              {
                leadingTeam = endingArgs.LeadingTeam;

                OnRoundEnded.InvokeEvent(null);

                FriendlyFireConfig.PauseDetector = true;

                string str = "Round finished! Anomalies: " + num3.ToString() + " | Chaos: " + num2.ToString() +
                             " | Facility Forces: " + num1.ToString() + " | D escaped percentage: " +
                             num6.ToString() + " | S escaped percentage: " + num7.ToString() + ".";

                GameCore.Console.AddLog(str, Color.gray);

                ServerLogs.AddLog(ServerLogs.Modules.GameLogic, str, ServerLogs.ServerLogType.GameEvent);

                yield return Timing.WaitForSeconds(1.5f);

                var endedArgs = new RoundEndedEventArgs(leadingTeam);

                LabApi.Events.Handlers.ServerEvents.OnRoundEnded(endedArgs);

                var showSummary = endedArgs.ShowSummary;
                var roundCd = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);

                if (summary != null && summary && showSummary)
                  summary.RpcShowRoundSummary(summary.classlistStart, newList, leadingTeam,
                    RoundSummary.EscapedClassD, RoundSummary.EscapedScientists, RoundSummary.KilledBySCPs, roundCd,
                    (int)RoundStart.RoundLength.TotalSeconds);

                yield return Timing.WaitForSeconds(roundCd - 1f);

                summary?.RpcDimScreen();

                yield return Timing.WaitForSeconds(1f);

                RoundRestart.InitiateRoundRestart();
                break;
              }
            }
          }
        }
      }
    }
  }
}

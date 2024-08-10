using HarmonyLib;

using CentralAuth;

using LabExtended.Core;
using LabExtended.Extensions;

using LabExtended.API.Npcs;

using Authenticator;

using NorthwoodLib;
using NorthwoodLib.Pools;

using GameCore;

namespace LabExtended.Patches.Npcs
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.RefreshOnlinePlayers))]
    public static class NpcNullReferenceVerificationLoopPatch
    {
        public static bool Prefix(ServerConsole __instance)
        {
            try
            {
                foreach (var hub in ReferenceHub.AllHubs)
                {
                    if (hub.Mode != ClientInstanceMode.ReadyClient || string.IsNullOrWhiteSpace(hub.authManager.UserId))
                        continue;

                    if (hub.isLocalPlayer || NpcHandler.IsNpc(hub))
                        continue;

                    ServerConsole.PlayersListRaw.objects.Add(hub.authManager.UserId);
                }

                ServerConsole._verificationPlayersList = JsonSerialize.ToJson(ServerConsole.PlayersListRaw);
                ServerConsole._playersAmount = ServerConsole.PlayersListRaw.objects.Count;
                ServerConsole.PlayersListRaw.objects.Clear();
            }
            catch (Exception ex)
            {
                ApiLoader.Error("LabExtended", $"An error ocurred in the game's verification processing loop!\n{ex.ToColoredString()}");
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.RefreshServerData))]
    public static class NpcNullReferenceServerDataLoopPatch
    {
        public static bool Prefix(ServerConsole __instance)
        {
            bool flag = true;
            byte b = 0;

            ServerConsole.RefreshEmailSetStatus();
            ServerConsole.RefreshToken(init: true);

            while (!ServerConsole._disposing)
            {
                b++;

                if (!flag && string.IsNullOrEmpty(ServerConsole.Password) && b < 15)
                {
                    if (b == 5 || b == 12 || ServerConsole.ScheduleTokenRefresh)
                    {
                        ServerConsole.RefreshToken();
                    }
                }
                else
                {
                    flag = false;
                    ServerConsole.Update = ServerConsole.Update || b == 10;
                    string text = string.Empty;
                    try
                    {
                        int count = ServerConsole.NewPlayers.Count;
                        int num = 0;

                        List<AuthenticatorPlayerObject> list = ListPool<AuthenticatorPlayerObject>.Shared.Rent();

                        while (!ServerConsole.NewPlayers.IsEmpty)
                        {
                            num++;

                            if (num > count + 30)
                                break;

                            try
                            {
                                if (ServerConsole.NewPlayers.TryTake(out var result) && result != null)
                                {
                                    if (result.authManager.AuthenticationResponse.AuthToken is null || result.authManager.AuthenticationResponse.SignedAuthToken is null)
                                        continue;

                                    if (result.authManager is null || result.authManager.InstanceMode != ClientInstanceMode.ReadyClient)
                                        continue;

                                    if (result.connectionToClient is null)
                                        continue;

                                    if (string.IsNullOrEmpty(result.authManager.UserId) || string.IsNullOrEmpty(result.connectionToClient.address))
                                        continue;

                                    string userId = result.authManager.UserId;
                                    string ip = result.authManager.connectionToClient == null || string.IsNullOrEmpty(result.authManager.connectionToClient.address) ? "N/A" : result.authManager.connectionToClient.address;
                                    string requestIp = result.authManager.AuthenticationResponse.AuthToken.RequestIp;

                                    int asn = result.authManager.AuthenticationResponse.AuthToken.Asn;

                                    list.Add(new AuthenticatorPlayerObject(userId, ip, requestIp, asn.ToString(), result.authManager.AuthenticationResponse.AuthToken.Serial, result.authManager.AuthenticationResponse.AuthToken.VacSession));
                                }
                            }
                            catch (Exception ex)
                            {
                                ServerConsole.AddLog("[VERIFICATION THREAD] Exception in New Player (inside of loop) processing: " + ex.Message);
                                ServerConsole.AddLog(ex.StackTrace);
                            }
                        }
                        text = JsonSerialize.ToJson(new AuthenticatorPlayerObjects(list));
                        ListPool<AuthenticatorPlayerObject>.Shared.Return(list);
                    }
                    catch (Exception ex2)
                    {
                        ServerConsole.AddLog("[VERIFICATION THREAD] Exception in New Players processing: " + ex2.Message);
                        ServerConsole.AddLog(ex2.StackTrace);
                    }

                    object obj;

                    if (!ServerConsole.Update)
                    {
                        obj = new List<string>
                        {
                            "ip=" + ServerConsole.Ip,
                            "players=" + ServerConsole._playersAmount + "/" + CustomNetworkManager.slots,
                            "newPlayers=" + text,
                            "port=" + ServerConsole.PortToReport,
                            "version=2"
                        };
                    }
                    else
                    {
                        obj = new List<string>
                        {
                            "ip=" + ServerConsole.Ip,
                            "players=" + ServerConsole._playersAmount + "/" + CustomNetworkManager.slots,
                            "playersList=" + ServerConsole._verificationPlayersList,
                             "newPlayers=" + text,
                            "port=" + ServerConsole.PortToReport,
                            "pastebin=" + ConfigFile.ServerConfig.GetString("serverinfo_pastebin_id", "7wV681fT"),
                            "gameVersion=" + GameCore.Version.VersionString,
                            "version=2",
                            "update=1",
                            "info=" + StringUtils.Base64Encode(__instance.RefreshServerNameSafe()).Replace('+', '-'),
                            "privateBeta=" + GameCore.Version.PrivateBeta,
                            "staffRA=" + ServerStatic.PermissionsHandler.StaffAccess,
                            "friendlyFire=" + ServerConsole.FriendlyFire
                        };

                        object obj2 = obj;
                        byte geoblocking = (byte)CustomLiteNetLib4MirrorTransport.Geoblocking;

                        ((List<string>)obj2).Add("geoblocking=" + geoblocking);
                        ((List<string>)obj).Add("modded=" + (CustomNetworkManager.Modded || ServerConsole.TransparentlyModdedServerConfig));
                        ((List<string>)obj).Add("tModded=" + ServerConsole.TransparentlyModdedServerConfig);
                        ((List<string>)obj).Add("whitelist=" + ServerConsole.WhiteListEnabled);
                        ((List<string>)obj).Add("accessRestriction=" + ServerConsole.AccessRestriction);
                        ((List<string>)obj).Add("emailSet=" + ServerConsole._emailSet);
                        ((List<string>)obj).Add("enforceSameIp=" + ServerConsole.EnforceSameIp);
                    }

                    List<string> list2 = (List<string>)obj;

                    if (!string.IsNullOrEmpty(ServerConsole.Password))
                    {
                        list2.Add("passcode=" + ServerConsole.Password);
                    }

                    ServerConsole.Update = false;

                    if (!AuthenticatorQuery.SendData(list2) && !ServerConsole._printedNotVerifiedMessage)
                    {
                        ServerConsole._printedNotVerifiedMessage = true;
                        ServerConsole.AddLog("Your server won't be visible on the public server list - (" + ServerConsole.Ip + ")", ConsoleColor.Red);

                        if (!ServerConsole._emailSet)
                        {
                            ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC, please set up your email in configuration file (\"contact_email\" value) and restart the server.", ConsoleColor.Red);
                        }
                        else
                        {
                            ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC please email following information:", ConsoleColor.Red);
                            ServerConsole.AddLog("- IP address of server (most likely " + ServerConsole.Ip + ")", ConsoleColor.Red);
                            ServerConsole.AddLog("- port of the server (currently the server is running on port " + ServerConsole.PortToReport + ")", ConsoleColor.Red);
                            ServerConsole.AddLog("- is this static or dynamic IP address (most of home adresses are dynamic)", ConsoleColor.Red);
                            ServerConsole.AddLog("PLEASE READ rules for verified servers first: https://scpslgame.com/Verified_server_rules.pdf", ConsoleColor.Red);
                            ServerConsole.AddLog("send us that information to: server.verification@scpslgame.com (server.verification at scpslgame.com)", ConsoleColor.Red);
                            ServerConsole.AddLog("if you can't see the AT sign in console (in above line): server.verification AT scpslgame.com", ConsoleColor.Red);
                            ServerConsole.AddLog("email must be sent from email address set as \"contact_email\" in your config file (current value: " + ConfigFile.ServerConfig.GetString("contact_email") + ").", ConsoleColor.Red);
                        }
                    }
                    else
                    {
                        ServerConsole._printedNotVerifiedMessage = true;
                    }
                }

                if (b >= 15)
                {
                    b = 0;
                }

                Thread.Sleep(5000);

                if (ServerConsole.ScheduleTokenRefresh || b == 0)
                {
                    ServerConsole.RefreshToken();
                }
            }

            return false;
        }
    }
}
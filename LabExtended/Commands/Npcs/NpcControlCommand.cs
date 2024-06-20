using CommandSystem;
using LabExtended.API;
using LabExtended.API.Npcs;
using MapGeneration;
using MEC;

using PlayerRoles;

using PluginAPI.Core;

using UnityEngine;

namespace LabExtended.Commands.Npcs
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class NpcControlCommand : ICommand
    {
        public string Command => "npccontrol";
        public string Description => "Allows you to control spawned NPCs.";

        public string[] Aliases { get; } = new string[] { "npcc" };

        public bool SanitizeResponse => false;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = $"Invalid usage!\nnpccontrol <npc ID> <npc action>";
                return false;
            }

            if (!Player.TryGet(sender, out var player))
            {
                response = "This command can only be used in the Remote Admin.";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out var npcId))
            {
                response = $"That is not a valid number.";
                return false;
            }

            if (!NpcHandler.TryGetById(npcId, out var npc))
            {
                response = $"Unknown NPC ID.";
                return false;
            }

            switch (arguments.At(1).ToLower().Trim())
            {
                case "despawn":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = $"That NPC is already despawned!";
                            return false;
                        }
                        else
                        {
                            npc.Despawn();

                            response = $"NPC despawned.";
                            return true;
                        }
                    }

                case "spawn":
                    {
                        if (npc.IsSpawned)
                        {
                            response = "That NPC is already spawned!";
                            return false;
                        }
                        else
                        {
                            npc.Player.Hub.roleManager.ServerSetRole(npc.PreviousRole == RoleTypeId.None ? player.Role : npc.PreviousRole, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);

                            response = $"NPC spawned!";
                            return true;
                        }
                    }

                case "role":
                    {
                        if (arguments.Count < 3)
                        {
                            response = $"Missing action parameters.\nnpccontrol {npcId} role <role ID>";
                            return false;
                        }

                        if (!Enum.TryParse<RoleTypeId>(arguments.At(2), out var newRole))
                        {
                            response = "Invalid role ID.";
                            return false;
                        }

                        npc.Player.Hub.roleManager.ServerSetRole(newRole, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);

                        response = $"NPC role set to {npc.Player.Role}";
                        return true;
                    }

                case "bring":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = "This NPC is not spawned!";
                            return false;
                        }

                        npc.Player.Position = player.Position;

                        response = $"Brought the specified NPC to your location.";
                        return true;
                    }

                case "goto":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = "This NPC is not spawned!";
                            return false;
                        }

                        if (arguments.Count < 3)
                        {
                            response = $"Missing action parameters.\nnpccontrol {npcId} goto <player>";
                            return false;
                        }

                        if (!Player.TryGetByName(arguments.At(2), out var targetPlayer))
                        {
                            response = "Unknown target player.";
                            return false;
                        }

                        npc.Player.Position = targetPlayer.Position;

                        response = $"Teleported the specified NPC to {targetPlayer.Nickname}";
                        return true;
                    }

                case "scale":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = "This NPC is not spawned!";
                            return false;
                        }

                        if (arguments.Count < 5)
                        {
                            response = $"Missing action arguments.\nnpccontrol {npcId} scale <scaleX> <scaleY> <scaleZ>";
                            return false;
                        }

                        if (!float.TryParse(arguments.At(2), out var scaleX) || !float.TryParse(arguments.At(3), out var scaleY) || !float.TryParse(arguments.At(4), out var scaleZ))
                        {
                            response = $"Failed to parse a scale axis.";
                            return false;
                        }

                        var scale = new Vector3(scaleX, scaleY, scaleZ);

                        if (npc.Player.Scale == scale)
                        {
                            response = $"This NPC is already the same scale.";
                            return false;
                        }

                        npc.Player.Scale = scale;

                        response = $"NPC scale changed to {scale}";
                        return true;
                    }

                case "visibility":
                    {
                        if (arguments.Count < 4)
                        {
                            response = $"Missing action arguments.\nnpccontrol {npcId} visibility <visibility type> <visibility status>\n" +
                                $"- <visibility status> is a boolean value (true / false)\n" +
                                $"- <visibility type> specifies which visibility to toggle (remoteadmin, spectatorlist, global)";
                            return false;
                        }

                        if (!bool.TryParse(arguments.At(3), out var status))
                        {
                            response = $"Invalid boolean value.";
                            return false;
                        }

                        switch (arguments.At(2).ToLower().Trim())
                        {
                            case "remoteadmin":
                                npc.Player.Switches.IsVisibleInRemoteAdmin = status;

                                response = $"NPC is now {(status ? "visible in" : "hidden from")} the Remote Admin panel.";
                                return true;

                            case "spectatorlist":
                                npc.Player.Switches.IsVisibleInSpectatorList = status;

                                response = $"NPC is now {(status ? "visible in" : "hidden from")} the Spectator List.";
                                return true;

                            case "global":
                                npc.Player.IsInvisible = !status;

                                response = $"NPC is now {(status ? "visible for" : "hidden for")} other players.";
                                return true;

                            default:
                                response = $"Invalid visibility type.";
                                return false;
                        }
                    }

                case "item":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = "This NPC is not spawned!";
                            return false;
                        }

                        if (arguments.Count < 3)
                        {
                            response = $"Missing action parameters.\nnpccontrol {npcId} item <item ID>";
                            return false;
                        }

                        if (!Enum.TryParse<ItemType>(arguments.At(2), true, out var itemId))
                        {
                            response = "Invalid item ID.";
                            return false;
                        }

                        npc.Player.CurrentItemType = itemId;

                        response = $"Set held item to {itemId}";
                        return true;
                    }

                case "sight":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = "This NPC is not spawned!";
                            return false;
                        }

                        var senderPlayer = ExPlayer.Get(player.ReferenceHub);

                        if (senderPlayer is null)
                        {
                            response = "An error occured - your Player object has not been found in the extended player API.";
                            return false;
                        }

                        if (npc.Player.IsInLineOfSight(senderPlayer))
                        {
                            response = $"You are in NPC's line of sight.";
                            return true;
                        }
                        else
                        {
                            response = $"You are NOT in NPC's line of sight.";
                            return true;
                        }
                    }

                case "follow":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = "This NPC is not spawned!";
                            return false;
                        }

                        var senderPlayer = ExPlayer.Get(player.ReferenceHub);

                        if (senderPlayer is null)
                        {
                            response = "An error occured - your Player object has not been found in the extended player API.";
                            return false;
                        }

                        npc.NavigationDestination = null;

                        if (npc.NavigationTarget != null && npc.NavigationTarget == senderPlayer)
                        {
                            npc.ResetNavigation();
                            response = "Disabled following.";
                            return true;
                        }
                        else
                        {
                            npc.NavigationTarget = senderPlayer;

                            response = "Started following.";
                            return true;
                        }
                    }

                case "navactive":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = "This NPC is not spawned!";
                            return false;
                        }

                        npc.IsNavigationActive = !npc.IsNavigationActive;

                        if (npc.IsNavigationActive)
                        {
                            response = "Navigation is now active.";
                            return true;
                        }

                        response = "Navigation disabled.";
                        return true;
                    }

                case "navinteractive":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = "This NPC is not spawned!";
                            return false;
                        }

                        npc.IsNavigationInteractable = !npc.IsNavigationInteractable;

                        if (npc.IsNavigationInteractable)
                        {
                            response = "Navigation interactions are now active.";
                            return true;
                        }

                        response = "Navigation interactions disabled.";
                        return true;
                    }

                case "navroom":
                    {
                        if (!npc.IsSpawned)
                        {
                            response = "This NPC is not spawned!";
                            return false;
                        }

                        npc.ResetNavigation();

                        if (arguments.Count < 3)
                        {
                            response = $"Missing action parameters.\nnpccontrol {npcId} navroom <room ID>";
                            return false;
                        }

                        if (!Enum.TryParse<RoomName>(arguments.At(2), true, out var roomId))
                        {
                            response = "Invalid room ID.";
                            return false;
                        }

                        var room = RoomIdentifier.AllRoomIdentifiers.FirstOrDefault(r => r.Name == roomId);

                        if (room is null)
                        {
                            response = "Failed to find that room.";
                            return false;
                        }

                        npc.NavigationDestination = room.transform.position;

                        response = $"Set navigation target to {room.Name} ({room.name})";
                        return true;
                    }

                case "navreset":
                    {
                        npc.ResetNavigation();

                        response = "Navigation reset.";
                        return true;
                    }

                default:
                    response = $"Unknown action!" +
                        $"\nValid actions:\n" +
                        $" - despawn (despawns the specified NPC)\n" +
                        $" - spawn (spawns the specified NPC at your position and as your role)\n" +
                        $" - role (sets the specified NPC's role to the specified role)\n" +
                        $" - bring (brings the specified NPC to your location)\n" +
                        $" - goto (teleports the specified NPC to the specified player)\n" +
                        $" - visibility (changes the status of a specified visibility type)\n" +
                        $" - scale (changes the NPC's model scale)\n" +
                        $" - item (changes the item held by the NPC)\n" +
                        $" - sight (tells you if the NPC can see you in it's line of sight)";
                    return false;
            }
        }
    }
}
using CommandSystem;

using LabExtended.Utilities;

using LabExtended.API;
using LabExtended.API.Voice;
using LabExtended.API.RemoteAdmin;

using PluginAPI.Core;

using MapGeneration;

using UnityEngine;

namespace LabExtended.Commands.Debug
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class DebugCommand : ICommand
    {
        public string Command => "debug";
        public string Description => "A set of useful debug commands.";

        public string[] Aliases { get; } = new string[] { };

        public bool SanitizeResponse => false;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = $"Missing arguments.\ndebug <action> (action arguments)\n" +
                    $"Currently available actions:\n" +
                    $" - addraobject\n" +
                    $" - removeraobject <object ID>\n" +
                    $" - toggleplayback\n" +
                    $" - togglespeakdebug\n" +
                    $" - setpitch\n" +
                    $" - spawnbounds\n" +
                    $" - spawnprimitive";

                return false;
            }

            if (!Player.TryGet(sender, out var apiPlayer))
            {
                response = "Failed to fetch your player object.";
                return false;
            }

            var player = ExPlayer.Get(apiPlayer.ReferenceHub);

            if (player is null)
            {
                response = "Failed to fetch your player object.";
                return false;
            }

            switch (arguments.At(0).Trim().ToLower())
            {
                case "addraobject":
                    {
                        var obj = new DebugRemoteAdminObject();

                        if (RemoteAdminUtils.TryAddObject(obj))
                        {
                            response = $"Added a new debug object. (ID: {obj.AssignedId})";
                            return true;
                        }
                        else
                        {
                            response = "Failed to add a new debug object.";
                            return false;
                        }
                    }

                case "removeraobject":
                    {
                        if (arguments.Count < 2)
                        {
                            response = "Missing action arguments.\ndebug removeraobject <object ID/name>";
                            return false;
                        }

                        if (!RemoteAdminUtils.TryRemoveObject(arguments.At(1)))
                        {
                            if (!int.TryParse(arguments.At(1), out var objectId) || !RemoteAdminUtils.TryRemoveObject(objectId))
                            {
                                response = "Failed to remove object.";
                                return false;
                            }
                            else
                            {
                                response = "Object removed!";
                                return true;
                            }
                        }
                        else
                        {
                            response = "Object removed!";
                            return true;
                        }
                    }

                case "toggleplayback":
                    {
                        if (player.VoiceFlags.HasFlag(API.Voice.VoiceFlags.CanHearSelf))
                        {
                            player.VoiceFlags.RemoveFlag(API.Voice.VoiceFlags.CanHearSelf);

                            response = "Playback disabled.";
                            return false;
                        }
                        else
                        {
                            player.VoiceFlags.AddFlag(API.Voice.VoiceFlags.CanHearSelf);

                            response = "Playback enabled.";
                            return false;
                        }
                    }

                case "togglespeakdebug":
                    {
                        VoiceSystem.ShowSpeakingDebug = !VoiceSystem.ShowSpeakingDebug;

                        response = VoiceSystem.ShowSpeakingDebug ? "Speaking debug ENABLED" : "Speaking debug DISABLED";
                        return true;
                    }

                case "setpitch":
                    {
                        if (arguments.Count < 2)
                        {
                            response = "Missing action arguments.\ndebug setpitch <pitch>";
                            return false;
                        }

                        if (!float.TryParse(arguments.At(1), out var pitch))
                        {
                            response = "That is not a vaid number.";
                            return false;
                        }

                        player.VoicePitch = pitch;

                        response = $"Pitch set to {player.VoicePitch}";
                        return true;
                    }

                case "spawnbounds":
                    {
                        var curRoom = RoomIdUtils.RoomAtPosition(player.Position);

                        if (curRoom is null)
                        {
                            response = "You aren't in a valid room.";
                            return false;
                        }

                        foreach (var bounds in curRoom.SubBounds)
                            PrimitiveUtils.Spawn(bounds.center, bounds.size, Quaternion.identity);

                        response = "Bounds spawned.";
                        return true;
                    }

                case "spawnprimitive":
                    {
                        PrimitiveUtils.Spawn(player.Position, player.Scale, player.Rotation);

                        response = "Primitive spawned.";
                        return true;
                    }

                default:
                    response = $"Unkown action.";
                    return false;
            }
        }
    }
}
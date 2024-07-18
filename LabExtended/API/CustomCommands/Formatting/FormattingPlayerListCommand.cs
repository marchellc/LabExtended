using CommandSystem;

using LabExtended.Commands;

namespace LabExtended.API.CustomCommands.Formatting
{
    public class FormattingPlayerListCommand : VanillaCommandBase
    {
        public override string Command => "playerlist";

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response =
                $"== Player List Argument Syntax ==\n" +
                $"This allows you to select a singular player or multiple players with expressions similar to Minecraft.\n" +
                $"\nBasic Selections:\n" +
                $" >- Player ID (just specify the player's ID, ex. sethp 5 50)\n" +
                $" >- Player Name (just use the player's nickname, ex. sethp random 50)\n" +
                $" >- Player User ID (just use the player's user ID, ex. sethp 76561190123456789@steam random 50)\n" +
                $" >- Wildcard (Use '*' as the argument, ex. sethp * 50)\n" +
                $"\nExpressions:\n" +
                $"It's important to keep in mind that expressions can be combined.\n" +
                $"Expressions are formatted as follows: type(list,value). Let's break it down.\n" +
                $" >- type stands for the selection type, supported are:\n" +
                $"  -< role (selects players with a specific role)\n" +
                $"  -< team (selects players with a specific team)\n" +
                $"  -< rand (selects random players)\n" +
                $"  -< tag (selects players with a specific tag)\n" +
                $" >- list stands for a list of players. This can either be supplied by using the wildcard ('*') for all players or another expression.\n" +
                $" >- value stands for the value to match.\n" +
                $"Let's look at some examples.\n" +
                $" >- role(*,1) - This would select players with role of ID 1 from all players.\n" +
                $" >- team(*,3) - This would select players with team of ID 3 from all players.\n" +
                $" >- rand(*,5) - This would select five random players from all players.\n" +
                $" >- rank(*, admin) - This would select players with the 'admin' tag from all players.\n" +
                $" >- tag(*, Administrator) - This would select players with the 'Administrator' group name from all players.\n" +
                $"Let's try combining them.\n" +
                $" >- rand(role(*,1),3)  - This would select three random players from a list of players with a role ID that matches 1.\n" +
                $" >- rand(tag(team(*,1),admin,2) - This would select all players that have a team ID of 1 and then from those players select those who have an 'admin' tag .. and then from those players it'd take 2 random players.";

            return true;
        }
    }
}
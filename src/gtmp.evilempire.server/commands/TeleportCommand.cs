using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.commands
{
    class TeleportCommand : Command
    {
        public override CommandInfo Info { get; }

        public ILoginService LoginService { get; set; }
        public Map Map { get; set; }

        public TeleportCommand(ServiceContainer services)
        {
            LoginService = services.Get<ILoginService>();
            Map = services.Get<Map>();
            Info = new CommandInfo { Name = "tp", Description = "Teleports yourself to a given map point / player or teleport another player to you.", Usage = "/tp [2me] [ p<Login> | c<Coordinate> | n<Named Point> ] e. g. /tp c28,238,24.5    /tp 2me pUser", IsAuthorized = c => c.UserGroup.IsAuthGroupOrHigher(entities.AuthUserGroup.GameMaster), Execute = Execute };
        }

        public bool Execute(IClient client, ParsedCommand command)
        {
            var teleportToMe = string.Compare("2me", command.Args.At(0), StringComparison.OrdinalIgnoreCase) == 0;
            string target = teleportToMe ? command.Args.At(1) : command.Args.At(0);

            if (target == null || target.Length < 1)
            {
                client.SendChatMessage("Unable to parse teleport target.");
            }
            var prefix = char.ToUpperInvariant(target[0]);
            var targetWithoutPrefix = target.Substring(1);
            switch (prefix)
            {
                case 'P':
                    return TeleportPlayerByLogin(client, targetWithoutPrefix, teleportToMe);
                case 'C':
                    if (teleportToMe)
                    {
                        return WithSemanticalError(client, "a coordinate");
                    }
                    return TeleportToCoordinate(client, targetWithoutPrefix);
                case 'N':
                    if (teleportToMe)
                    {
                        return WithSemanticalError(client, "a map point");
                    }
                    return TeleportToNamedPoint(client, targetWithoutPrefix);
                default:
                    client.SendChatMessage($"Invalid prefix \"{prefix}\" used for command.");
                    break;
            }

            return true;
        }

        bool WithSemanticalError(IClient client, string target)
        {
            client.SendChatMessage($"Semantical error. Can not teleport {target} to yourself.");
            return true;
        }


        bool TeleportPlayerByLogin(IClient client, string target, bool teleportToMe)
        {
            var targetClient = LoginService.FindLoggedInClientByLogin(target);
            if (targetClient == null)
            {
                var user = LoginService.FindUserByLogin(target);
                if (user == null)
                {
                    client.SendChatMessage($"There is no player with the login \"{target}\".");
                }
                else
                {
                    client.SendChatMessage($"The player with the login \"{target}\" is not logged in.");
                }
                return true;
            }

            if (teleportToMe)
            {
                targetClient.Position = client.Position;
            }
            else
            {
                client.Position = targetClient.Position;
            }
            return true;
        }

        bool TeleportToCoordinate(IClient client, string target)
        {
            var coordinate = target.AsVector3f();
            if (coordinate != null)
            {
                client.Position = coordinate.Value;
            }
            else
            { 
                client.SendChatMessage($"Unable to parse coordinate from value \"{target}\".");
            }
            return true;
        }

        bool TeleportToNamedPoint(IClient client, string target)
        {
            var namedMapPoint = Map.GetPointByName(target);
            if (namedMapPoint != null)
            {
                client.Position = namedMapPoint.Position;
            }
            else
            {
                client.SendChatMessage($"There is no named point with the name \"{target}\".");
            }
            return true;
        }
    }
}

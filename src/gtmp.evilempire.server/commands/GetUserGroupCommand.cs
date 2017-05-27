using gtmp.evilempire.entities;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.commands
{
    class GetUserGroupCommand : Command
    {
        IAuthenticationService authentication;

        public override CommandInfo Info { get; }

        public GetUserGroupCommand(ServiceContainer services)
        {
            authentication = services.Get<IAuthenticationService>();
            Info = new CommandInfo { Name = "get-usergroup", Description = "Returns the user group for a player. If no name is specified the user group of your login is returned.", Usage = "/get-usergroup [ <player> ]", IsAuthorized = p => p.User.UserGroup.IsAuthGroupOrHigher(AuthUserGroup.GameMaster), Execute = Execute };
        }

        public bool Execute(ISession session, ParsedCommand parsedCommand)
        {
            var target = parsedCommand.Args.At(0);
            var client = session.Client;
            if (!string.IsNullOrEmpty(target))
            {
                var targetUser = authentication.FindUserByLogin(target);
                if (targetUser == null)
                {
                    client.SendChatMessage($"No user for login \"{target}\" found.");
                }
                else
                {
                    client.SendChatMessage($"User group for login \"{target}\" is {targetUser.UserGroup}.");
                }
                return true;
            }

            var user = session.User;
            client.SendChatMessage($"Your user group is {user.UserGroup}.");
            return true;
        }
    }
}

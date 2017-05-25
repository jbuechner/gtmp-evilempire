using gtmp.evilempire.entities;
using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.commands
{
    class GetUserGroupCommand : Command
    {
        public override CommandInfo Info { get; }
        public ILoginService LoginService { get; }

        public GetUserGroupCommand(ServiceContainer services)
        {
            LoginService = services.Get<ILoginService>();

            Info = new CommandInfo { Name = "get-usergroup", Description = "Returns the user group for a player. If no name is specified the user group of your login is returned.", Usage = "/get-usergroup [ <player> ]", IsAuthorized = p => p.UserGroup.IsAuthGroupOrHigher(AuthUserGroup.GameMaster), Execute = Execute };
        }

        public bool Execute(IClient client, ParsedCommand parsedCommand)
        {
            var target = parsedCommand.Args.At(0);

            if (!string.IsNullOrEmpty(target))
            {
                var user = LoginService.FindUserByLogin(target);
                if (user == null)
                {
                    client.SendChatMessage($"No user for login \"{target}\" found.");
                }
                else
                {
                    client.SendChatMessage($"User group for login \"{target}\" is {user.UserGroup}.");
                }
                return true;
            }

            client.SendChatMessage($"Your user group is {client.UserGroup}.");
            return true;
        }
    }
}

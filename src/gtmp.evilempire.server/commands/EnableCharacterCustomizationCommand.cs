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
    class EnableCharacterCustomizationCommand : Command
    {
        ISessionService sessions;
        ISessionStateTransitionService sessionStateTransition;
        IDbService db;

        public override CommandInfo Info { get; }

        public EnableCharacterCustomizationCommand(ServiceContainer services)
        {
            sessions = services.Get<ISessionService>();
            sessionStateTransition = services.Get<ISessionStateTransitionService>();
            db = services.Get<IDbService>();

            Info = new CommandInfo { Name = "enable-charcust", Description = "Enabled the character customization for a single player. Use now=true to change the client into character customization state. If no player is specified character customization for yourself is enabled.", Usage = "/enable-charcust [ <player> ] [ <now> ]", IsAuthorized = p => p.User.UserGroup.IsAuthGroupOrHigher(AuthUserGroup.GameMaster), Execute = Execute };
        }

        public bool Execute(ISession session, ParsedCommand parsedCommand)
        {
            var playerOrNow = parsedCommand.Args.At(0);
            var playerOrNowAsBool = playerOrNow.AsBool();
            var now = parsedCommand.Args.At(1).AsBool();
            var targetSession = session;

            if (playerOrNowAsBool.HasValue)
            {
                targetSession = session;
                now = playerOrNowAsBool;
            }
            else
            {
                if (!string.IsNullOrEmpty(playerOrNow))
                {
                    targetSession = sessions.GetSessionByLogin(playerOrNow.AsString());
                    if (targetSession == null)
                    {
                        session.Client.SendChatMessage($"No active session for user {playerOrNow} has been found. The command can only act on logged in players.");
                        return false;
                    }
                }
            }

            var character = targetSession?.Character;
            if (character != null)
            {
                character.HasBeenThroughInitialCustomization = false;
                if (!now.GetValueOrDefault())
                {
                    db.Update(character);
                }
                else
                {
                    sessionStateTransition.Transit(targetSession, SessionState.CharacterCustomization);
                }
                return true;
            }

            return false;
        }
    }
}

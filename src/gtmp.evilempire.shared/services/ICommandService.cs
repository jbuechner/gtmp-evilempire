using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;

namespace gtmp.evilempire.services
{
    public interface ICommandService
    {
        void RegisterCommand(CommandInfo command);

        CommandExecutionResult ExecuteCommand(ISession session, string command);

        IList<CommandInfo> GetRegisteredCommands(ISession session);
    }
}

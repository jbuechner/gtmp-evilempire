using System;
using System.Collections.Generic;

namespace gtmp.evilempire.services
{
    public interface ICommandService
    {
        void RegisterCommand(CommandInfo command);

        IServiceResult ExecuteCommand(IClient client, string command);

        IList<CommandInfo> GetRegisteredCommands(IClient client);
    }
}

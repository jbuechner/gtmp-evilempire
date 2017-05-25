using gtmp.evilempire.services;
using System;
using System.Linq;

namespace gtmp.evilempire.server.commands
{
    class HelpCommand : Command
    {
        public override CommandInfo Info { get; }

        public IClientService ClientService { get; }
        public ICommandService CommandService { get; }

        public HelpCommand(ServiceContainer services)
        {
            ClientService = services.Get<IClientService>();
            CommandService = services.Get<ICommandService>();
            Info = new CommandInfo { Name = "help", Description = "Displays help and information for all available commands or a specific command.", IsAuthorized = client => true, Usage = "/help [<command>]", Execute = Execute };
        }

        bool Execute(IClient client, ParsedCommand parsedCommand)
        {
            var targetCommand = parsedCommand.Args.At(0);
            var commands = CommandService.GetRegisteredCommands(client);
            var allCount = commands.Count;
            commands = commands.Where(p => string.IsNullOrEmpty(targetCommand) || string.Compare(p.Name, targetCommand, StringComparison.OrdinalIgnoreCase) == 0).ToList();
            var filteredCount = commands.Count;
            client.SendChatMessage($"{filteredCount} out of {allCount} available commands.");
            foreach (var command in commands)
            {
                string description = $"/{command.Name}     {command.Usage}\n{command.Description}";
                client.SendChatMessage(description);
            }
            return true;
        }
    }
}

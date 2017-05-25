using gtmp.evilempire.services;
using System;
using System.Collections.Generic;

namespace gtmp.evilempire.server.services
{
    public class CommandService : ICommandService
    {
        readonly IDictionary<string, CommandInfo> _registered = new Dictionary<string, CommandInfo>();

        public void RegisterCommand(CommandInfo command)
        {
            if (command == null)
            {
                return;
            }

            _registered.Add(command.Name, command);
        }

        public IServiceResult ExecuteCommand(IClient client, string command)
        {
            var l = command.IndexOf(' ');
            if (l < 1)
            {
                l = command.Length;
            }
            var commandName = command.Substring(1, l - 1);
            CommandInfo info;
            if (_registered.TryGetValue(commandName, out info) && info != null)
            {
                var args = ParseCommand(command, l);
                var parsedCommand = new ParsedCommand { Command = info, Args = args };
                if (!info.Execute(client, parsedCommand))
                {
                    return ServiceResult.AsError("Command failed.");
                }
            }
            else
            {
                return ServiceResult.AsError("Unknown command.");
            }
            return ServiceResult.AsSuccess();
        }

        public IList<CommandInfo> GetRegisteredCommands(IClient client)
        {
            var result = new List<CommandInfo>(_registered.Count);
            foreach(var command in _registered.Values)
            {
                if (command.IsAuthorized(client))
                {
                    result.Add(command);
                }
            }
            return result;
        }

        string[] ParseCommand(string command, int startIndex)
        {
            const string GroupSequence = "\"";

            var parts = command.Substring(startIndex).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            bool inGroup = false;
            string group = null;
            List<string> args = new List<string>(parts.Length);
            foreach(var part in parts)
            {
                if (inGroup)
                {
                    group = string.Concat(group, ' ', part);
                    if (part.EndsWith(GroupSequence, StringComparison.Ordinal))
                    {
                        inGroup = false;
                        args.Add(group);
                        group = null;
                    }
                }
                else
                {
                    if (part.StartsWith(GroupSequence, StringComparison.Ordinal))
                    {
                        inGroup = true;
                        group = part.Substring(GroupSequence.Length);
                    }
                    else
                    {
                        args.Add(part);
                    }
                }
            }
            return args.ToArray();
        }
    }
}

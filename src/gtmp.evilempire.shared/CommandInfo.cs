using gtmp.evilempire.sessions;

namespace gtmp.evilempire
{
    public delegate bool CommandExecute(ISession session, ParsedCommand args);
    public delegate bool CommandIsAuthorized(ISession session);

    public class CommandInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public CommandExecute Execute { get; set; }
        public CommandIsAuthorized IsAuthorized { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;

namespace gtmp.evilempire.server.actions
{
    [ActionHandler("SendNotification")]
    class SendNotificationActionHandler : ActionHandler
    {
        string sender;
        string message;

        public SendNotificationActionHandler(ServiceContainer services, IDictionary<string, object> arguments)
            : base(services, arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            object intermediate;
            arguments.TryGetValue("Message", out intermediate);
            message = intermediate.AsString() ?? string.Empty;

            arguments.TryGetValue("Sender", out intermediate);
            sender = intermediate.AsString() ?? string.Empty;
        }

        public override void Handle(ISession session)
        {
            session.Client.SendNotification(sender, message);
        }
    }
}

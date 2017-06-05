using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.entities;
using gtmp.evilempire.services;

namespace gtmp.evilempire.server.actions
{
    [ActionHandler("SendMoneyChanged")]
    class SendMoneyChangedActionHandler : ActionHandler
    {
        ISessionService sessions;
        Currency[] currencies;

        public SendMoneyChangedActionHandler(ServiceContainer services, IDictionary<string, object> arguments)
            : base(services, arguments)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            object intermediate;
            if (arguments.TryGetValue("Currencies", out intermediate))
            {
                var raw = intermediate.AsString();
                if (raw != null)
                {
                    var list = raw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => (s ?? string.Empty).Trim());
                    var currencies = new List<Currency>();
                    foreach (var item in list)
                    {
                        Currency currency;
                        if (Enum.TryParse<Currency>(item, out currency))
                        {
                            currencies.Add(currency);
                        }
                        else
                        {
                            using (ConsoleColor.Yellow.Foreground())
                            {
                                Console.WriteLine($"[SendMoneyChangedActionHandler] Unable to parse currency from raw value \"{item}\".");
                            }
                        }
                    }
                    this.currencies = currencies.ToArray();
                }
                else
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine($"[SendMoneyChangedActionHandler] Unable to get Currencies for the action (AsString).");
                    }
                }
            }
            else
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"[SendMoneyChangedActionHandler] Unable to get Currencies for the action.");
                }
            }

            sessions = services.Get<ISessionService>();
        }

        public override void Handle(ISession session)
        {
            if (session != null)
            {
                sessions.SendMoneyChangedEvents(session, currencies);
            }
        }
    }
}

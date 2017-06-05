using System;
using System.Collections.Generic;
using gtmp.evilempire.sessions;
using gtmp.evilempire.entities;
using gtmp.evilempire.services;
using gtmp.evilempire.server.mapping;

namespace gtmp.evilempire.server.actions
{
    [ActionHandler("SpawnEntity")]
    class SpawnEntityActionHandler : ActionHandler
    {
        IPlatformService platform;
        Vehicle vehicle; //todo: only vehicles atm

        public SpawnEntityActionHandler(ServiceContainer services, IDictionary<string, object> arguments)
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

            platform = services.Get<IPlatformService>();
            var map = services.Get<Map>();

            object intermediate;
            if (arguments.TryGetValue("Type", out intermediate))
            {
                var entityType = (intermediate.AsString() ?? string.Empty).ToUpperInvariant();

                if (arguments.TryGetValue("Template", out intermediate))
                {
                    var templateName = (intermediate.AsString() ?? string.Empty);

                    vehicle = GetEntityTemplate(map, entityType, templateName);
                    if (vehicle == null)
                    {
                        using (ConsoleColor.Yellow.Foreground())
                        {
                            Console.WriteLine($"[SpawnEntityActionHandler] There is no entity for the template {templateName} of type {entityType}.");
                        }
                    }
                }
                else
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine($"[SpawnEntityActionHandler] Unable to get Template argument.");
                    }
                }
            }
            else
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"[SpawnEntityActionHandler] Unable to get Type argument.");
                }
            }
        }

        public override void Handle(ISession session)
        {
            if (vehicle != null && platform.IsClearRange(vehicle.Position, 5, 5))
            {
                platform.SpawnVehicle(vehicle);
            }
        }

        static Vehicle GetEntityTemplate(Map map, string entityType, string templateName)
        {
            switch(entityType)
            {
                case "VEHICLE":
                    return map.FindVehicleByTemplateName(templateName);
            }

            return null;
        }
    }
}

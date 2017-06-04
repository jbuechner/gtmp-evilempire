using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;

namespace gtmp.evilempire.server.mapping.actions
{
    class SpawnEntityServerAction : MapDialogueServerAction
    {
        Map map;
        IPlatformService platform;

        public override string Name
        {
            get
            {
                return "SpawnEntity";
            }
        }

        public SpawnEntityServerAction(ServiceContainer services)
            : base(services)
        {
            map = services.Get<Map>();
            platform = services.Get<IPlatformService>();
        }

        public override bool PerformAction(ISession session, IDictionary<string, string> args)
        {
            string entityType;
            string templateName;
            if (!args.TryGetValue("Type", out entityType) || !args.TryGetValue("Template", out templateName))
            {
                return false;
            }

            entityType = (entityType ?? string.Empty).ToUpperInvariant();

            switch(entityType)
            {
                case "VEHICLE":
                    var vehicle = map.FindVehicleByTemplateName(templateName);
                    if (vehicle != null && platform.IsClearRange(vehicle.Position, 5, 5))
                    {
                        platform.SpawnVehicle(vehicle);
                    }
                    break;
            }
            

            return false;
        }
    }
}

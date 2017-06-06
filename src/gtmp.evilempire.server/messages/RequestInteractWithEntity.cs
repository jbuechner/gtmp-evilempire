using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace gtmp.evilempire.server.messages
{
    class RequestInteractWithEntity : MessageHandlerBase
    {
        IPlatformService platform;
        ISerializationService serialization;
        Map map;

        public override string EventName
        {
            get
            {
                return ClientEvents.RequestInteractWithEntity;
            }
        }

        public RequestInteractWithEntity(ServiceContainer services)
            : base(services)
        {
            platform = services.Get<IPlatformService>();
            serialization = services.Get<ISerializationService>();
            map = services.Get<Map>();
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            var entityId = args.At(0).AsInt();
            var action = args.At(1).AsString();

            if (!entityId.HasValue)
            {
                return false;
            }

            var client = session.Client;
            var ped = map.GetPedByRuntimeHandle(entityId.Value);
            if (ped != null && string.Compare(action, "speak", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (ped.Dialogue != null)
                {
                    var response = new EntityContentResponse(serialization, entityId.Value, ped.Dialogue, null);
                    var responseData = serialization.SerializeAsDesignatedJson(response);
                    client.TriggerClientEvent(ClientEvents.RequestInteractWithEntityResponse, true, responseData);
                    return true;
                }
            }

            client.TriggerClientEvent(ClientEvents.RequestInteractWithEntityResponse, false, null);
            return false;
        }
    }
}

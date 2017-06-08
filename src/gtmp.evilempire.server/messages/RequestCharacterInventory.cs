using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.server.messages.transfer;
using gtmp.evilempire.services;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.messages
{
    class RequestCharacterInventory : MessageHandlerBase
    {
        ISerializationService serialization;
        IDbService db;

        public override string EventName
        {
            get
            {
                return ClientEvents.RequestCharacterInventory;
            }
        }

        public RequestCharacterInventory(ServiceContainer services)
            : base(services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            db = services.Get<IDbService>();
            serialization = services.Get<ISerializationService>();
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            var client = session?.Client;

            if (session.CharacterInventory != null)
            {
                var inventory = db.Select<CharacterInventory, int>(session.CharacterInventory.CharacterId);

                if (client != null && inventory != null)
                {
                    var response = new ClientCharacterInventory(inventory);
                    var data = serialization.SerializeAsDesignatedJson(response);
                    client.TriggerClientEvent(ClientEvents.RequestCharacterInventoryResponse, true, data);
                    return true;
                }
                client.TriggerClientEvent(ClientEvents.RequestCharacterInventoryResponse, false);
            }
            return false;
        }
    }
}

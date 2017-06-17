using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.messages
{
    class RequestCharacterInventoryDeleteItem : MessageHandlerBase
    {
        ISessionService sessions;
        ICharacterService characters;
        IItemService items;

        public override string EventName
        {
            get
            {
                return ClientEvents.RequestCharacterInventoryDeleteItem;
            }
        }

        public RequestCharacterInventoryDeleteItem(ServiceContainer services)
            : base(services)
        {
            sessions = services.Get<ISessionService>();
            characters = services.Get<ICharacterService>();
            items = services.Get<IItemService>();
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            var itemId = args.At(0).AsLong();
            var quantity = args.At(1).AsInt();
            var characterId = session?.Character?.Id;
            if (characterId != null && itemId.HasValue && quantity.HasValue && quantity.Value > 0)
            {
                var changes = characters.RemoveFromCharacterInventory(characterId.Value, new[] { new Item { Id = itemId.Value, Amount = quantity.Value } });
                sessions.SendCharacterInventoryChangedEvents(session, changes);
            }
            return false;
        }
    }
}

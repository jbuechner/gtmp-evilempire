using System;
using System.Collections.Generic;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;
using gtmp.evilempire.entities;
using gtmp.evilempire.server.mapping;

namespace gtmp.evilempire.server.actions
{
    [ActionHandler("GiveItem")]
    class GiveItemActionHandler : ActionHandler
    {
        ICharacterService characters;
        Item[] items;

        public GiveItemActionHandler(ServiceContainer services, IDictionary<string, object> arguments)
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

            characters = services.Get<ICharacterService>();
            var map = services.Get<Map>();

            object intermediate;
            arguments.TryGetValue("ItemDescriptionId", out intermediate);
            var itemDescriptionId = intermediate.AsIntFromHex();

            if (itemDescriptionId.HasValue)
            {
                arguments.TryGetValue("Amount", out intermediate);
                var amount = intermediate.AsInt();

                if (amount.HasValue)
                {
                    ItemDescription itemDescription;
                    if (map.ItemDescriptionMap.TryGetValue(itemDescriptionId.Value, out itemDescription) && itemDescription != null)
                    {
                        items = new Item[1] { new Item { Id = int.MinValue, ItemDescriptionId = itemDescription.Id, Amount = amount.Value } };
                    }
                }
                else
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine($"[GiveItemActionHandler] Unable to parse an amount from the raw argument \"{intermediate}\".");
                    }
                }
            }
            else
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"[GiveItemActionHandler] Unable to parse an item description id from the raw argument \"{intermediate}\".");
                }
            }
        }

        public override void Handle(ISession session)
        {
            if (session != null && session.CharacterInventory != null && items != null)
            {
                characters.AddToCharacterInventory(session.CharacterInventory.CharacterId, items);
            }
        }
    }
}

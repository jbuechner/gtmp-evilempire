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
        IPlatformService platform;
        Item[] items;
        string decorateWithKeyUsageFor;

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
            platform = services.Get<IPlatformService>();
            var map = services.Get<Map>();


            object intermediate;
            arguments.TryGetValue("ItemDescriptionId", out intermediate);
            var itemDescriptionId = intermediate.AsInt();

            if (itemDescriptionId.HasValue)
            {
                string name = null;
                if (arguments.TryGetValue("Name", out intermediate))
                {
                    name = intermediate.AsString();
                }

                arguments.TryGetValue("Amount", out intermediate);
                var amount = intermediate.AsInt();

                if (amount.HasValue)
                {
                    ItemDescription itemDescription;
                    if (map.ItemDescriptionMap.TryGetValue(itemDescriptionId.Value, out itemDescription) && itemDescription != null)
                    {
                        items = new Item[1] { new Item { ItemDescriptionId = itemDescription.Id, Amount = amount.Value, Name = name } };
                    }
                }
                else
                {
                    using (ConsoleColor.Yellow.Foreground())
                    {
                        Console.WriteLine($"[GiveItemActionHandler] Unable to parse an amount from the raw argument \"{intermediate}\".");
                    }
                }

                if (arguments.TryGetValue("UseAsKeyFor", out intermediate))
                {
                    decorateWithKeyUsageFor = intermediate.AsString();
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

        public override void Handle(ActionExecutionContext context)
        {
            var session = context.Session;
            if (session != null && session.CharacterInventory != null && items != null)
            {
                var length = items.Length;
                var newItems = new Item[length];
                for (var i = 0; i < length; i++)
                {
                    newItems[i] = new Item(items[i]);
                    if (!string.IsNullOrEmpty(decorateWithKeyUsageFor))
                    {
                        object contextValue;
                        if (context.KeyValues.TryGetValue(decorateWithKeyUsageFor, out contextValue) && contextValue != null)
                        {
                            var vehicle = contextValue as Vehicle;
                            if (vehicle == null)
                            {
                                using (ConsoleColor.Yellow.Foreground())
                                {
                                    Console.WriteLine($"[GiveItemActionHandler] Unable to use the item as key for the item of type \"{contextValue?.GetType()?.Name}\"..");
                                }
                                return;
                            }
                            if (vehicle.Id == Vehicle.ZeroId)
                            {
                                using (ConsoleColor.Yellow.Foreground())
                                {
                                    Console.WriteLine($"[GiveItemActionHandler] Unable to use the item as key for the vehicle because its id is LONG.MINVALUE.");
                                }
                                return;
                            }

                            newItems[i].Name = ReplaceTokens(newItems[i].Name, vehicle);
                            newItems[i].KeyForEntityId = vehicle.Id;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                characters.AddToCharacterInventory(session.CharacterInventory.CharacterId, newItems);
            }
        }

        string ReplaceTokens(string text, Vehicle vehicle)
        {
            var vehicleName = platform.GetVehicleModelName(vehicle);
            return text.Replace("{{ENTITY::MODELNAME}}", vehicleName);
        }
    }
}

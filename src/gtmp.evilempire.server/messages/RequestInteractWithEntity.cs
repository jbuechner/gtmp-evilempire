using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.messages
{
    class RequestInteractWithEntity : MessageHandlerBase
    {
        delegate bool InteractionHandler(ISession session, int? entityId, string entityKey, string action);

        readonly IDictionary<string, InteractionHandler> interactionHandlers;

        IPlatformService platform;
        ISerializationService serialization;
        ICharacterService characters;

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
            interactionHandlers = new Dictionary<string, InteractionHandler>
            {
                { "PED", InteractWithPed },
                { "VEHICLE", InteractWithVehicle }
            };

            platform = services.Get<IPlatformService>();
            serialization = services.Get<ISerializationService>();
            characters = services.Get<ICharacterService>();
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            var entityId = args.At(0).AsInt();
            var entityType = args.At(1).AsString();
            var entityKey = args.At(2).AsString();
            var action = args.At(3).AsString();

            if (!entityId.HasValue)
            {
                return false;
            }

            InteractionHandler handler;
            if (interactionHandlers.TryGetValue(entityType, out handler) && handler != null)
            {
                return handler(session, entityId, entityKey, action);
            }
            else
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"[RequestInteractWithEntity] There is not interaction handler for entity type \"{entityType}\".");
                }
            }

            session?.Client?.TriggerClientEvent(ClientEvents.RequestInteractWithEntityResponse, false, null);
            return false;
        }

        bool InteractWithPed(ISession session, int? entityId, string entityKey, string action)
        {
            var client = session.Client;
            var ped = (MapPed)platform.GetPedByRuntimeHandle(entityId.Value);
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
            return false;
        }

        bool InteractWithVehicle(ISession session, int? entityId, string entityKey, string action)
        {
            var vehicle = platform.GetVehicleByRuntimeHandle(entityId.Value);
            if (vehicle != null)
            {
                action = action.ToUpper();
                switch(action)
                {
                    case "LOCK":
                        if (HasRequiredKey(session, vehicle))
                        {
                            vehicle.IsLocked = !vehicle.IsLocked;
                            platform.UpdateSpawnedVehicle(vehicle);
                            session.Client.SendNotification(null, vehicle.IsLocked ? "The vehicle is now locked." : "The vehicle is now unlocked.");
                        }
                        else
                        {
                            session.Client.SendNotification(null, "You have no appropriate key to perform this action.");
                        }
                        return true;
                    case "ENGINE":
                        if (HasRequiredKey(session, vehicle))
                        {
                            vehicle.IsEngineRunning = !vehicle.IsEngineRunning;
                            platform.UpdateSpawnedVehicle(vehicle);
                            session.Client.SendNotification(null, vehicle.IsEngineRunning ? "The engine is now running." : "The engine is now off.");
                        }
                        else
                        {
                            session.Client.SendNotification(null, "You have no appropriate key to perform this action.");
                        }
                        return true;
                }
            }
            
            return false;
        }

        bool HasRequiredKey(ISession session, Vehicle vehicle)
        {
            return characters.GetKeys(session.Character.Id, vehicle).Any();
        }
    }
}

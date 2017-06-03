﻿using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

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
                    var dialogueData = serialization.DecorateAsJson( SerializeDialogue(ped.Dialogue));
                    var response = new RequestInteractWithEntityResponse { EntityId = entityId.Value, Dialogue = dialogueData };

                    var responseData = serialization.SerializeAsDesignatedJson(response);
                    client.TriggerClientEvent(ClientEvents.RequestInteractWithEntityResponse, true, responseData);
                    return true;
                }
            }


            client.TriggerClientEvent(ClientEvents.RequestInteractWithEntityResponse, false, null);
            return false;
        }

        static string SerializeDialogue(MapDialogue dialogue)
        {
            var builder = new StringBuilder();
            using (var textWriter = new StringWriter(builder))
            using (var writer = new JsonTextWriter(textWriter))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("__start");
                writer.WriteValue(dialogue.Name);

                SerializeDialoguePage(dialogue, writer, false);

                foreach(var page in dialogue.Pages)
                {
                    SerializeDialoguePage(page, writer, true);
                }
                writer.WriteEndObject();
                writer.Flush();
            }
            return builder.ToString();
        }

        static void SerializeDialoguePage(MapDialoguePage page, JsonTextWriter writer, bool writeNestedPages)
        {
            writer.WritePropertyName(page.Key);
            writer.WriteStartObject();
            if (!string.IsNullOrEmpty(page.Markdown))
            {
                writer.WritePropertyName("__markdown");
                writer.WriteValue(page.Markdown);
            }
            if (!string.IsNullOrEmpty(page.Action))
            {
                writer.WritePropertyName("__action");
                writer.WriteValue(page.Action);
            }

            if (writeNestedPages && page.Pages != null)
            {
                foreach(var nestedPage in page.Pages)
                {
                    SerializeDialoguePage(nestedPage, writer, writeNestedPages);
                }
            }

            writer.WriteEndObject();
        }
    }
}

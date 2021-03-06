﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.server.actions;

namespace gtmp.evilempire.server.messages
{
    class RequestTriggerEntityAction : MessageHandlerBase
    {
        ServiceContainer services;
        ISerializationService serialization;
        IPlatformService platform;

        public override string EventName
        {
            get
            {
                return ClientEvents.RequestTriggerEntityInteraction;
            }
        }

        public RequestTriggerEntityAction(ServiceContainer services)
            : base(services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            this.services = services;
            serialization = services.Get<ISerializationService>();
            platform = services.Get<IPlatformService>();
        }

        public override bool ProcessClientMessage(ISession session, params object[] args)
        {
            var entityId = args.At(0).AsInt();
            var pageKey = args.At(1).AsString();

            if (!entityId.HasValue)
            {
                return false;
            }

            var client = session.Client;
            var ped = (MapPed)platform.GetRuntimePedById(entityId.Value);
            if (ped != null && ped.Dialogue != null)
            {
                var dialoguePage = FindDialoguePage(ped.Dialogue, pageKey);
                var executionContext = new ActionExecutionContext(session);
                if (dialoguePage != null)
                {
                    if (dialoguePage.Actions == null || dialoguePage.Actions.Count < 1)
                    {

                    }
                    else
                    {
                        foreach (var set in dialoguePage.Actions)
                        {
                            var executionEngine = new ActionExecutionEngine(services, set);
                            executionEngine.Run(executionContext);
                        }
                    }

                    var response = new EntityContentResponse(serialization, entityId.Value, dialoguePage, null);
                    var responseData = serialization.SerializeAsDesignatedJson(response);
                    client.TriggerClientEvent(ClientEvents.RequestTriggerEntityInteractionResponse, true, responseData);
                    return true;
                }
            }

            var fallbackResponse = new EntityContentResponse(entityId.Value, null, null);
            var fallbackResponseData = serialization.SerializeAsDesignatedJson(fallbackResponse);
            client.TriggerClientEvent(ClientEvents.RequestTriggerEntityInteractionResponse, false, fallbackResponseData);
            return false;
        }

        static MapDialoguePage FindDialoguePage(MapDialogue dialogue, string pageKey)
        {
            Stack<MapDialoguePage> remainingPages = new Stack<MapDialoguePage>(new[] { dialogue });
            while (remainingPages.Count > 0)
            {
                var page = remainingPages.Pop();
                if (page != null)
                {
                    if (string.Compare(page.Key, pageKey, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return page;
                    }
                    if (page.Pages != null)
                    {
                        foreach (var subPage in page.Pages)
                        {
                            if (subPage == null)
                            {
                                continue;
                            }
                            remainingPages.Push(subPage);
                        }
                    }
                }
            }
            return null;
        }
    }
}

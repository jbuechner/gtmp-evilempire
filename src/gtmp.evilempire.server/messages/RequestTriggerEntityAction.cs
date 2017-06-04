using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.services;
using gtmp.evilempire.server.mapping;

namespace gtmp.evilempire.server.messages
{
    class RequestTriggerEntityAction : MessageHandlerBase
    {
        ISerializationService serialization;
        Map map;

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

            var response = new RequestTriggerEntityActionResponse { EntityId = entityId.Value };
            var responseData = serialization.SerializeAsDesignatedJson(response);

            var client = session.Client;
            var ped = map.GetPedByRuntimeHandle(entityId.Value);
            if (ped != null && ped.Dialogue != null)
            {
                var dialogueAction = FindDialogueAction(ped.Dialogue, action);
                if (dialogueAction != null)
                {
                    var overallSuccess = true;
                    foreach(var item in dialogueAction.Sequence)
                    {
                        var serverAction = map.FindDialogueServerActionByName(item.Type);
                        if (serverAction != null)
                        {
                            overallSuccess &= serverAction.PerformAction(session, item.Args);
                        }
                    }
                    client.TriggerClientEvent(ClientEvents.RequestTriggerEntityInteractionResponse, overallSuccess, responseData);
                }
            }

            client.TriggerClientEvent(ClientEvents.RequestTriggerEntityInteractionResponse, false, responseData);
            return false;
        }

        static MapDialogueAction FindDialogueAction(MapDialogue dialogue, string actionName)
        {
            Stack<MapDialoguePage> remainingPages = new Stack<MapDialoguePage>(new[] { dialogue });
            while (remainingPages.Count > 0)
            {
                var page = remainingPages.Pop();
                if (page != null)
                {
                    if (page.Action != null && string.Compare(page.Action.Name, actionName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return page.Action;
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

using gtmp.evilempire.sessions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace gtmp.evilempire.server.actions
{
    class ActionExecutionEngine
    {
        static readonly IDictionary<string, Type> actionHandlerMap = InitializeActionHandlerMap();
        static readonly ConcurrentDictionary<ActionSequenceItem, ActionHandler> actionToHandlerMap = new ConcurrentDictionary<ActionSequenceItem, ActionHandler>();

        ServiceContainer services;
        ActionSet set;

        public ActionExecutionEngine(ServiceContainer services, ActionSet set)
        {
            this.services = services;
            this.set = set;
        }

        public void Run(ISession session)
        {
            IList<ActionSetItem> actionItems = null;
            if (set.Condition == null)
            {
                actionItems = set.ThenActions;
            }
            else
            {
                var result = set.Condition.ProvideValue(session);
                if (result != null && (result.AsBool() ?? false))
                {
                    actionItems = set.ThenActions;
                }
                else
                {
                    actionItems = set.ElseActions;
                }
            }

            if (actionItems != null)
            {
                foreach (var action in actionItems)
                {
                    foreach (var item in action.Sequence.Items)
                    {
                        var actionHandler = ResolveAction(item, services, item.Args);
                        if (actionHandler == null)
                        {
                            using (ConsoleColor.Yellow.Foreground())
                            {
                                Console.WriteLine($"There is no action handler registered for the action type \"{item.ActionType}\".");
                            }
                            continue;
                        }
                        actionHandler.Handle(session);
                    }
                }
            }
        }

        static ActionHandler ResolveAction(ActionSequenceItem actionSequenceItem, ServiceContainer services, IDictionary<string, object> arguments)
        {
            Type actionHandlerType;
            if (!actionHandlerMap.TryGetValue(actionSequenceItem.ActionType, out actionHandlerType))
            {
                return null;
            }

            var actionHandler = actionToHandlerMap.GetOrAdd(actionSequenceItem, (item) => (ActionHandler)Activator.CreateInstance(actionHandlerType, services, arguments));
            return actionHandler;
        }

        static IDictionary<string, Type> InitializeActionHandlerMap()
        {
            var map = new Dictionary<string, Type>();
            var actionHandlerTypes = typeof(ActionHandler).Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(ActionHandler)));
            if (actionHandlerTypes != null)
            {
                foreach(var actionHandlerType in actionHandlerTypes)
                {
                    if (actionHandlerType == null)
                    {
                        continue;
                    }

                    var actionHandlerAttribute = actionHandlerType.GetCustomAttributes(typeof(ActionHandlerAttribute), false).Cast<ActionHandlerAttribute>().FirstOrDefault();
                    if (actionHandlerAttribute == null)
                    {
                        using (ConsoleColor.Yellow.Foreground())
                        {
                            Console.WriteLine($"[ActionExecutionEngine] The action handler type \"{actionHandlerType.FullName}\" does not have a ActionHandlerAttribute. It will be skipped.");
                        }
                        continue;
                    }

                    map[actionHandlerAttribute.Name] = actionHandlerType;
                }
            }
            return map;
        }
    }
}

using gtmp.evilempire.sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages
{
    abstract class MessageHandlerBase
    {
        protected ServiceContainer Services { get; }

        public abstract string EventName { get; }

        public MessageHandlerBase(ServiceContainer services)
        {
            Services = services;
        }

        public abstract bool ProcessClientMessage(ISession session, params object[] args);
    }
}

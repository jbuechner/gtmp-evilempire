using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;

namespace gtmp.evilempire.server.services
{
    delegate bool SessionStateTransit(ISession session);

    class SessionStateTransitionService : ISessionStateTransitionService
    {
        IPlatformService platform;
        ISerializationService serialization;

        readonly Dictionary<SessionState, SessionStateTransit> transitions;

        public SessionStateTransitionService(IPlatformService platform, ISerializationService serialization)
        {
            this.platform = platform;
            this.serialization = serialization;

            transitions = new Dictionary<SessionState, SessionStateTransit>
            {
                { SessionState.Connected, OnClientConnected },
                { SessionState.LoggedIn, OnClientLoggedIn },
                { SessionState.CharacterCustomization, OnClientCharacterCustomization },
                { SessionState.Freeroam, OnClientFreeroam }
            };
        }

        public bool Transit(ISession session, SessionState newState)
        {
            SessionStateTransit transition;
            if (transitions.TryGetValue(newState, out transition))
            {
                return transition?.Invoke(session) ?? false;
            }

            return false;
        }

        bool OnClientConnected(ISession session)
        {
            session?.Client?.TriggerClientEvent(ClientEvents.DisplayLoginScreen);
            session.State = SessionState.Connected;
            return true;
        }

        bool OnClientLoggedIn(ISession session)
        {
            if (!session.Character.HasBeenThroughInitialCustomization)
            {
                Transit(session, SessionState.CharacterCustomization);
            }
            else
            {
                session.State = SessionState.LoggedIn;
            }
            return true;
        }

        bool OnClientCharacterCustomization(ISession session)
        {
            var freeroamCustomizationData = platform.GetFreeroamCharacterCustomizationData();
            var data = serialization.SerializeAsDesignatedJson(freeroamCustomizationData);
            session?.Client?.TriggerClientEvent(ClientEvents.DisplayCharacterCustomization, data);
            session.State = SessionState.CharacterCustomization;
            return true;
        }

        bool OnClientFreeroam(ISession session)
        {
            session?.Client?.TriggerClientEvent(ClientEvents.EnterFreeroam);
            return true;
        }
    }
}

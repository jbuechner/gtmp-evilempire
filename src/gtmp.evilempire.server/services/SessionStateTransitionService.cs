using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.sessions;
using gtmp.evilempire.server.mapping;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.services
{
    delegate bool SessionStateTransit(ISession session);

    class SessionStateTransitionService : ISessionStateTransitionService
    {
        Map map;
        IPlatformService platform;
        ISerializationService serialization;

        readonly Dictionary<SessionState, SessionStateTransit> transitions;

        public SessionStateTransitionService(IPlatformService platform, ISerializationService serialization, Map map)
        {
            this.map = map;
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
            var loadingPoint = map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0)?.Position ?? Vector3f.One;
            var client = session.Client;
            client.IsNametagVisible = false;
            client.Dimension = session.PrivateDimension;
            client.CanMove = false;
            client.Position = loadingPoint;
            client.StopAnimation();

            client.TriggerClientEvent(ClientEvents.DisplayLoginScreen);
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
            var client = session.Client;
            if (client != null)
            {
                client.Dimension = 0;
                if (session.Character != null)
                {
                    UpdateClientPositionWithLastKnownPosition(session);
                }
                client.CanMove = true;
            }

            session?.Client?.TriggerClientEvent(ClientEvents.EnterFreeroam);
            return true;
        }

        void UpdateClientPositionWithLastKnownPosition(ISession session)
        {
            var client = session.Client;
            var character = session.Character;

            if (character.Position.HasValue)
            {
                var vector = character.Position.Value;
                vector.Z += 0.2f;
                client.Position = vector;
            }
            else
            {
                var startingPoint = map.GetPoint(MapPointType.NewPlayerSpawnPoint, 0);
                if (startingPoint != null)
                {
                    client.Position = startingPoint.Position;
                }
            }

            if (character.Rotation.HasValue)
            {
                client.Rotation = character.Rotation.Value;
            }
        }
    }
}

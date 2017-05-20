using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.services;
using System;
using System.Collections.Generic;

namespace gtmp.evilempire.server
{
    public delegate void ClientEventCallback(ServiceContainer services, Client client, dynamic args);
    public delegate IServiceResult ClientEventCallbackWithResponse(ServiceContainer services, Client client, dynamic args);

    public static class ClientEventCallbackExtensions
    {
        public static ClientEventCallback WrapIntoFailSafeResponse(this ClientEventCallbackWithResponse callback, string response)
        {
            ClientEventCallback cb = (services, client, args) =>
            {
                IServiceResult result = null;
                try
                {
                    result = callback(services, client, args);
                }
                catch (Exception ex)
                {
                    result = ServiceResult.AsError(ex);
                }
                var jsonSerializer = services.Get<IJsonSerializer>();
                var json = jsonSerializer.Stringify(result);
                client.triggerEvent(response, json);
            };
            return cb;
        }
    }
}

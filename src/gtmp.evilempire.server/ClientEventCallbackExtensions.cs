using GrandTheftMultiplayer.Server.Elements;
using gtmp.evilempire.services;
using System;
using System.Collections.Generic;

namespace gtmp.evilempire.server
{
    public delegate void ClientEventCallback(ServiceContainer services, Client client, params object[] args);
    public delegate IServiceResult ClientEventCallbackWithResponse(ServiceContainer services, Client client, params object[] args);

    public static class ClientEventCallbackExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static ClientEventCallback WrapIntoFailsafeResponse(this ClientEventCallbackWithResponse callback, string response)
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
                var json = string.Concat(Constants.DataSerialization.ClientServerJsonPrefix, jsonSerializer.Stringify(result.Data));
                client.triggerEvent(response, (int)result.State, json);

            };
            return cb;
        }
    }
}

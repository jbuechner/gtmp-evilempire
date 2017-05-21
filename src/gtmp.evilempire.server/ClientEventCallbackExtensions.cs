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
        class ServiceTransferResult
        {
            public ServiceResultState State { get; set; }
            public object Data { get; set; }

            public ServiceTransferResult(IServiceResult serviceResult)
            {
                this.State = serviceResult.State;
                this.Data = serviceResult.Data;
            }
        }

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
                var json = jsonSerializer.Stringify(new ServiceTransferResult(result));
                client.triggerEvent(response, json);
            };
            return cb;
        }
    }
}

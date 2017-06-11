using gtmp.evilempire.ipc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Net;
using System.Text;

namespace gtmp.evilempire.server.httprpc.routes
{
    class ApiStatusRoute : HttpListenerRoute
    {
        readonly IpcClient ipcClient;

        public ApiStatusRoute()
        {
            ipcClient = new IpcClient();
        }

        public override bool Matches(HttpListenerContext context)
        {
            return context?.Request?.Url.LocalPath.IndexOf("/api/status") >= 0;
        }

        public override void Handle(HttpListenerContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentEncoding = Encoding.UTF8;

            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var serializer = JsonSerializer.Create(settings);
            var response = ipcClient.ReadStatus();
            using (var writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
            {
                serializer.Serialize(writer, response);
                writer.Flush();
            }
        }
    }
}

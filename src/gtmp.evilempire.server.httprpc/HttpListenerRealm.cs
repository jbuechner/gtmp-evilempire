using gtmp.evilempire.server;
using gtmp.evilempire.server.httprpc.routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.httprpc
{
    class HttpListenerRealm : IDisposable
    {
        class Private : IDisposable
        {
            HttpListener listener;

            public Private(string realm)
            {
                listener = new HttpListener
                {
                    AuthenticationSchemes = AuthenticationSchemes.Anonymous | AuthenticationSchemes.Basic
                };
                listener.Prefixes.Clear();
                listener.Prefixes.Add(string.Concat("http://", realm, "/"));
                listener.Start();
            }

            public Task<HttpListenerContext> GetContext()
            {
                return listener.GetContextAsync();
            }

            public void Dispose()
            {
                listener?.Stop();
                listener?.Close();
                listener = null;
            }
        }

        Thread thread;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        readonly List<HttpListenerRoute> routes = new List<HttpListenerRoute> { new ApiStatusRoute() };

        public string Address { get; }
        public int Port { get; }

        public HttpListenerRealm(string address, int port)
        {
            Address = address;
            Port = port;

            thread = new Thread(Listen);
            thread.Start(this);
        }

        public void Dispose()
        {
            var cencallationTokenSource = cancellationTokenSource;
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                thread?.Join();
            }
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            thread = null;
        }

        public IEnumerable<HttpListenerRoute> GetMatchingRoutes(HttpListenerContext context)
        {
            foreach (var route in routes)
            {
                if (route?.Matches(context) ?? false)
                {
                    yield return route;
                }
            }
        }

        static void Listen(object untypedOwner)
        {
            var owner = (HttpListenerRealm)untypedOwner;
            var realm = $"{owner.Address}:{owner.Port}";
            var listener = new Private(realm);

            using (ConsoleColor.Cyan.Foreground())
            {
                Console.Write("[HTTP] ");
            }
            Console.WriteLine($"listening on {realm}       API RPC route is {realm}/api/");
            while (!owner.cancellationTokenSource.IsCancellationRequested)
            {
                var context = listener.GetContext();
                if (context != null)
                {
                    while (!(context.IsCompleted || context.IsCanceled || context.IsFaulted))
                    {
                        HandleContext(owner, context.Result);
                    }
                }

                Thread.Sleep(10);
            }
        }

        static void HandleContext(HttpListenerRealm listener, HttpListenerContext context)
        {
            if (context == null)
            {
                return;
            }

            try
            {
                var hasMatchedAtLeastOnce = false;
                foreach (var matchingRoute in listener.GetMatchingRoutes(context))
                {
                    if (matchingRoute == null)
                    {
                        continue;
                    }
                    hasMatchedAtLeastOnce = true;
                    matchingRoute.Handle(context);
                }

                if (!hasMatchedAtLeastOnce)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                context.Response.Close();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Close();
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine($"[HTTP] Error while handling request {context.Request.Url} from {context.Request.RemoteEndPoint}\n\n{ex}");
                }
            }

        }
    }
}

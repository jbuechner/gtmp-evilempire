using gtmp.evilempire.db;
using gtmp.evilempire.server.services;
using gtmp.evilempire.services;
using System;
using TinyIoC;

namespace gtmp.evilempire.server
{
    public sealed class ServiceContainer : IDisposable
    {
        TinyIoCContainer container = new TinyIoCContainer();

        public T Get<T>()
            where T : class
        {
            return this.container.Resolve<T>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Register<TType, TInstance>()
            where TType : class
            where TInstance : class, TType
        {
            this.container.Register<TType, TInstance>();
        }

        public void Register<T>(T instance)
            where T : class
        {
            this.container.Register(instance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static ServiceContainer Create()
        {
            var services = new ServiceContainer();
            services.Register<IJsonSerializer>(new JsonSerializer());
            services.Register<IDbService>(new DbService(evilempire.Constants.Database.DatabasePath));
            services.Register<IAuthorizationService, AuthorizationService>();
            return services;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "container")]
        public void Dispose()
        {
            this.container?.Dispose();
            this.container = null;
        }
    }
}

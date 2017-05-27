using gtmp.evilempire.db;
using gtmp.evilempire.server.mapping;
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
            return container.Resolve<T>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Register<TType, TInstance>()
            where TType : class
            where TInstance : class, TType
        {
            container.Register<TType, TInstance>();
        }

        public void Register<T>(T instance)
            where T : class
        {
            container.Register(instance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static ServiceContainer Create()
        {
            var services = new ServiceContainer();
            services.Register<ISerializationService, SerializationService>();
            services.Register<IDbService>(new DbService(evilempire.Constants.Database.DatabasePath));
            services.Register<ISessionStateTransitionService, SessionStateTransitionService>();
            services.Register<ISessionService, SessionService>();
            services.Register<IClientService, ClientService>();
            services.Register<IAuthenticationService, AuthenticationService>();
            services.Register<ICharacterService, CharacterService>();

            //services.Register<ICommandService, CommandService>();
            //services.Register<IClientLifecycleService, ClientLifecycleService>();
            //services.Register<IAuthorizationService, AuthorizationService>();
            //services.Register<ILoginService, LoginService>();
            //services.Register<MapObjectService, MapObjectService>();
            return services;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "container")]
        public void Dispose()
        {
            container?.Dispose();
            container = null;
        }
    }
}


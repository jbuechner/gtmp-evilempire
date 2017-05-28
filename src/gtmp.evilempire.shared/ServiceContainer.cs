using System;
using TinyIoC;

namespace gtmp.evilempire
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "container")]
        public void Dispose()
        {
            container?.Dispose();
            container = null;
        }
    }
}


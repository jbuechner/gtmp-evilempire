using gtmp.evilempire.services;
using System;

namespace gtmp.evilempire.db
{
    public sealed class DbService : IDbService, IDisposable
    {
        DbEnvironment _dbe;

        public DbService(string databaseRootPath)
        {
            _dbe = new DbEnvironment(databaseRootPath);
        }

        public T SelectEntity<T, TKey>(TKey key)
        {
            return _dbe.Select<T, TKey>(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_dbe")]
        public void Dispose()
        {
            _dbe?.Dispose();
            _dbe = null;
        }
    }
}

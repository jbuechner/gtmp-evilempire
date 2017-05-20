using gtmp.evilempire.services;
using System;

namespace gtmp.evilempire.db
{
    public class DbService : IDbService, IDisposable
    {
        DbEnvironment _dbe;

        public DbService(string databaseRootPath)
        {
            this._dbe = new DbEnvironment(databaseRootPath);
        }

        public T Select<T, TKey>(TKey key)
        {
            return this._dbe.Select<T, TKey>(key);
        }

        public void Dispose()
        {
            this._dbe?.Dispose();
            this._dbe = null;
        }
    }
}

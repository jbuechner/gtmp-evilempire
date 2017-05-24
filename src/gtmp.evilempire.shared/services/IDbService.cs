using System;
using System.Linq.Expressions;

namespace gtmp.evilempire.services
{
    public interface IDbService : IDisposable
    {
        void AddKnownEntity<T, TKey>(string name, Func<T, TKey> uniqueKeySelector, Expression<Func<T, TKey>> uniqueKeyFieldName);

        T Insert<T>(T element);
        T Select<T, TKey>(TKey key);
        T Update<T>(T element);

        object InsertOrUpdate(object element);

        int NextValueFor(string sequence);
        int? ValueFor(string sequence);
    }
}

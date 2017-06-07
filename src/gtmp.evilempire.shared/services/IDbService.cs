using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace gtmp.evilempire.services
{
    public interface IDbService : IDisposable
    {
        T Insert<T>(T element);

        T Select<T, TKey>(Expression<Func<T, TKey>> keySelector, TKey key);
        T Select<T, TKey>(TKey key);
        IEnumerable<T> SelectMany<T, TKey>(TKey key);
        T Update<T>(T element);

        object InsertOrUpdate(object element);

        int NextValueFor(string sequence, int seed = 0);
        long NextInt64ValueFor(string sequence, long seed = 0);
        int? ValueFor(string sequence);
        long? Int64ValueFor(string sequence);
    }
}

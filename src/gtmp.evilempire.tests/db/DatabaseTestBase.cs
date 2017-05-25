using gtmp.evilempire.db;
using gtmp.evilempire.services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace gtmp.evilempire.tests.db
{
    public class DatabaseTestBase
    {
        class MemoryDbService : IDbService
        {
            IDbService _decorated;
            MemoryStream _stream = new MemoryStream();

            public MemoryDbService()
            {
                _decorated = new DbService(_stream);
            }

            public void Dispose()
            {
                _stream?.Dispose();
                _decorated?.Dispose();
                _stream = null;
                _decorated = null;
            }

            public T Insert<T>(T element)
            {
                return _decorated.Insert<T>(element);
            }

            public object InsertOrUpdate(object element)
            {
                return _decorated.InsertOrUpdate(element);
            }

            public int NextValueFor(string sequence)
            {
                return _decorated.NextValueFor(sequence);
            }

            public T Select<T, TKey>(TKey key)
            {
                return _decorated.Select<T, TKey>(key);
            }

            public T Select<T, TKey>(Expression<Func<T, TKey>> keySelector, TKey key)
            {
                return _decorated.Select<T, TKey>(keySelector, key);
            }

            public IEnumerable<T> SelectMany<T, TKey>(TKey key)
            {
                return _decorated.SelectMany<T, TKey>(key);
            }

            public T Update<T>(T element)
            {
                return _decorated.Update<T>(element);
            }

            public int? ValueFor(string sequence)
            {
                return _decorated.ValueFor(sequence);
            }
        }

        public TestContext TestContext { get; set; }
        public Func<IDbService> DbServiceFactory { get; set; }

        [TestInitialize]
        public void Setup()
        {
            DbServiceFactory = () =>
            {
                var stream = new MemoryStream();
                return new DbService(stream);
            };
        }
    }
}

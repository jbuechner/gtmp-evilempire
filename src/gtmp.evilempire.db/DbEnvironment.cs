using DBreeze;
using DBreeze.DataTypes;
using gtmp.evilempire.entities;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace gtmp.evilempire.db
{
    public sealed class DbEnvironment : IDisposable
    {
        DBreezeConfiguration _configuration;
        DBreezeEngine _engine;
        ConcurrentDictionary<Type, EntityStorageAttribute> _entityStorageDescriptionCache = new ConcurrentDictionary<Type, EntityStorageAttribute>();
        ConcurrentDictionary<Type, Func<object, object>> _entityKeySelectorCache = new ConcurrentDictionary<Type, Func<object, object>>();

        public DbEnvironment(string databaseRootPath)
        {
            _configuration = new DBreezeConfiguration();
            try
            {
                _configuration.Storage = DBreezeConfiguration.eStorage.DISK;
                _configuration.DBreezeDataFolderName = databaseRootPath;
            }
            catch
            {
                _configuration?.Dispose();
                throw;
            }
            _engine = new DBreezeEngine(_configuration);
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject;
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_engine")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_configuration")]
        public void Dispose()
        {
            _engine?.Dispose();
            _configuration?.Dispose();
            _engine = null;
            _configuration = null;
        }

        public T Select<T, TKey>(TKey key)
        {
            var reflectedTableName = GetReflectedTableName<T>();
            return Select<T, TKey>(reflectedTableName, key);
        }

        public void Insert<T, TKey>(string tableName, TKey key, T value)
        {
            using (var t = _engine.GetTransaction())
            {
                byte[] r;
                bool wasUpdated;
                t.Insert<TKey, DbMJSON<T>>(tableName, key, value, out r, out wasUpdated, true);
                t.Commit();
            }
        }

        public T Select<T, TKey>(string tableName, TKey key)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            using (var t = _engine.GetTransaction())
            {
                var row = t.Select<TKey, DbMJSON<T>>(tableName, key);
                if (row != null && row.Exists)
                {
                    var o = row.Value;
                    if (o != null)
                    {
                        return o.Get;
                    }
                }
            }
            return default(T);
        }

        internal void InsertOrUpdate(object entity)
        {
            var key = SelectKey(entity);
            InsertOrUpdate(key, entity);
        }

        internal void InsertOrUpdate(object key, object entity) // todo: performance, heavy reflection
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var keyType = key.GetType();
            var entityType = entity.GetType();
            var reflectedTableName = GetEntityStorageDescription(entityType)?.Storage;

            using (var t = _engine.GetTransaction())
            {
                var dbmJsonType = typeof(DBreezeEngine).Assembly.GetTypes().First(p => p.Name == "DbMJSON`1").MakeGenericType(entityType);
                var dbmJson = Activator.CreateInstance(dbmJsonType, entity);
                var insert = t.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(p => p.Name == "Insert" && p.GetParameters().Count() == 3).MakeGenericMethod(keyType, dbmJsonType);
                insert.Invoke(t, new[] { reflectedTableName, key, dbmJson });
                t.Commit();
            }
        }

        public object SelectKey(object entity)
        {
            if (entity == null)
            {
                return null;
            }

            var keySelector = GetEntityKeySelector(entity.GetType());
            if (keySelector != null)
            {
                return keySelector(entity);
            }
            return null;
        }

        string GetReflectedTableName<T>()
        {
            var description = GetEntityStorageDescription<T>();
            if (description == null)
            {
                return null;
            }
            return description.Storage;
        }

        EntityStorageAttribute GetEntityStorageDescription<T>()
        {
            return GetEntityStorageDescription(typeof(T));
        }

        EntityStorageAttribute GetEntityStorageDescription(Type t)
        {
            EntityStorageAttribute v;
            if (_entityStorageDescriptionCache.TryGetValue(t, out v))
            {
                return v;
            }
            else
            {
                v = t.GetCustomAttributes(typeof(EntityStorageAttribute), false).Select(s => s as EntityStorageAttribute).FirstOrDefault();
                _entityStorageDescriptionCache.TryAdd(t, v);
                return v;
            }
        }

        Func<object, object> GetEntityKeySelector(Type t)
        {
            Func<object, object> keySelector;
            if (_entityKeySelectorCache.TryGetValue(t, out keySelector))
            {
                return keySelector;
            }
            else
            {
                var entityStorageDescription = GetEntityStorageDescription(t);
                var keyMember = entityStorageDescription.KeyMember;
                var keyProperty = t.GetProperty(keyMember);

                var p = Expression.Parameter(typeof(object));
                var castedParameter = Expression.TypeAs(p, t);
                var call = Expression.Call(castedParameter, keyProperty.GetMethod);
                var cast = Expression.TypeAs(call, typeof(object));
                var lambda = Expression.Lambda(cast, p);
                keySelector = (Func<object, object>)lambda.Compile();

                _entityKeySelectorCache.TryAdd(t, keySelector);
                return keySelector;
            }
        }
    }
}

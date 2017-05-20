using DBreeze;
using DBreeze.DataTypes;
using gtmp.evilempire.entities;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace gtmp.evilempire.db
{
    public class DbEnvironment : IDisposable
    {
        DBreezeEngine _engine;
        ConcurrentDictionary<Type, EntityStorageAttribute> _entityStorageDescriptionCache = new ConcurrentDictionary<Type, EntityStorageAttribute>();
        ConcurrentDictionary<Type, Func<object, object>> _entityKeySelectorCache = new ConcurrentDictionary<Type, Func<object, object>>();

        public DbEnvironment(string databaseRootPath)
        {
            var configuration = new DBreezeConfiguration { Storage = DBreezeConfiguration.eStorage.DISK, DBreezeDataFolderName = databaseRootPath };
            this._engine = new DBreezeEngine(configuration);
        }

        public void Dispose()
        {
            this._engine?.Dispose();
            this._engine = null;
        }

        public T Select<T, TKey>(TKey key)
        {
            var reflectedTableName = this.GetReflectedTableName<T>();
            return Select<T, TKey>(reflectedTableName, key);
        }

        public void Insert<T, TKey>(string tableName, TKey key, T value)
        {
            using (var t = this._engine.GetTransaction())
            {
                t.Insert<TKey, DbMJSON<T>>(tableName, key, value);
                t.Commit();
            }
        }

        public T Select<T, TKey>(string tableName, TKey key)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            using (var t = this._engine.GetTransaction())
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

        public void InsertOrUpdate(object key, object entity) // todo: performance, heavy reflection
        {
            var keyType = key.GetType();
            var entityType = entity.GetType();
            var reflectedTableName = this.GetEntityStorageDescription(entityType)?.Storage;

            using (var t = this._engine.GetTransaction())
            {
                var select = t.GetType().GetMethod("Select").MakeGenericMethod(keyType, entityType);
                var result = select.Invoke(t, new[] { reflectedTableName, key, false });
                if (result != null)
                {
                    var existsProperty = result.GetType().GetProperty("Exists");
                    var exists = existsProperty?.GetMethod?.Invoke(result, null);
                    if (exists != null && !(bool)exists)
                    {
                        var l = typeof(DBreezeEngine).Assembly.GetTypes().Select(s => s.Name).ToList();
                        var dbmJsonType = typeof(DBreezeEngine).Assembly.GetTypes().First(p => p.Name == "DbMJSON`1").MakeGenericType(entityType);
                        var dbmJson = Activator.CreateInstance(dbmJsonType, entity);
                        var insert = t.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(p => p.Name == "Insert" && p.GetParameters().Count() == 3).MakeGenericMethod(keyType, dbmJsonType);
                        insert.Invoke(t, new[] { reflectedTableName, key, dbmJson });
                        t.Commit();
                    }
                }
            }
        }

        public object SelectKey(object entity)
        {
            if (entity == null)
            {
                return null;
            }

            var keySelector = this.GetEntityKeySelector(entity.GetType());
            if (keySelector != null)
            {
                return keySelector(entity);
            }
            return null;
        }

        string GetReflectedTableName<T>()
        {
            var description = this.GetEntityStorageDescription<T>();
            if (description == null)
            {
                return null;
            }
            return description.Storage;
        }

        EntityStorageAttribute GetEntityStorageDescription<T>()
        {
            return this.GetEntityStorageDescription(typeof(T));
        }

        EntityStorageAttribute GetEntityStorageDescription(Type t)
        {
            EntityStorageAttribute v;
            if (this._entityStorageDescriptionCache.TryGetValue(t, out v))
            {
                return v;
            }
            else
            {
                v = t.GetCustomAttributes(typeof(EntityStorageAttribute), false).Select(s => s as EntityStorageAttribute).FirstOrDefault();
                this._entityStorageDescriptionCache.TryAdd(t, v);
                return v;
            }
        }

        Func<object, object> GetEntityKeySelector(Type t)
        {
            Func<object, object> keySelector;
            if (this._entityKeySelectorCache.TryGetValue(t, out keySelector))
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

                this._entityKeySelectorCache.TryAdd(t, keySelector);
                return keySelector;
            }
        }
    }
}

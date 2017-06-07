using LiteDB;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.IO;

namespace gtmp.evilempire.db
{
    public sealed class DbEnvironment : IDisposable
    {
        class KnownEntity
        {
            public Type EntityType { get; set; }
            public string Name { get; set; }

            public EntityKey PrimaryKey { get; set; }

            public EntityKey GetKey(string memberName)
            {
                if (PrimaryKey.Name == memberName)
                {
                    return PrimaryKey;
                }
                if (memberName == "Id")
                {
                    return new EntityKey { Name = "Id", IsUnique = true };
                }
                throw new NotImplementedException();
            }
        }

        class EntityKey
        {
            public string Name { get; set; }
            public Func<object, object> ValueSelector { get; set; }
            public bool IsUnique { get; set; }
        }

        readonly object _syncRoot = new object();

        LiteDatabase _db;

        Dictionary<string, KnownEntity> _known = new Dictionary<string, KnownEntity>();
        Dictionary<Type, KnownEntity> _knownByType = new Dictionary<Type, KnownEntity>();

        internal DbEnvironment(Stream stream)
        {
            _db = new LiteDatabase(stream);
        }

        internal DbEnvironment(string databaseRootPath)
        {
            if (databaseRootPath.Contains(Path.DirectorySeparatorChar) || databaseRootPath.Contains(Path.AltDirectorySeparatorChar))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(databaseRootPath));
            }
            _db = new LiteDatabase(databaseRootPath);
        }

        public void AddKnownEntity<T, TKey>(string name, Func<T, TKey> primaryKeySelector, Expression<Func<T, TKey>> primaryKeyNameExpression, bool isPrimaryKeyUnique)
        {
            lock (_syncRoot)
            {
                if (_known.ContainsKey(name))
                {
                    throw new ArgumentOutOfRangeException(nameof(name), "Only one entity for each unique name is allowed.");
                }
                var type = typeof(T);
                Func<object, object> kvs = element => primaryKeySelector((T)element);
                var primaryKey = new EntityKey { Name = primaryKeyNameExpression.MemberName(), ValueSelector = kvs, IsUnique = isPrimaryKeyUnique };
                var entity = new KnownEntity { EntityType = type, Name = name, PrimaryKey = primaryKey };
                _known.Add(name, entity);
                _knownByType.Add(type, entity);

                var collection = _db.GetCollection(entity.Name);
                collection.EnsureIndex(primaryKey.Name, primaryKey.IsUnique);
            }
        }

        public T Insert<T>(T element)
        {
            var type = typeof(T);
            var collection = GetCollection(type);
            var document = _db.Mapper.ToDocument(type, element);
            collection.Insert(document);
            return element;
        }

        public T Select<T, TKey>(TKey key)
        {
            var knownEntity = GetKnownEntity<T>();
            CheckKnownEntity(knownEntity);

            return Select<T, TKey>(knownEntity.PrimaryKey, key);
        }

        public T Select<T, TKey>(Expression<Func<T, TKey>> keySelector, TKey key)
        {
            var knownEntity = GetKnownEntity<T>();
            CheckKnownEntity(knownEntity);

            var entityKey = knownEntity.GetKey(keySelector.MemberName());
            return Select<T, TKey>(entityKey, key);
        }

        T Select<T, TKey>(EntityKey entityKey, TKey keyValue)
        {
            var knownEntity = GetKnownEntity<T>();
            CheckKnownEntity(knownEntity);

            var collection = _db.GetCollection<T>(knownEntity.Name);
            if (entityKey.Name == "Id")
            {
                var element = collection.FindById(new BsonValue(keyValue));
                return element;
            }
            else
            {
                var element = collection.FindOne(Query.EQ(entityKey.Name, new BsonValue(keyValue)));
                return element;
            }
        }

        public IEnumerable<T> SelectMany<T, TKey>(TKey key)
        {
            var knownEntity = GetKnownEntity<T>();
            CheckKnownEntity(knownEntity);

            var collection = _db.GetCollection<T>(knownEntity.Name);

            var elements = collection.Find(Query.EQ(knownEntity.PrimaryKey.Name, new BsonValue(key)));
            return elements;
        }

        public T Update<T>(T element)
        {
            var knownEntity = GetKnownEntity<T>();
            CheckKnownEntity(knownEntity);

            var collection = _db.GetCollection(knownEntity.Name);
            var key = knownEntity.PrimaryKey.ValueSelector(element);
            var target = collection.FindOne(Query.EQ(knownEntity.PrimaryKey.Name, new BsonValue(key)));
            if (target == null)
            {
                throw new InvalidOperationException("Element is not part of collection");
            }
            var documentId = target["_id"];
            var document = _db.Mapper.ToDocument<T>(element);
            if (collection.Update(documentId, document))
            {
                return element;
            }
            throw new InvalidOperationException("Element is not part of collection");
        }

        public int? ValueFor(string sequence)
        {
            var collection = _db.GetCollection(string.Concat("__sequence_", sequence));
            var bson = collection.FindOne(p => true);
            if (bson == null)
            {
                return null;
            }
            return bson["value"].AsInt32;
        }

        public long? Int64ValueFor(string sequence)
        {
            var collection = _db.GetCollection(string.Concat("__sequence64_", sequence));
            var bson = collection.FindOne(p => true);
            if (bson == null)
            {
                return null;
            }
            return bson["value"].AsInt64;
        }

        public int NextValueFor(string sequence, int seed = 0)
        {
            var collection = _db.GetCollection(string.Concat("__sequence_", sequence));
            using (var t = _db.BeginTrans())
            {
                var bson = collection.FindOne(p => true);
                if (bson == null)
                {
                    bson = new BsonDocument(new Dictionary<string, BsonValue> { { "value", new BsonValue(seed) } });
                    collection.Insert(bson);
                    t.Commit();
                    return seed;
                }
                else
                {
                    var next = bson["value"].AsInt32 + 1;
                    bson["value"] = new BsonValue(next);
                    collection.Update(bson);
                    t.Commit();
                    return next;
                }
            }
        }

        public long NextInt64ValueFor(string sequence, long seed = 0)
        {
            var collection = _db.GetCollection(string.Concat("__sequence64_", sequence));
            using (var t = _db.BeginTrans())
            {
                var bson = collection.FindOne(p => true);
                if (bson == null)
                {
                    bson = new BsonDocument(new Dictionary<string, BsonValue> { { "value", new BsonValue(seed) } });
                    collection.Insert(bson);
                    t.Commit();
                    return seed;
                }
                else
                {
                    long next = bson["value"].AsInt64 + 1;
                    bson["value"] = new BsonValue(next);
                    collection.Update(bson);
                    t.Commit();
                    return next;
                }
            }
        }

        public object InsertOrUpdate(object element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var type = element.GetType();
            var knownEntity = GetKnownEntity(type);
            CheckKnownEntity(knownEntity);

            var collection = _db.GetCollection(knownEntity.Name);
            var document = _db.Mapper.ToDocument(knownEntity.EntityType, element);
            var key = knownEntity.PrimaryKey.ValueSelector(element);
            var target = collection.FindOne(Query.EQ(knownEntity.PrimaryKey.Name, new BsonValue(key)));
            if (target == null)
            {
                collection.Insert(document);
            }
            else
            {
                var documentId = target["_id"];
                collection.Update(documentId, document);
            }
            return element;
        }

        KnownEntity GetKnownEntity<T>()
        {
            return GetKnownEntity(typeof(T));
        }

        KnownEntity GetKnownEntity(Type type)
        {
            KnownEntity knownEntity;
            if (_knownByType.TryGetValue(type, out knownEntity))
            {
                return knownEntity;
            }
            return null;
        }

        LiteCollection<T> GetCollection<T>()
        {
            var knownEntity = GetKnownEntity<T>();
            CheckKnownEntity(knownEntity);

            return _db.GetCollection<T>(knownEntity.Name);
        }

        LiteCollection<BsonDocument> GetCollection(Type type)
        {
            var knownEntity = GetKnownEntity(type);
            CheckKnownEntity(knownEntity);

            return _db.GetCollection(knownEntity.Name);
        }

        void CheckKnownEntity(KnownEntity knownEntity)
        {
            if (knownEntity == null)
            {
                throw new InvalidOperationException("T is not a known entity. T = " + knownEntity.EntityType.Name);
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
            _db = null;
        }
    }
}

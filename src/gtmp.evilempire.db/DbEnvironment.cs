﻿using LiteDB;
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
                Func<object, object> kvs = element => primaryKeySelector((T)element);
                var primaryKey = new EntityKey { Name = primaryKeyNameExpression.MemberName(), ValueSelector = kvs, IsUnique = isPrimaryKeyUnique };
                var entity = new KnownEntity { EntityType = typeof(T), Name = name, PrimaryKey = primaryKey };
                _known.Add(name, entity);

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

            var collection = _db.GetCollection<T>(knownEntity.Name);
            var element = collection.FindOne(Query.EQ(knownEntity.PrimaryKey.Name, new BsonValue(key)));
            return element;
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

        public int NextValueFor(string sequence)
        {
            var collection = _db.GetCollection(string.Concat("__sequence_", sequence));
            using (var t = _db.BeginTrans())
            {
                var bson = collection.FindOne(p => true);
                if (bson == null)
                {
                    bson = new BsonDocument(new Dictionary<string, BsonValue> { { "value", new BsonValue(0) } });
                    collection.Insert(bson);
                    t.Commit();
                    return 0;
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
            return _known.FirstOrDefault(p => p.Value.EntityType == type).Value;
        }

        KnownEntity GetKnownEntity(string name)
        {
            KnownEntity v;
            if (_known.TryGetValue(name, out v))
            {
                return v;
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

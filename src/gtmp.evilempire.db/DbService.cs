﻿using gtmp.evilempire.entities;
using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace gtmp.evilempire.db
{
    public sealed class DbService : IDbService, IDisposable
    {
        DbEnvironment _dbe;

        public DbService(Stream stream)
        {
            _dbe = new DbEnvironment(stream);
            AddKnownEntities();
        }

        public DbService(string databaseRootPath)
        {
            _dbe = new DbEnvironment(databaseRootPath);
            AddKnownEntities();
            AddSequences();
        }

        void AddKnownEntities()
        {
            _dbe.AddKnownEntity<User, string>("user", ks => ks.Login, ks => ks.Login, true);
            _dbe.AddKnownEntity<Character, string>("character", ks => ks.AssociatedLogin, ks => ks.AssociatedLogin, false);
            _dbe.AddKnownEntity<CharacterCustomization, int>("characterCustomization", ks => ks.CharacterId, ks => ks.CharacterId, true);
            _dbe.AddKnownEntity<CharacterInventory, int>("characterInventory", ks => ks.CharacterId, ks => ks.CharacterId, true);
            _dbe.AddKnownEntity<Vehicle, long>("vehicle", ks => ks.Id, ks => ks.Id, true);
        }

        void AddSequences()
        {
            _dbe.NextValueFor(Constants.Database.Sequences.CharacterIdSequence, 0);
            _dbe.NextInt64ValueFor(Constants.Database.Sequences.ItemIdSequence, Item.ZeroId + 1);
            _dbe.NextInt64ValueFor(Constants.Database.Sequences.VehicleIdSequence, Item.ZeroId + 1);
        }

        public T Insert<T>(T element)
        {
            return _dbe.Insert<T>(element);
        }

        public T Select<T, TKey>(TKey key)
        {
            return _dbe.Select<T, TKey>(key);
        }

        public T Select<T, TKey>(Expression<Func<T, TKey>> keySelector, TKey key)
        {
            return _dbe.Select<T, TKey>(keySelector, key);
        }
        public IEnumerable<T> SelectMany<T, TKey>(TKey key)
        {
            return _dbe.SelectMany<T, TKey>(key);
        }

        public T Update<T>(T element)
        {
            return _dbe.Update<T>(element);
        }

        public object InsertOrUpdate(object element)
        {
            return _dbe.InsertOrUpdate(element);
        }

        public int NextValueFor(string sequence, int seed = 0)
        {
            return _dbe.NextValueFor(sequence, seed);
        }

        public long NextInt64ValueFor(string sequence, long seed = 0)
        {
            return _dbe.NextInt64ValueFor(sequence, seed);
        }

        public int? ValueFor(string sequence)
        {
            return _dbe.ValueFor(sequence);
        }

        public long? Int64ValueFor(string sequence)
        {
            return _dbe.Int64ValueFor(sequence);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_dbe")]
        public void Dispose()
        {
            _dbe?.Dispose();
            _dbe = null;
        }
    }
}

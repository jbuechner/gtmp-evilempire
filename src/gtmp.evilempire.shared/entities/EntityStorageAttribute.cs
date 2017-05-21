using System;

namespace gtmp.evilempire.entities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class EntityStorageAttribute : Attribute
    {
        public string Storage { get; }

        public string KeyMember { get; }

        public EntityStorageAttribute(string storage)
            : this(storage, null)
        {
        }

        public EntityStorageAttribute(string storage, string keyMember)
        {
            this.Storage = storage;
            this.KeyMember = keyMember;
        }
    }
}

using System;

namespace gtmp.evilempire.entities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class EntityStorageAttribute : Attribute
    {
        public string Storage { get; }

        public string KeyMember { get; }

        public EntityStorageAttribute(string storage, string keyMember = null)
        {
            this.Storage = storage;
            this.KeyMember = keyMember;
        }
    }
}

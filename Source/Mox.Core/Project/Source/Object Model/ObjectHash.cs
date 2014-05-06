using System;
using System.Collections.Generic;

namespace Mox
{
    public class ObjectHash
    {
        private readonly ObjectManager m_manager;
        private readonly Dictionary<int, HashInfo> m_hashedObjects = new Dictionary<int, HashInfo>();

        public ObjectHash(ObjectManager manager)
        {
            Throw.IfNull(manager, "manager");
            m_manager = manager;
        }

        public int Hash(int identifier)
        {
            return Hash(m_manager.GetObjectByIdentifier<Object>(identifier));
        }

        public int Hash(Object obj)
        {
            if (ReferenceEquals(obj, null))
                return 0;

            HashInfo hashInfo;
            if (m_hashedObjects.TryGetValue(obj.Identifier, out hashInfo))
            {
                Throw.InvalidOperationIf(!hashInfo.IsReady, "Hashing cyclical references is not supported");
                return hashInfo.Value;
            }

            hashInfo.IsReady = false;
            m_hashedObjects[obj.Identifier] = hashInfo;

            Hash hash = new Hash();

            // Always include the object's concrete type
            hash.Add(obj.GetType().MetadataToken);

            if (!obj.ComputeHash(hash))
            {
                var manipulator = ObjectManipulators.GetManipulator(obj);
                manipulator.ComputeHash(obj, hash, this);
            }

            hashInfo.IsReady = true;
            hashInfo.Value = hash.Value;
            m_hashedObjects[obj.Identifier] = hashInfo;

            return hash.Value;
        }

        private struct HashInfo
        {
            public int Value;
            public bool IsReady;
        }
    }

    public interface IHashable
    {
        void ComputeHash(Hash hash, ObjectHash context);
    }
}

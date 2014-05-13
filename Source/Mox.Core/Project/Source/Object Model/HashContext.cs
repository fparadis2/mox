using System;
using System.Threading;

namespace Mox
{
    public class HashContext
    {
        private static int ms_nextHashVersion = 0;

        private readonly ObjectManager m_manager;
        private readonly int m_version;

        public HashContext(ObjectManager manager)
        {
            Throw.IfNull(manager, "manager");
            m_manager = manager;
            m_version = Interlocked.Increment(ref ms_nextHashVersion);
        }

        public int Version
        {
            get { return m_version; }
        }

        public int Hash(int identifier)
        {
            Object obj = m_manager.GetObjectByIdentifier<Object>(identifier);
            return obj.ComputeHash(this);
        }
    }

    public interface IHashable
    {
        void ComputeHash(Hash hash, HashContext context);
    }
}

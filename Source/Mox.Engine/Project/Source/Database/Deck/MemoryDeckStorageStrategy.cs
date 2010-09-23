using System;
using System.Collections.Generic;
using System.IO;

namespace Mox.Database
{
    public class MemoryDeckStorageStrategy : IDeckStorageStrategy
    {
        private class PersistenceStream : MemoryStream
        {
            private readonly Action<MemoryStream> m_disposeAction;

            public PersistenceStream(Action<MemoryStream> disposeAction)
            {
                m_disposeAction = disposeAction;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    m_disposeAction(this);
                }

                base.Dispose(disposing);
            }
        }

        private readonly Dictionary<Guid, byte[]> m_persistedDecks = new Dictionary<Guid, byte[]>();

        public bool IsPersisted(Deck deck)
        {
            return m_persistedDecks.ContainsKey(deck.Guid);
        }

        public int PersistedDecksCount
        {
            get { return m_persistedDecks.Count; }
        }

        public void LoadAll(Action<Stream, Guid> loadingAction)
        {
            foreach (var pair in m_persistedDecks)
            {
                using (MemoryStream stream = new MemoryStream(pair.Value))
                {
                    loadingAction(stream, pair.Key);
                }
            }
        }

        public Stream OpenWrite(Guid guid)
        {
            return new PersistenceStream(stream => m_persistedDecks[guid] = stream.ToArray());
        }

        public void Delete(Guid guid)
        {
            m_persistedDecks.Remove(guid);
        }
    }
}
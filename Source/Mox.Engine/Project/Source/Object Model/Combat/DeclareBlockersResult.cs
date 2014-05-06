// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mox
{
    [Serializable]
    public class DeclareBlockersResult : IHashable
    {
        #region Inner Types

        [Serializable]
        public struct BlockingCreature : IComparable<BlockingCreature>
        {
            public readonly int BlockingCreatureId;
            public readonly int BlockedCreatureId;

            internal BlockingCreature(int blockingCreature, int blockedCreature)
            {
                Debug.Assert(blockedCreature != ObjectManager.InvalidIdentifier);

                BlockingCreatureId = blockingCreature;
                BlockedCreatureId = blockedCreature;
            }

            public BlockingCreature(Card blockingCreature, Card blockedCreature)
                : this(blockingCreature.Identifier, blockedCreature.Identifier)
            {
            }

            public int CompareTo(BlockingCreature other)
            {
                int result = BlockedCreatureId.CompareTo(other.BlockedCreatureId);
                if (result != 0)
                    return result;

                return BlockingCreatureId.CompareTo(other.BlockingCreatureId);
            }
        }

        #endregion

        #region Variables

        private readonly List<BlockingCreature> m_blockers = new List<BlockingCreature>();

        #endregion

        #region Ctor

        public DeclareBlockersResult(params BlockingCreature[] blockingCreatures)
            : this((IEnumerable<BlockingCreature>)blockingCreatures)
        {
        }

        public DeclareBlockersResult(IEnumerable<BlockingCreature> blockingCreatures)
        {
            m_blockers.AddRange(blockingCreatures);
            m_blockers.Sort();
        }

        private DeclareBlockersResult(IEnumerable<BlockingCreature> blockingCreatures, bool forCloning)
        {
            // Assume already sorted here
            m_blockers.AddRange(blockingCreatures);
        }

        #endregion

        #region Properties

        public IList<BlockingCreature> Blockers
        {
            get { return m_blockers.AsReadOnly(); }
        }

        public static DeclareBlockersResult Empty
        {
            get { return new DeclareBlockersResult(); }
        }

        #endregion

        #region Methods

        internal DeclareBlockersResult Clone()
        {
            return new DeclareBlockersResult(m_blockers, false);
        }

        public IEnumerable<Card> GetBlockers(Game game)
        {
            var result = from pair in m_blockers
                         where pair.BlockingCreatureId != ObjectManager.InvalidIdentifier
                         select game.GetObjectByIdentifier<Card>(pair.BlockingCreatureId);
            return result.Distinct();
        }

        public IEnumerable<KeyValuePair<Card, Card>> GetBlockerPairs(Game game)
        {
            return from pair in m_blockers
                   where pair.BlockingCreatureId != ObjectManager.InvalidIdentifier
                   select new KeyValuePair<Card, Card>(game.GetObjectByIdentifier<Card>(pair.BlockedCreatureId), game.GetObjectByIdentifier<Card>(pair.BlockingCreatureId));
        }

        internal bool Remove(Card card)
        {
            // Remove all pairs corresponding to the card being an attacker
            bool changed = m_blockers.RemoveAll(pair => pair.BlockedCreatureId == card.Identifier) > 0;

            for (int i = m_blockers.Count - 1; i >= 0; i --)
            {
                BlockingCreature pair = m_blockers[i];

                if (pair.BlockingCreatureId == card.Identifier)
                {
                    changed = true;
                    m_blockers.RemoveAt(i);
                    m_blockers.Add(new BlockingCreature(ObjectManager.InvalidIdentifier, pair.BlockedCreatureId));
                }
            }

            if (changed)
                m_blockers.Sort();

            return changed;
        }

        public override string ToString()
        {
            return string.Format("[Declare {0} blocker(s) ({1})]", m_blockers.Count, m_blockers.Select<BlockingCreature, string>(ToString).Join(", "));
        }

        public void ComputeHash(Hash hash, ObjectHash context)
        {
            for (int i = 0; i < m_blockers.Count; i++)
            {
                hash.Add(context.Hash(m_blockers[i].BlockedCreatureId));
                hash.Add(context.Hash(m_blockers[i].BlockingCreatureId));
            }
        }

        private static string ToString(BlockingCreature blockerPair)
        {
            return string.Format("{0}=>{1}", blockerPair.BlockingCreatureId, blockerPair.BlockedCreatureId);
        }

        #endregion
    }
}

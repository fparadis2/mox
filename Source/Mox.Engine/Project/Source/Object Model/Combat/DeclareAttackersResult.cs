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
    public class DeclareAttackersResult : IHashable
    {
        #region Variables

        private readonly List<int> m_attackers = new List<int>();

        #endregion

        #region Ctor

        public DeclareAttackersResult(params Card[] attackingCreatures)
            : this(attackingCreatures.Select(c => c.Identifier))
        {
            m_attackers.SortAndRemoveDuplicates();
        }

        private DeclareAttackersResult(IEnumerable<int> attackingCreatures)
        {
            // Assume already sorted here
            m_attackers.AddRange(attackingCreatures);
        }

        #endregion

        #region Properties

        public static DeclareAttackersResult Empty
        {
            get { return new DeclareAttackersResult(); }
        }

        internal IList<int> AttackerIdentifiers
        {
            get { return m_attackers.AsReadOnly(); }
        }

        public bool IsEmpty
        {
            get { return m_attackers.Count == 0; }
        }

        #endregion

        #region Methods

        internal DeclareAttackersResult Clone()
        {
            return new DeclareAttackersResult(m_attackers);
        }

        internal bool Remove(Card card)
        {
            return m_attackers.RemoveAll(i => i == card.Identifier) > 0;
        }

        public IEnumerable<Card> GetAttackers(Game game)
        {
            return m_attackers.Select(game.GetObjectByIdentifier<Card>);
        }

        public override string ToString()
        {
            return string.Format("[Declare {0} attacker(s) ({1})]", m_attackers.Count, m_attackers.Join(", "));
        }

        public void ComputeHash(Hash hash, HashContext context)
        {
            for (int i = 0; i < m_attackers.Count; i++)
                hash.Add(context.Hash(m_attackers[i]));
        }

        #endregion
    }
}

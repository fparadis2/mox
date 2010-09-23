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

namespace Mox.Flow
{
    public sealed class ChoiceRecorder
    {
        #region Inner Types

        private class ChoiceEntry
        {
            public readonly object Choice;
            public ChoiceEntry Next;

            public ChoiceEntry(object choice)
            {
                Choice = choice;
            }
        }

        #endregion

        #region Variables

        private ChoiceEntry m_head; // Start point
        private ChoiceEntry m_read; // Next replay point
        private ChoiceEntry m_tail; // Insertion point

        #endregion

        #region Constructor

        public ChoiceRecorder()
        {
        }

        private ChoiceRecorder(ChoiceRecorder other)
        {
            m_head = other.m_head;
            m_tail = other.m_tail;
            m_read = m_head;
        }

        #endregion

        #region Methods

        public bool TryReplay(out object choice)
        {
            if (m_read != null)
            {
                choice = m_read.Choice;
                m_read = FindNextRead(m_read);
                return true;
            }

            choice = null;
            return false;
        }

        private ChoiceEntry FindNextRead(ChoiceEntry currentRead)
        {
            ChoiceEntry result = null;
            for (ChoiceEntry entry = m_tail; entry != null && entry != currentRead; entry = entry.Next)
            {
                result = entry;
            }
            return result;
        }

        public void Record(object choice)
        {
            Throw.InvalidOperationIf(m_read != null, "Cannot record while still reading");

            ChoiceEntry newEntry = new ChoiceEntry(choice)
            {
                Next = m_tail
            };

            m_tail = newEntry;

            if (m_head == null)
            {
                m_head = m_tail;
            }
        }

        public ChoiceRecorder Clone()
        {
            return new ChoiceRecorder(this);
        }

        #endregion
    }
}

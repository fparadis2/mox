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
using System.Collections.Generic;
using System.Diagnostics;
using Mox.Collections;
using Mox.Transactions;

using Mox.Abilities;

namespace Mox
{
    /// <summary>
    /// The spell stack
    /// </summary>
    /// <remarks>
    /// Not to be confused with the stack zone (which is where the cards on the stack are); this stack holds spells only.
    /// </remarks>
    public class SpellStack : IEnumerable<Spell>
    {
        #region Inner Types

        [Serializable]
        private abstract class SpellStackCommand : Command
        {
            protected static SpellStack GetSpellStack(ObjectManager manager)
            {
                return ((Game)manager).SpellStack;
            }
        }

        [Serializable]
        private class PopCommand : SpellStackCommand
        {
            #region Variables

            [NonSerialized]
            private Spell m_removedSpell;

            #endregion

            #region Overrides of Command

            /// <summary>
            ///  Executes (does or redoes) the command.
            /// </summary>
            public override void Execute(ObjectManager manager)
            {
                m_removedSpell = GetSpellStack(manager).PopInternal();
            }

            /// <summary>
            ///  Unexecutes (undoes) the command.
            /// </summary>
            public override void Unexecute(ObjectManager manager)
            {
                Debug.Assert(m_removedSpell != null);

                GetSpellStack(manager).PushInternal(m_removedSpell);
            }

            #endregion
        }

        [Serializable]
        private abstract class BasePushCommand : SpellStackCommand, ISynchronizableCommand
        {
            #region Variables

            [NonSerialized]
            protected Spell m_spell;

            #endregion

            #region Methods

            protected virtual Spell GetSpell(ObjectManager manager)
            {
                Debug.Assert(m_spell.Game == manager);
                return m_spell;
            }

            #endregion

            #region Overrides of Command

            /// <summary>
            ///  Executes (does or redoes) the command.
            /// </summary>
            public override void Execute(ObjectManager manager)
            {
                GetSpellStack(manager).PushInternal(GetSpell(manager));
            }

            /// <summary>
            ///  Unexecutes (undoes) the command.
            /// </summary>
            public override void Unexecute(ObjectManager manager)
            {
                GetSpellStack(manager).PopInternal();
            }

            #endregion

            #region Implementation of ISynchronizableCommand

            /// <summary>
            /// Whether this particular property should only visible to the owner of the <see cref="Object"/>.
            /// </summary>
            public bool IsPublic
            {
                get { return true; }
            }

            /// <summary>
            /// Object associated with the synchronizable command, if any.
            /// </summary>
            public Object GetObject(ObjectManager objectManager)
            {
                return null;
            }

            /// <summary>
            /// Gets the synchronization command for this command (usually the command itself).
            /// </summary>
            public ICommand Synchronize()
            {
                return new SerializablePushCommand(m_spell);
            }

            [Serializable]
            private class SerializablePushCommand : BasePushCommand
            {
                #region Variables

                private readonly Spell.Storage m_storage;

                #endregion

                #region Constructor

                public SerializablePushCommand(Spell spell)
                {
                    m_storage = spell.ToStorage();
                }

                #endregion

                #region Properties

                protected override Spell GetSpell(ObjectManager manager)
                {
                    if (m_spell == null)
                    {
                        m_spell = m_storage.CreateSpell((Game)manager);
                    }

                    return base.GetSpell(manager);
                }

                #endregion
            }

            #endregion
        }

        /// <summary>
        /// Delibarately non serializable to avoid errors
        /// </summary>
        private class PushCommand : BasePushCommand
        {
            public PushCommand(Spell spell)
            {
                m_spell = spell;
            }
        }

        #endregion

        #region Variables

        private readonly Game m_game;
        private ImmutableStack<Spell> m_stack = new ImmutableStack<Spell>();

        #endregion

        #region Constructor

        public SpellStack(Game game)
        {
            Throw.IfNull(game, "game");
            m_game = game;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns whether the stack is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return m_stack.IsEmpty; }
        }

        private IObjectController Controller
        {
            get { return m_game.Controller; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the top-most spell on the stack. Null if stack is empty.
        /// </summary>
        /// <returns></returns>
        public Spell Peek()
        {
            return m_stack.Peek();
        }

        /// <summary>
        /// Pushes a spell on the stack.
        /// </summary>
        /// <param name="spell"></param>
        internal void Push(Spell spell)
        {
            Throw.IfNull(spell, "spell");
            Throw.InvalidArgumentIf(spell.Game != m_game, "Cross-game operation", "spell");
            Controller.Execute(new PushCommand(spell));
        }

        private void PushInternal(Spell spell)
        {
            m_stack = m_stack.Push(spell);
            OnCollectionChanged(new CollectionChangedEventArgs<Spell>(CollectionChangeAction.Add, new [] { spell }));
        }

        /// <summary>
        /// Pops a spell from the stack.
        /// </summary>
        internal Spell Pop()
        {
            Spell top = Peek();
            Controller.Execute(new PopCommand());
            return top;
        }

        private Spell PopInternal()
        {
            Spell top = Peek();
            m_stack = m_stack.Pop();
            OnCollectionChanged(new CollectionChangedEventArgs<Spell>(CollectionChangeAction.Remove, new[] { top }));
            return top;
        }

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // To be consistent with Zone.ToString()
            return "[Zone: SpellStack]";
        }

        #endregion

        #region Events

        public event EventHandler<CollectionChangedEventArgs<Spell>> CollectionChanged;

        private void OnCollectionChanged(CollectionChangedEventArgs<Spell> e)
        {
            CollectionChanged.Raise(this, e);
        }

        #endregion

        #region IEnumerable<Spell> Members

        public IEnumerator<Spell> GetEnumerator()
        {
            return m_stack.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

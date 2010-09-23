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
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace Mox.UI
{
    /// <summary>
    /// Translates <see cref="IClientController"/> actions into <see cref="InteractionModel"/> terms.
    /// </summary>
    public partial class InteractionController : IDisposable
    {
        #region Inner Types

        protected abstract class InteractionBase
        {
            internal InteractionController Controller
            {
                get;
                set;
            }

            protected GameViewModel Model
            {
                get { return Controller.m_model; }
            }

            protected Game Game
            {
                get 
                {
                    Debug.Assert(Model.Source != null);
                    return Model.Source; 
                }
            }

            protected Player Player
            {
                get 
                {
                    return Controller.Player;
                }
            }

            protected PlayerViewModel PlayerViewModel
            {
                get 
                {
                    return Controller.PlayerViewModel;
                }
            }

            public virtual void Run()
            {
            }

            /// <summary>
            /// For tests... :(
            /// </summary>
            /// <param name="result"></param>
            public abstract void End(object result);
        }

        /// <summary>
        /// Base class for interactions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected abstract class Interaction<T> : InteractionBase, IInteraction<T>
        {
            #region Implementation of IInteraction<T>

            /// <summary>
            /// The result of the interaction, or null/default value if the operation has not completed.
            /// </summary>
            public T Result { get; protected set; }

            /// <summary>
            /// A wait handle that can be used to wait for the operation to finish.
            /// </summary>
            public WaitHandle WaitHandle
            {
                get { return Controller.WaitHandle; }
            }

            /// <summary>
            /// Returns true if the interaction has completed.
            /// </summary>
            public bool IsCompleted
            {
                get;
                private set;
            }

            /// <summary>
            /// Stops the calling thread until the interaction completes.
            /// </summary>
            public void Wait()
            {
                WaitHandle.WaitOne();
            }

            #endregion

            #region Methods

            /// <summary>
            /// Ends this interaction.
            /// </summary>
            protected virtual void End()
            {
                IsCompleted = true;
                Controller.EndInteraction(this);
            }

            /// <summary>
            /// For tests... :(
            /// </summary>
            /// <param name="result"></param>
            public sealed override void End(object result)
            {
                Result = (T)result;
                End();
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly GameViewModel m_model;
        private readonly Dispatcher m_dispatcher;
        private readonly ManualResetEvent m_waitHandle = new ManualResetEvent(false);

        private InteractionBase m_currentInteraction;

        #endregion

        #region Constructor

        public InteractionController(GameViewModel model, Dispatcher dispatcher)
        {
            Throw.IfNull(model, "model");

            m_model = model;
            m_dispatcher = dispatcher;
        }

        public virtual void Dispose()
        {
            m_waitHandle.Close();
        }

        #endregion

        #region Properties

        private ManualResetEvent WaitHandle
        {
            get { return m_waitHandle; }
        }

        private Player Player
        {
            get
            {
                Debug.Assert(PlayerViewModel.Source != null);
                return PlayerViewModel.Source;
            }
        }

        private PlayerViewModel PlayerViewModel
        {
            get
            {
                Debug.Assert(m_model.MainPlayer != null);
                return m_model.MainPlayer;
            }
        }

        #endregion

        #region Methods

        #region Interaction management

        protected T BeginInteraction<T>()
            where T : InteractionBase, new()
        {
            return BeginInteraction<T>(null);
        }

        protected virtual T BeginInteraction<T>(Action<T> initializer)
            where T : InteractionBase, new()
        {
            Throw.InvalidOperationIf(m_currentInteraction != null, "An interaction is already in progress!");

            T interaction = new T { Controller = this };
            m_currentInteraction = interaction;

            WaitHandle.Reset();
            if (initializer != null)
            {
                initializer(interaction);
            }

            RunInteraction(interaction);
            return interaction;
        }

        private void RunInteraction(InteractionBase interaction)
        {
            if (m_dispatcher == null || m_dispatcher.Thread == Thread.CurrentThread)
            {
                RunInteractionImpl(interaction);
            }
            else
            {
                m_dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Action(() => RunInteractionImpl(interaction)));
            }
        }

        private static void RunInteractionImpl(InteractionBase interaction)
        {
            interaction.Run();
            CommandManager.InvalidateRequerySuggested();
        }

        private void EndInteraction(InteractionBase interaction)
        {
            Throw.InvalidOperationIf(interaction != m_currentInteraction, "The given interaction is not in progress");
            m_currentInteraction = null;
            WaitHandle.Set();

            m_model.ResetInteraction();
        }

        #endregion

        #region Helpers

        protected void TagCardThatCanBePlayed(ExecutionEvaluationContext context)
        {
            foreach (CardViewModel cardViewModel in m_model.AllCards)
            {
                Card card = cardViewModel.Source;
                if (card != null)
                {
                    cardViewModel.CanBeChosen = card.Abilities.Any(ability => ability.CanPlay(Player, context));
                }
            }
        }

        #endregion

        #endregion
    }
}

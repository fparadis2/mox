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
using System.Windows.Input;
using Mox.Flow;
using Mox.Lobby;

namespace Mox.UI.Game
{
    public partial class InteractionController
    {
        #region Variables

        private readonly GameViewModel m_model;

        private Interaction m_currentInteraction;
        private InteractionRequestedEventArgs m_currentRequest;

        #endregion

        #region Constructor

        public InteractionController(GameViewModel model)
        {
            Throw.IfNull(model, "model");
            m_model = model;
        }

        #endregion

        #region Properties

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

        public bool IsCompleted
        {
            get { return m_currentInteraction == null; }
        }

        #endregion

        #region Methods

        #region Interaction management

        public void BeginInteraction(InteractionRequestedEventArgs e)
        {
            BeginInteraction(e.Choice, e);
        }

        protected void BeginInteraction(Choice choice, InteractionRequestedEventArgs request)
        {
            Interaction interaction = ControllerCreator.Create(choice);
            BeginInteraction(interaction, request);
        }

        protected void BeginInteraction(Interaction interaction, InteractionRequestedEventArgs request)
        {
            Throw.InvalidOperationIf(m_currentInteraction != null || m_currentRequest != null, "An interaction is already in progress!");

            interaction.Controller = this;

            m_currentInteraction = interaction;
            m_currentRequest = request;

            RunInteraction(interaction);
        }

        private static void RunInteraction(Interaction interaction)
        {
            interaction.Run();
            CommandManager.InvalidateRequerySuggested();
        }

        private void EndInteraction(Interaction interaction, object result)
        {
            Throw.InvalidOperationIf(interaction != m_currentInteraction, "The given interaction is not in progress");

            SendResult(m_currentRequest, result);

            m_currentInteraction = null;
            m_currentRequest = null;

            m_model.ResetInteraction();
        }

        protected virtual void SendResult(InteractionRequestedEventArgs request, object result)
        {
            request.SendResult(result);
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
                    cardViewModel.CanChoose = card.Abilities.Any(ability => ability.CanPlay(Player, context));
                }
            }
        }

        #endregion

        #endregion

        #region Inner Types

        protected abstract class Interaction
        {
            public InteractionController Controller
            {
                get;
                set;
            }

            protected GameViewModel Model
            {
                get { return Controller.m_model; }
            }

            protected Mox.Game Game
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
            protected virtual void End(object result)
            {
                Controller.EndInteraction(this, result);
            }
        }

        private static class ControllerCreator
        {
            #region Variables

            private static readonly Dictionary<System.Type, Creator> ms_creators = new Dictionary<System.Type, Creator>();

            #endregion

            #region Constructor

            static ControllerCreator()
            {
                ms_creators.Add(typeof(MulliganChoice), c => new MulliganInteraction());
                ms_creators.Add(typeof(ModalChoice), c => new AskModalChoiceInteraction { Context = ((ModalChoice)c).Context });
                ms_creators.Add(typeof(DeclareAttackersChoice), c => new DeclareAttackersInteraction { AttackInfo = ((DeclareAttackersChoice)c).AttackContext });
                ms_creators.Add(typeof(DeclareBlockersChoice), c => new DeclareBlockersInteraction { Context = ((DeclareBlockersChoice)c).BlockContext });
                ms_creators.Add(typeof(GivePriorityChoice), c => new GivePriorityInteraction());
                ms_creators.Add(typeof(PayManaChoice), c => new PayManaInteraction { ManaCost = ((PayManaChoice)c).ManaCost });
                ms_creators.Add(typeof(TargetChoice), c => CreateTargetInteraction((TargetChoice)c));
            }

            #endregion

            #region Methods

            public static Interaction Create(Choice choice)
            {
                Creator creator;
                if (!ms_creators.TryGetValue(choice.GetType(), out creator))
                {
                    throw new InvalidProgramException(string.Format("Unknown choice type: {0}", choice.GetType()));
                }
                return creator(choice);
            }

            private static TargetInteraction CreateTargetInteraction(TargetChoice choice)
            {
                return new TargetInteraction
                {
                    AllowCancel = choice.Context.AllowCancel,
                    TargetContextType = choice.Context.Type,
                    Targets = choice.Context.Targets
                };
            }

            #endregion

            #region Inner Types

            private delegate Interaction Creator(Choice choice);

            #endregion
        }

        #endregion
    }
}

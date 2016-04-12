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
using Caliburn.Micro;

namespace Mox.UI.Game
{
    public class GameStateViewModel : PropertyChangedBase
    {
        #region Variables

        private StepViewModel m_currentStep;
        private readonly StepsCollectionViewModel m_steps = new StepsCollectionViewModel();
        private PlayerViewModel m_activePlayer;

        #endregion

        #region Constructor

        public GameStateViewModel()
        {
            m_currentStep = m_steps.PrecombatMain;
        }

        #endregion

        #region Properties

        #region Synchronization

        // Fits with the names of the properties in GameState. Don't remove!

        public Steps CurrentStep
        {
            set { Step = GetStep(value); }
        }

        public Phases CurrentPhase
        {
            set
            {
                var step = GetStep(value);
                if (step != null)
                    Step = step;
            }
        }

        #endregion

        public StepViewModel Step
        {
            get { return m_currentStep; }
            set
            {
                m_currentStep = value;
                NotifyOfPropertyChange();
            }
        }

        public IList<StepViewModel> Steps
        {
            get { return m_steps; }
        }

        public PlayerViewModel ActivePlayer
        {
            get { return m_activePlayer; }
            set
            {
                if (m_activePlayer != value)
                {
                    m_activePlayer = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        public StepViewModel GetStep(Steps step)
        {
            switch (step)
            {
                case Mox.Steps.Untap: return m_steps.Untap;
                case Mox.Steps.Upkeep: return m_steps.Upkeep;
                case Mox.Steps.Draw: return m_steps.Draw;
                case Mox.Steps.BeginningOfCombat: return m_steps.BeginningOfCombat;
                case Mox.Steps.DeclareAttackers: return m_steps.DeclareAttackers;
                case Mox.Steps.DeclareBlockers: return m_steps.DeclareBlockers;
                case Mox.Steps.CombatDamage: return m_steps.CombatDamage;
                case Mox.Steps.EndOfCombat: return m_steps.EndOfCombat;
                case Mox.Steps.End: return m_steps.EndOfTurn;
                case Mox.Steps.Cleanup: return m_steps.Cleanup;
                default:
                    throw new NotImplementedException();
            }
        }

        public StepViewModel GetStep(Phases phase)
        {
            switch (phase)
            {
                case Phases.PrecombatMain: return m_steps.PrecombatMain;
                case Phases.PostcombatMain: return m_steps.PostcombatMain;
                default:
                    return null;
            }
        }

        #endregion
    }
}

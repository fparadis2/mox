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
using System.Linq;
using System.Text;

namespace Mox.UI
{
    public enum MTGSteps
    {
        Untap,
        Upkeep,
        Draw,
        PrecombatMain,
        BeginningOfCombat,
        DeclareAttackers,
        DeclareBlockers,
        CombatDamage,
        EndOfCombat,
        PostcombatMain,
        EndOfTurn,
        Cleanup
    }

    public class GameStateViewModel : ViewModel
    {
        #region Variables

        private MTGSteps m_currentStep;
        private PlayerViewModel m_activePlayer;

        #endregion

        #region Properties

        #region Synchronization

        public Steps CurrentStep
        {
            set 
            {
                CurrentMTGStep = GetMTGStep(value);
            }
        }

        public Phases CurrentPhase
        {
            set 
            {
                switch (value)
                {
                    case Phases.PrecombatMain:
                        CurrentMTGStep = MTGSteps.PrecombatMain; break;
                    case Phases.PostcombatMain:
                        CurrentMTGStep = MTGSteps.PostcombatMain; break;
                    default:
                        break;
                }
            }
        }

        #endregion

        public MTGSteps CurrentMTGStep
        {
            get { return m_currentStep; }
            set 
            {
                if (m_currentStep != value)
                {
                    m_currentStep = value;
                    OnPropertyChanged("CurrentMTGStep");
                }
            }
        }

        public PlayerViewModel ActivePlayer
        {
            get { return m_activePlayer; }
            set
            {
                if (m_activePlayer != value)
                {
                    m_activePlayer = value;
                    OnPropertyChanged("ActivePlayer");
                }
            }
        }

        #endregion

        #region Methods

        private static MTGSteps GetMTGStep(Steps step)
        {
            switch (step)
            {
                case Steps.Untap: return MTGSteps.Untap;
                case Steps.Upkeep: return MTGSteps.Upkeep;
                case Steps.Draw: return MTGSteps.Draw;
                case Steps.BeginningOfCombat: return MTGSteps.BeginningOfCombat;
                case Steps.DeclareAttackers: return MTGSteps.DeclareAttackers;
                case Steps.DeclareBlockers: return MTGSteps.DeclareBlockers;
                case Steps.CombatDamage: return MTGSteps.CombatDamage;
                case Steps.EndOfCombat: return MTGSteps.EndOfCombat;
                case Steps.End: return MTGSteps.EndOfTurn;
                case Steps.Cleanup: return MTGSteps.Cleanup;
                default: throw new NotImplementedException();
            }
        }

        #endregion
    }
}

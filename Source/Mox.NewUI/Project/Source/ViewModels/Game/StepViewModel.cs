﻿using System;
using Caliburn.Micro;

namespace Mox.UI.Game
{
    public class StepViewModel : PropertyChangedBase
    {
        #region Variables

        private Phases? m_phase;
        private Steps m_step;

        #endregion

        #region Properties

        internal Steps CurrentStep
        {
            set
            {
                m_phase = null;
                m_step = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        internal Phases CurrentPhase
        {
            set
            {
                switch (value)
                {
                    case Phases.PrecombatMain:
                    case Phases.PostcombatMain:
                        m_phase = value;
                        NotifyOfPropertyChange(() => Name);
                        break;
                    default:
                        break;
                }
            }
        }

        public string Name
        {
            get
            {
                if (m_phase.HasValue)
                {
                    switch (m_phase.Value)
                    {
                        case Phases.PrecombatMain:
                            return "Precombat Main";
                        case Phases.PostcombatMain:
                            return "Postcombat Main";
                        default:
                            throw new NotImplementedException();
                    }
                }

                switch (m_step)
                {
                    case Steps.Untap: return "Untap";
                    case Steps.Upkeep: return "Upkeep";
                    case Steps.Draw: return "Draw";
                    case Steps.BeginningOfCombat: return "Beginning of Combat";
                    case Steps.DeclareAttackers: return "Declare Attackers";
                    case Steps.DeclareBlockers: return "Declare Blockers";
                    case Steps.CombatDamage: return "Combat Damage";
                    case Steps.EndOfCombat: return "End of Combat";
                    case Steps.End: return "End of Turn";
                    case Steps.Cleanup: return "Cleanup";
                    default: throw new NotImplementedException();
                }
            }
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            StepViewModel other = obj as StepViewModel;
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return m_phase == other.m_phase && m_step == other.m_step;
        }

        public override int GetHashCode()
        {
            if (m_phase != null)
            {
                return m_phase.Value.GetHashCode();
            }

            return m_step.GetHashCode();
        }

        #endregion
    }
}

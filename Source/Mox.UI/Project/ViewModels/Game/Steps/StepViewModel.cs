using System;
using Caliburn.Micro;

namespace Mox.UI.Game
{
    public class StepViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly Phases? m_phase;
        private readonly Steps m_step;

        #endregion

        #region Constructor

        public StepViewModel(Phases phase)
        {
            switch (phase)
            {
                case Phases.PrecombatMain:
                case Phases.PostcombatMain:
                    break;
                default:
                    throw new ArgumentException("Invalid phase");
            }

            m_phase = phase;
        }

        public StepViewModel(Steps step)
        {
            m_step = step;
        }

        #endregion

        #region Properties

        public string Name
        {
            get
            {
                if (m_phase.HasValue)
                {
                    switch (m_phase.Value)
                    {
                        case Phases.PrecombatMain:
                        case Phases.PostcombatMain:
                            return "Main";
                        default:
                            throw new NotImplementedException();
                    }
                }

                switch (m_step)
                {
                    case Steps.Untap: return "Untap";
                    case Steps.Upkeep: return "Upkeep";
                    case Steps.Draw: return "Draw";
                    case Steps.BeginningOfCombat: return "Begin Combat";
                    case Steps.DeclareAttackers: return "Attack";
                    case Steps.DeclareBlockers: return "Block";
                    case Steps.CombatDamage: return "Damage";
                    case Steps.EndOfCombat: return "End Combat";
                    case Steps.End: return "End";
                    case Steps.Cleanup: return "Cleanup";
                    default: throw new NotImplementedException();
                }
            }
        }

        public string FullName
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

        public bool ShowStops
        {
            get
            {
                if (m_phase.HasValue)
                    return true;

                switch (m_step)
                {
                    case Steps.Upkeep:
                    case Steps.Draw:
                    case Steps.BeginningOfCombat:
                    case Steps.DeclareAttackers:
                    case Steps.DeclareBlockers:
                    case Steps.CombatDamage:
                    case Steps.EndOfCombat:
                    case Steps.End:
                        return true;

                    case Steps.Untap:
                    case Steps.Cleanup:
                        return false;
                    
                    default: 
                        throw new NotImplementedException();
                }
            }
        }

        private bool m_stopOnMyTurn;

        public bool StopOnMyTurn
        {
            get { return m_stopOnMyTurn; }
            set
            {
                if (m_stopOnMyTurn != value)
                {
                    m_stopOnMyTurn = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private bool m_stopOnOpponentTurn;

        public bool StopOnOpponentTurn
        {
            get { return m_stopOnOpponentTurn; }
            set
            {
                if (m_stopOnOpponentTurn != value)
                {
                    m_stopOnOpponentTurn = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion
    }
}

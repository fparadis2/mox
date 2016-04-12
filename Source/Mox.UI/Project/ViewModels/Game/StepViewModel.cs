using System;

namespace Mox.UI.Game
{
    public class StepViewModel
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

        #endregion
    }
}

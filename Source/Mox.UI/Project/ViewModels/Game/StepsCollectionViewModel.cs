using System.Collections.Generic;

namespace Mox.UI.Game
{
    public class StepsCollectionViewModel : List<StepViewModel>
    {
        private readonly StepViewModel m_untap = new StepViewModel(Steps.Untap);
        private readonly StepViewModel m_upkeep = new StepViewModel(Steps.Upkeep);
        private readonly StepViewModel m_draw = new StepViewModel(Steps.Draw);

        private readonly StepViewModel m_precombatMain = new StepViewModel(Phases.PrecombatMain);

        private readonly StepViewModel m_beginningOfCombat = new StepViewModel(Steps.BeginningOfCombat);
        private readonly StepViewModel m_declareAttackers = new StepViewModel(Steps.DeclareAttackers);
        private readonly StepViewModel m_declareBlockers = new StepViewModel(Steps.DeclareBlockers);
        private readonly StepViewModel m_combatDamage = new StepViewModel(Steps.CombatDamage);
        private readonly StepViewModel m_endCombat = new StepViewModel(Steps.EndOfCombat);

        private readonly StepViewModel m_postcombatMain = new StepViewModel(Phases.PostcombatMain);

        private readonly StepViewModel m_endOfTurn = new StepViewModel(Steps.End);
        private readonly StepViewModel m_cleanup = new StepViewModel(Steps.Cleanup);

        public StepsCollectionViewModel()
        {
            Add(m_untap);
            Add(m_upkeep);
            Add(m_draw);

            Add(m_precombatMain);

            Add(m_beginningOfCombat);
            Add(m_declareAttackers);
            Add(m_declareBlockers);
            Add(m_combatDamage);
            Add(m_endCombat);

            Add(m_postcombatMain);

            Add(m_endOfTurn);
            Add(m_cleanup);
        }

        public StepViewModel Untap { get { return m_untap; } }
        public StepViewModel Upkeep { get { return m_upkeep; } }
        public StepViewModel Draw { get { return m_draw; } }

        public StepViewModel PrecombatMain { get { return m_precombatMain; } }

        public StepViewModel BeginningOfCombat { get { return m_beginningOfCombat; } }
        public StepViewModel DeclareAttackers { get { return m_declareAttackers; } }
        public StepViewModel DeclareBlockers { get { return m_declareBlockers; } }
        public StepViewModel CombatDamage { get { return m_combatDamage; } }
        public StepViewModel EndOfCombat { get { return m_endCombat; } }

        public StepViewModel PostcombatMain { get { return m_postcombatMain; } }

        public StepViewModel EndOfTurn { get { return m_endOfTurn; } }
        public StepViewModel Cleanup { get { return m_cleanup; } }
    }
}
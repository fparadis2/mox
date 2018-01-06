using System;
using System.Collections.Generic;
using System.Linq;
using Mox.Transactions;

namespace Mox.Abilities
{
    internal class TargetData
    {
        #region Variables

        private readonly Game m_game;
        private Dictionary<TargetCost, Resolvable<GameObject>> m_results = new Dictionary<TargetCost, Resolvable<GameObject>>();

        #endregion

        #region Constructor

        public TargetData(Game game)
        {
            Throw.IfNull(game, "game");
            m_game = game;
        }

        #endregion

        #region Methods

        public void Clear()
        {
            m_game.Controller.Execute(new ClearCommand());
        }

        public void SetTargetResult(TargetCost target, Resolvable<GameObject> result)
        {
            m_game.Controller.Execute(new SetResultCommand(target, result));
        }

        public Resolvable<GameObject> GetTargetResult(TargetCost target)
        {
            Resolvable<GameObject> result;
            if (!m_results.TryGetValue(target, out result))
            {
                throw new InvalidOperationException("Cannot get the result for this target. It either has not been played yet or is now invalid.");
            }
            return result;
        }

        public void ComputeHash(Hash hash, HashContext context)
        {
#warning todo spell_v2
            /*// Sort the targets by their source ability to get consistency
            var targetPairs = m_results.ToList();
            targetPairs.Sort((a, b) => a.Key.SourceAbility.Identifier.CompareTo(b.Key.SourceAbility.Identifier));

            foreach (var pair in targetPairs)
            {
                hash.Add(context.Hash(pair.Key.SourceAbility.Identifier));
                hash.Add(context.Hash(pair.Value.Identifier));
            }*/
        }

        #endregion

        #region Inner Types

        private abstract class TargetDataCommand : Command
        {
            protected static TargetData GetTargetData(ObjectManager manager)
            {
                return ((Game)manager).TargetData;
            }
        }

        private class ClearCommand : TargetDataCommand
        {
            #region Variables

            private Dictionary<TargetCost, Resolvable<GameObject>> m_oldResults;

            #endregion

            #region Overrides of Command

            public override void Execute(ObjectManager manager)
            {
                var targetData = GetTargetData(manager);
                m_oldResults = targetData.m_results;
                targetData.m_results = new Dictionary<TargetCost, Resolvable<GameObject>>();
            }

            public override void Unexecute(ObjectManager manager)
            {
                GetTargetData(manager).m_results = m_oldResults;
                m_oldResults = null;
            }

            #endregion
        }

        private class SetResultCommand : TargetDataCommand
        {
            #region Variables

            private readonly TargetCost m_target;
            private readonly Resolvable<GameObject> m_result;

            #endregion

            #region Constructor

            public SetResultCommand(TargetCost target, Resolvable<GameObject> result)
            {
                m_target = target;
                m_result = result;
            }

            #endregion

            #region Overrides of Command

            public override void Execute(ObjectManager manager)
            {
                var targetData = GetTargetData(manager);
                targetData.m_results.Add(m_target, m_result);
            }

            public override void Unexecute(ObjectManager manager)
            {
                var targetData = GetTargetData(manager);
                targetData.m_results.Remove(m_target);
            }

            #endregion
        }

        #endregion
    }
}

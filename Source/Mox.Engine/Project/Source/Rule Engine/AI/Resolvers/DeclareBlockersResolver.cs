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
using System.Reflection;
using System.Text;

namespace Mox.AI.Resolvers
{
    internal class DeclareBlockersResolver : BaseMTGChoiceResolver
    {
        #region Overrides of BaseMTGChoiceResolver

        /// <summary>
        /// Expected method name, used for asserts...
        /// </summary>
        public override string ExpectedMethodName
        {
            get { return "DeclareBlockers"; }
        }

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        /// <param name="choiceMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override IEnumerable<object> ResolveChoices(MethodBase choiceMethod, object[] args)
        {
            DeclareBlockersContext blockInfo = GetBlockerInfo(args);

            // Ouch, that hurts
            for (int numBlockers = blockInfo.LegalBlockers.Count; numBlockers >= 0; numBlockers--)
            {
                foreach (var blockerCombination in blockInfo.LegalBlockers.EnumerateCombinations(numBlockers))
                {
                    foreach (var attackerCombination in blockInfo.Attackers.EnumeratePermutations(numBlockers))
                    {
                        List<DeclareBlockersResult.BlockingCreature> blocks = new List<DeclareBlockersResult.BlockingCreature>();
                        
                        for (int i = 0; i < numBlockers; i++)
                        {
                            blocks.Add(new DeclareBlockersResult.BlockingCreature(blockerCombination[i], attackerCombination[i]));
                        }

                        yield return new DeclareBlockersResult(blocks);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the default choice for the choice context.
        /// </summary>
        /// <remarks>
        /// The actual value is not so important, only that it returns a valid value.
        /// </remarks>
        /// <param name="choiceMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object GetDefaultChoice(MethodBase choiceMethod, object[] args)
        {
            return DeclareBlockersResult.Empty;
        }

        private static DeclareBlockersContext GetBlockerInfo(object[] args)
        {
            return (DeclareBlockersContext)args[2];
        }

        #endregion
    }
}

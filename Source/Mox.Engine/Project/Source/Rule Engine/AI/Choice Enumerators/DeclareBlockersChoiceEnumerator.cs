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

using Mox.Flow;

namespace Mox.AI.ChoiceEnumerators
{
    internal class DeclareBlockersChoiceEnumerator : ChoiceEnumerator
    {
        #region Overrides of ChoiceEnumerator

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<object> EnumerateChoices(Game game, Choice choice)
        {
            DeclareBlockersContext blockInfo = ((DeclareBlockersChoice)choice).BlockContext;

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

        #endregion
    }
}

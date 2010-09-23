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
using Mox.Flow;
using Rhino.Mocks;

namespace Mox
{
    public static class CostExtensions
    {
        #region Immediate Costs

        public static void Expect_Execute(this ImmediateCost cost, Player player, bool result)
        {
            Expect_Execute(cost, player, result, null);
        }

        public static void Expect_Execute(this ImmediateCost cost, Player player, bool result, System.Action callback)
        {
            Expect.Call(cost.Execute(null, null)).IgnoreArguments().Return(result).Callback<Part<IGameController>.Context, Player>((context, callbackPlayer) =>
            {
                Assert.AreEqual(player, callbackPlayer);
                if (callback != null)
                {
                    callback();
                }
                return true;
            });
        }

        #endregion

        #region Delayed Costs

        public static void Expect_Execute(this DelayedCost cost, Player player, bool result)
        {
            Expect_Execute(cost, player, result, null);
        }

        public static void Expect_Execute(this DelayedCost cost, Player player, bool result, System.Action callback)
        {
            cost.Execute(null, null);
            LastCall.IgnoreArguments().Callback<Part<IGameController>.Context, Player>((context, callbackPlayer) =>
            {
                Assert.AreEqual(player, callbackPlayer);
                if (callback != null)
                {
                    callback();
                }
                context.PushArgument(result, DelayedCost.ArgumentToken);
                return true;
            });
        }

        #endregion
    }
}

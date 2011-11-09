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

using Mox.Flow;
using Rhino.Mocks;

namespace Mox
{
    public static class CostExtensions
    {
        #region Expectations

        public static void Expect_Execute(this Cost cost, Player player, bool result)
        {
            Expect_Execute(cost, player, result, null);
        }

        public static void Expect_Execute(this Cost cost, Player player, bool result, System.Action callback)
        {
            cost.Execute(null, null);
            LastCall.IgnoreArguments().Callback<NewPart.Context, Player>((context, callbackPlayer) =>
            {
                Assert.AreEqual(player, callbackPlayer);
                if (callback != null)
                {
                    callback();
                }
                Cost.PushResult(context, result);
                return true;
            });
        }

        #endregion
    }
}

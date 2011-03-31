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
using Mox.Database;

namespace Mox.UI.Browser
{
    internal class CardViewModel_DesignTime : CardViewModel
    {
        public CardViewModel_DesignTime()
            : base(CreateCardInfo(), null)
        {
        }

        private static CardInfo CreateCardInfo()
        {
            var database = new CardDatabase();

            List<SetInfo> sets = new List<SetInfo>
            {
                database.AddSet("TMP", "Forces of nature", "Joys of life", DateTime.Now), 
                database.AddSet("M11", "Creatures of the house", "Joys of life", DateTime.Now.Subtract(TimeSpan.FromDays(2)))
            };

            return DesignTimeCardDatabase.CreateCardInfo(database, sets);
        }
    }
}
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
using System.Collections.ObjectModel;

namespace Mox.UI.Game
{
    public class CardCollectionViewModel : ObservableCollection<CardViewModel>
    {
    }

    public class CardCollectionViewModel_DesignTime : CardCollectionViewModel
    {
        public CardCollectionViewModel_DesignTime()
        {
            GameViewModel model = new GameViewModel_DesignTime_Empty();

            PlayerViewModel player = new PlayerViewModel_DesignTime(model);

            Add(new CardViewModel_DesignTime(player));
            Add(new CardViewModel_DesignTime(player));
            Add(new CardViewModel_DesignTime(player));

            this[1].Tapped = true;
            this[2].CanChoose = true;
        }
    }
}

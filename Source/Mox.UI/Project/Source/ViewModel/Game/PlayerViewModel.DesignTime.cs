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

namespace Mox.UI
{
    public class DesignTimePlayerViewModel : PlayerViewModel
    {
        public DesignTimePlayerViewModel()
            : this(new EmptyDesignTimeGameViewModel())
        {
            GameViewModel.MainPlayer = this;
        }

        public DesignTimePlayerViewModel(GameViewModel model)
            : base(model)
        {
            Source = model.Source.CreatePlayer();

            Life = 20;
            Name = "Roger Moore";

            Hand.Add(new DesignTimeCardViewModel(this));
            Hand.Add(new DesignTimeCardViewModel(this));
            Hand.Add(new DesignTimeCardViewModel(this));

            Library.Add(new DesignTimeCardViewModel(this));
            Library.Add(new DesignTimeCardViewModel(this));
            Library.Add(new DesignTimeCardViewModel(this));

            Graveyard.Add(new DesignTimeCardViewModel(this));
            Graveyard.Add(new DesignTimeCardViewModel(this));
            Graveyard.Add(new DesignTimeCardViewModel(this));

            Battlefield.Add(new DesignTimeCardViewModel(this));
            Battlefield.Add(new DesignTimeCardViewModel(this));
            Battlefield.Add(new DesignTimeCardViewModel(this));

            ManaPool.Mana[Color.Red] = 10;
            ManaPool.Mana[Color.None] = 2;
            ManaPool.Mana[Color.Blue] = 3;
        }
    }
}

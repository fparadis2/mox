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
using System.Runtime.CompilerServices;

namespace Mox.UI.Game
{
    public class CardCollectionViewModel : ObservableCollection<CardViewModel>
    {
        public virtual void OnCardChanged(CardViewModel card, PropertyChangedEventArgs e)
        {
        }

        protected void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class OrderedCardCollectionViewModel : CardCollectionViewModel
    {
        public CardViewModel Top
        {
            get
            {
                return Count > 0 ? this[Count - 1] : null;
            }
        }

        protected override void InsertItem(int index, CardViewModel item)
        {
            base.InsertItem(index, item);

            if (index == Count - 1)
            {
                NotifyOfPropertyChange(nameof(Top));
            }
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            if (index == Count)
            {
                NotifyOfPropertyChange(nameof(Top));
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            NotifyOfPropertyChange(nameof(Top));
        }

        protected override void SetItem(int index, CardViewModel item)
        {
            base.SetItem(index, item);

            if (index == Count - 1)
            {
                NotifyOfPropertyChange(nameof(Top));
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);

            if (oldIndex == Count - 1 ||
                newIndex == Count - 1)
            {
                NotifyOfPropertyChange(nameof(Top));
            }
        }
    }

    public class CardCollectionViewModel_DesignTime : OrderedCardCollectionViewModel
    {
        public CardCollectionViewModel_DesignTime()
        {
            GameViewModel model = new GameViewModel_DesignTime_Empty();

            PlayerViewModel player = new PlayerViewModel_DesignTime(model);

            Add(new CardViewModel_DesignTime(player));
            Add(new CardViewModel_DesignTime(player));
            Add(new CardViewModel_DesignTime(player));

            this[2].InteractionType = InteractionType.Play;
        }
    }
}

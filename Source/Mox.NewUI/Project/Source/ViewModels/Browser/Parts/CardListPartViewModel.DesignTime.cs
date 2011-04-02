using System;

namespace Mox.UI.Browser
{
    public class CardListPartViewModel_DesignTime : CardListPartViewModel
    {
        public CardListPartViewModel_DesignTime()
            : base(new CardCollectionViewModel_DesignTime())
        {
        }
    }
}

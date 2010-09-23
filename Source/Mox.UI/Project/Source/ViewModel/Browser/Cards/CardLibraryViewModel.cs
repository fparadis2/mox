using System;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class CardLibraryViewModel : CardCollectionViewModel
    {
        public CardLibraryViewModel()
            : base(MasterCardDatabase.Instance.Cards)
        {
        }
    }
}

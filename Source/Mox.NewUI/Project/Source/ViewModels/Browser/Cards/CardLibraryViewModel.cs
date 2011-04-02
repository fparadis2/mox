using System;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class CardLibraryViewModel : CardCollectionViewModel
    {
        private static readonly CardLibraryViewModel ms_instance = new CardLibraryViewModel();

        private CardLibraryViewModel()
            : base(MasterCardDatabase.Instance.Cards, MasterCardFactory.Instance)
        {
        }

        public static CardLibraryViewModel Instance
        {
            get { return ms_instance; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Mox.UI.Library
{
    public class CardLibrariesViewModel : Conductor<CardLibraryViewModel>.Collection.OneActive
    {
        public CardLibrariesViewModel()
        {
            DisplayName = "Cards";

            Items.Add(new CardLibraryViewModel { DisplayName = "ALL CARDS" });
        }
    }
}

using Mox.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.UI.Library
{
    public class DecksViewModel : NavigationConductor
    {
        public DecksViewModel()
        {
            var library = new DeckLibraryViewModel(MasterDeckLibrary.Instance);

            Push(library);

            DisplayName = "Decks";
        }
    }
}

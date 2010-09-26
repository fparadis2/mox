namespace Mox.UI.Browser
{
    public class DesignTimeEditDeckPageViewModel : EditDeckPageViewModel
    {
        public DesignTimeEditDeckPageViewModel()
            : base(DesignTimeDeckLibraryViewModel.CreateLibrary(), DesignTimeCardDatabase.Instance, DesignTimeDeckLibraryViewModel.CreateDeck())
        {
        }
    }
}
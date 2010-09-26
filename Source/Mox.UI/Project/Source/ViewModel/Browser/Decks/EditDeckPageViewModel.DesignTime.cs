namespace Mox.UI.Browser
{
    public class DesignTimeBrowseDecksPageViewModel : BrowseDecksPageViewModel
    {
        public DesignTimeBrowseDecksPageViewModel()
            : base(DesignTimeDeckLibraryViewModel.CreateLibrary(), DesignTimeCardDatabase.Instance)
        {
        }
    }
}
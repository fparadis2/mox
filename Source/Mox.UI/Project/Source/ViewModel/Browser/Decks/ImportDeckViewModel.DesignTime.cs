namespace Mox.UI.Browser
{
    public class DesignTimeImportDeckViewModel : ImportDeckViewModel
    {
        public DesignTimeImportDeckViewModel()
            : base(DesignTimeCardDatabase.Instance)
        {
            Text = @"
5 Mousse
3 The breeze of the matinee";

            CanImport = false;
            Error = "This is an error";
        }
    }
}
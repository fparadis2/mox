namespace Mox.UI
{
    public interface INavigationViewModel<in TWorkspace>
    {
        void Fill(TWorkspace view);
    }
}
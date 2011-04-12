namespace Mox.UI
{
    public interface INavigationConductor
    {
        void Pop();
    }

    public interface INavigationConductor<in TViewModel> : INavigationConductor
    {
        IPageHandle Push(TViewModel viewModel);
    }
}

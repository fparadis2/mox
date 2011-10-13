using System;
using Mox.Database;
using Mox.UI.Browser;

namespace Mox.UI
{
    public static class ViewModelDataSource
    {
        #region Instance

        private static IViewModelDataSource ms_instance = new DesignTimeDataSource();

        internal static IDisposable Use(IViewModelDataSource newInstance)
        {
            var oldInstance = ms_instance;
            ms_instance = newInstance;
            return new DisposableHelper(() => ms_instance = oldInstance);
        }

        internal static IViewModelDataSource Instance
        {
            get
            {
                Throw.IfNull(ms_instance, "ViewModelDataSource instance is not set");
                return ms_instance;
            }
        }

        #endregion

        #region Methods

        public static void UseRealSource()
        {
            Use(new ProductionDataSource());
        }

        #endregion

        #region Inner Types

        private class ProductionDataSource : IViewModelDataSource
        {
            #region Variables

            private readonly CardCollectionViewModel m_cardLibraryViewModel = new CardCollectionViewModel(MasterCardDatabase.Instance.Cards, MasterCardFactory.Instance);
            private readonly DeckLibraryViewModel m_deckLibraryViewModel = new DeckLibraryViewModel(MasterDeckLibrary.Instance, DeckViewModelEditor.FromMaster());

            #endregion

            #region Properties

            public CardCollectionViewModel CardLibraryViewModel
            {
                get { return m_cardLibraryViewModel; }
            }

            public DeckLibraryViewModel DeckLibraryViewModel
            {
                get { return m_deckLibraryViewModel; }
            }

            public DeckLibrary DeckLibrary
            {
                get { return m_deckLibraryViewModel.Library; }
            }

            #endregion
        }

        private class DesignTimeDataSource : IViewModelDataSource
        {
            #region Variables

            private readonly CardCollectionViewModel m_cardLibraryViewModel = new CardCollectionViewModel_DesignTime();
            private readonly DeckLibraryViewModel m_deckLibraryViewModel = new DeckLibraryViewModel_DesignTime();

            #endregion

            #region Properties

            public CardCollectionViewModel CardLibraryViewModel
            {
                get { return m_cardLibraryViewModel; }
            }

            public DeckLibraryViewModel DeckLibraryViewModel
            {
                get { return m_deckLibraryViewModel; }
            }

            public DeckLibrary DeckLibrary
            {
                get { return m_deckLibraryViewModel.Library; }
            }

            #endregion
        }

        #endregion
    }

    internal interface IViewModelDataSource
    {
        #region Properties

        CardCollectionViewModel CardLibraryViewModel { get; }
        DeckLibraryViewModel DeckLibraryViewModel { get; }
        DeckLibrary DeckLibrary { get; }

        #endregion
    }
}

using System;
using Mox.Database;

namespace Mox.UI.Browser
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

            private readonly CardCollectionViewModel m_cardLibrary = new CardCollectionViewModel(MasterCardDatabase.Instance.Cards, MasterCardFactory.Instance);
            private readonly DeckLibraryViewModel m_deckLibrary = new DeckLibraryViewModel(MasterDeckLibrary.Instance, DeckViewModelEditor.FromMaster());

            #endregion

            #region Properties

            public CardCollectionViewModel CardLibrary
            {
                get { return m_cardLibrary; }
            }

            public DeckLibraryViewModel DeckLibrary
            {
                get { return m_deckLibrary; }
            }

            #endregion
        }

        private class DesignTimeDataSource : IViewModelDataSource
        {
            #region Variables

            private readonly CardCollectionViewModel m_cardLibrary = new CardCollectionViewModel_DesignTime();
            private readonly DeckLibraryViewModel m_deckLibrary = new DeckLibraryViewModel_DesignTime();

            #endregion

            #region Properties

            public CardCollectionViewModel CardLibrary
            {
                get { return m_cardLibrary; }
            }

            public DeckLibraryViewModel DeckLibrary
            {
                get { return m_deckLibrary; }
            }

            #endregion
        }

        #endregion
    }

    internal interface IViewModelDataSource
    {
        #region Properties

        CardCollectionViewModel CardLibrary { get; }
        DeckLibraryViewModel DeckLibrary { get; }

        #endregion
    }
}

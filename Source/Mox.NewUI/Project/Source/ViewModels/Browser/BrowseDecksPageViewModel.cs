﻿using System;

namespace Mox.UI.Browser
{
    public class BrowseDecksPageViewModel : MoxNavigationViewModel
    {
        #region Variables

        private readonly DeckListPartViewModel m_deckList;
        private readonly DeckContentPartViewModel m_deckContent;
        private readonly InfoPanelPartViewModel m_infoPanel;
        private readonly BrowseDecksCommandPartViewModel m_command;

        #endregion

        #region Constructor

        public BrowseDecksPageViewModel()
        {
            m_deckList = ActivatePart(new DeckListPartViewModel());
            m_deckContent = ActivatePart(new DeckContentPartViewModel());
            m_infoPanel = ActivatePart(new InfoPanelPartViewModel());
            m_command = ActivatePart(new BrowseDecksCommandPartViewModel());
        }

        #endregion

        #region Methods

        public override void Fill(MoxWorkspace view)
        {
            view.LeftView = m_deckList;
            view.CenterView = m_deckContent;
            view.RightView = m_infoPanel;
            view.CommandView = m_command;
        }

        #endregion
    }
}

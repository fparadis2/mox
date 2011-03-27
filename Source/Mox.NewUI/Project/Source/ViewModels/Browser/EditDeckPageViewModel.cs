using System;

namespace Mox.UI.Browser
{
    public class EditDeckPageViewModel : MoxNavigationViewModel
    {
        #region Variables

        private readonly CardListPartViewModel m_cards;
        private readonly EditDeckCommandPartViewModel m_command;

        #endregion

        #region Constructor

        public EditDeckPageViewModel()
        {
            m_cards = ActivatePart(new CardListPartViewModel());
            m_command = ActivatePart(new EditDeckCommandPartViewModel());
        }

        #endregion

        #region Methods

        public override void Fill(MoxWorkspace view)
        {
            view.LeftView = m_cards;
            view.CommandView = m_command;
        }

        #endregion
    }
}

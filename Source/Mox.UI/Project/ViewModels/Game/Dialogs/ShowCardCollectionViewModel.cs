using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.UI.Game
{
    public class ShowCardCollectionViewModel : PropertyChangedBase
    {
        public ShowCardCollectionViewModel(CardCollectionViewModel collection)
        {
            CardCollection = collection;
        }

        public CardCollectionViewModel CardCollection
        {
            get;
            private set;
        }

        private string m_title;

        public string Title
        {
            get { return m_title; }
            set
            {
                if (m_title != value)
                {
                    m_title = value;
                    NotifyOfPropertyChange();
                }
            }
        }
    }

    public class ShowCardCollectionViewModel_DesignTime : ShowCardCollectionViewModel
    {
        public ShowCardCollectionViewModel_DesignTime() 
            : base(new GraveyardCardCollectionViewModel_DesignTime())
        {
            Title = "Graveyard - Pepito";
        }
    }
}

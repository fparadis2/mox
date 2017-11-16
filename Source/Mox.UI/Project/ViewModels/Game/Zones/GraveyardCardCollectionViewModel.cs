using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mox.UI.Game
{
    public class GraveyardCardCollectionViewModel : OrderedCardCollectionViewModel
    {
        private readonly PlayerViewModel m_player;

        public GraveyardCardCollectionViewModel(PlayerViewModel player)
        {
            m_player = player;
        }

        public GameViewModel GameViewModel
        {
            get { return m_player.GameViewModel; }
        }

        public void ShowContents()
        {
            var viewModel = new ShowCardCollectionViewModel(this);
            viewModel.Title = $"Graveyard — {m_player.Name}";

            GameViewModel.DialogConductor.Push(viewModel);
        }

        public ICommand ShowContentsCommand
        {
            get
            {
                return new RelayCommand(ShowContents);
            }
        }
    }

    public class GraveyardCardCollectionViewModel_DesignTime : GraveyardCardCollectionViewModel
    {
        public GraveyardCardCollectionViewModel_DesignTime()
            : base(new PlayerViewModel_DesignTime(new GameViewModel_DesignTime_Empty()))
        {
            GameViewModel model = new GameViewModel_DesignTime_Empty();

            PlayerViewModel player = new PlayerViewModel_DesignTime(model);

            for (int i = 0; i < 20; i++)
            {
                var card = new CardViewModel_DesignTime(player);
                card.Source.Zone = card.Source.Manager.Zones.Graveyard;
                Add(card);

                if (i % 4 == 0)
                    card.InteractionType = InteractionType.Play;
            }
        }
    }
}

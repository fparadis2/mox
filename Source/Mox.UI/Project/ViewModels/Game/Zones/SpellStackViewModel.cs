using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Game
{
    public class SpellStackViewModel : PropertyChangedBase
    {
        private readonly ObservableCollection<SpellViewModel> m_spells = new BindableCollection<SpellViewModel>();

        public ObservableCollection<SpellViewModel> Spells
        {
            get { return m_spells; }
        }
    }

    public class SpellViewModel : PropertyChangedBase
    {
        #region Properties

        private CardIdentifier m_cardIdentifier;
        public CardIdentifier CardIdentifier
        {
            get { return m_cardIdentifier; }
            set
            {
                if (m_cardIdentifier != value)
                {
                    m_cardIdentifier = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(() => CardInstanceInfo);
                }
            }
        }

        public CardInstanceInfo CardInstanceInfo
        {
            get { return MasterCardDatabase.Instance.GetCardInstance(m_cardIdentifier); }
        }

        private string m_abilityText;
        public string AbilityText
        {
            get { return m_abilityText; }
            set
            {
                if (m_abilityText != value)
                {
                    m_abilityText = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(() => ShowAbilityText);
                }
            }
        }

        public bool ShowAbilityText
        {
            get { return !string.IsNullOrEmpty(m_abilityText); }
        }

        #endregion
    }

    public class SpellStackViewModel_DesignTime : SpellStackViewModel
    {
        public SpellStackViewModel_DesignTime()
        {
            CardIdentifier drossCrocodile = new CardIdentifier { Card = "Dross Crocodile" };
            Spells.Add(new SpellViewModel { CardIdentifier = drossCrocodile });

            CardIdentifier dragonEngine = new CardIdentifier { Card = "Dragon Engine" };
            Spells.Add(new SpellViewModel { CardIdentifier = dragonEngine, AbilityText = "2: Dragon Engine gets +1/+0 until end of turn." });
            Spells.Add(new SpellViewModel { CardIdentifier = dragonEngine, AbilityText = "2: Dragon Engine gets +1/+0 until end of turn." });
            Spells.Add(new SpellViewModel { CardIdentifier = dragonEngine, AbilityText = "2: Dragon Engine gets +1/+0 until end of turn." });
        }
    }
}

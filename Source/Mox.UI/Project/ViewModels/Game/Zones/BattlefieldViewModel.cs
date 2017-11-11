using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;

namespace Mox.UI.Game
{
    public class BattlefieldViewModel : CardCollectionViewModel
    {
        #region Variables

        private readonly Dictionary<CardViewModel, BattlefieldGroup> m_groupsByCard = new Dictionary<CardViewModel, BattlefieldGroup>();
        private readonly List<BattlefieldGroup> m_groups = new List<BattlefieldGroup>();

        private readonly HashSet<CardViewModel> m_dirtyCards = new HashSet<CardViewModel>();

        private bool m_invertY;

        #endregion

        #region Properties

        public bool InvertY
        {
            get { return m_invertY; }
            set
            {
                if (m_invertY != value)
                {
                    m_invertY = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Callbacks

        protected override void ClearItems()
        {
            base.ClearItems();

            m_groupsByCard.Clear();
            m_groups.Clear();
        }

        protected override void InsertItem(int index, CardViewModel item)
        {
            base.InsertItem(index, item);

            var key = new BattlefieldGroup.GroupKey(item);
            AddToGroup(item, key);
        }

        protected override void RemoveItem(int index)
        {
            CardViewModel card = this[index];

            base.RemoveItem(index);

            BattlefieldGroup group;
            if (m_groupsByCard.TryGetValue(card, out group))
            {
                RemoveFromGroup(group, card);
            }
        }

        protected override void SetItem(int index, CardViewModel item)
        {
            throw new NotImplementedException();
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        public override void OnCardChanged(CardViewModel card, PropertyChangedEventArgs e)
        {
            base.OnCardChanged(card, e);

            if (m_dirtyCards.Add(card))
                Dispatcher.CurrentDispatcher.BeginInvoke(new System.Action(UpdateCards), DispatcherPriority.Normal);
        }

        private void UpdateCards()
        {
            foreach (var dirtyCard in m_dirtyCards)
            {
                if (m_groupsByCard.TryGetValue(dirtyCard, out BattlefieldGroup currentGroup))
                {
                    var key = new BattlefieldGroup.GroupKey(dirtyCard);

                    if (key.CompareTo(currentGroup.Key) != 0)
                    {
                        RemoveFromGroup(currentGroup, dirtyCard);
                        AddToGroup(dirtyCard, key);
                    }
                }
            }

            m_dirtyCards.Clear();

            ArrangeNeeded.Raise(this, EventArgs.Empty);
        }

        #endregion

        #region Groups

        public IEnumerable<BattlefieldGroup> Groups
        {
            get { return m_groups; }
        }

        private void AddToGroup(CardViewModel card, BattlefieldGroup.GroupKey key)
        {
            var group = GetOrCreateGroup(key);
            m_groupsByCard.Add(card, group);
            group.Add(card);
        }

        private void RemoveFromGroup(BattlefieldGroup group, CardViewModel card)
        {
            m_groupsByCard.Remove(card);
            group.Remove(card);

            if (group.IsEmpty)
                RemoveGroup(group);
        }

        private BattlefieldGroup GetOrCreateGroup(BattlefieldGroup.GroupKey key)
        {
            int index = m_groups.BinarySearch(key);
            if (index >= 0)
            {
                return m_groups[index];
            }

            index = ~index;
            var group = new BattlefieldGroup(key);
            m_groups.Insert(index, group);
            return group;
        }

        private void RemoveGroup(BattlefieldGroup group)
        {
            int index = m_groups.BinarySearch(group.Key);
            Debug.Assert(index >= 0);
            m_groups.RemoveAt(index);
        }

        #endregion

        #region Events

        public event EventHandler ArrangeNeeded;

        #endregion
    }

    public class BattlefieldViewModel_DesignTime : BattlefieldViewModel
    {
        public BattlefieldViewModel_DesignTime()
        {
            var gameViewModel = new GameViewModel_DesignTime_Empty();
            gameViewModel.Source.CreatePlayer();

            Add(CreateCard(gameViewModel, Type.Creature, "Dross Crocodile"));
            Add(CreateCard(gameViewModel, Type.Creature, "Dross Crocodile"));
            Add(CreateCard(gameViewModel, Type.Creature, "Dross Crocodile"));

            var elemental = CreateCard(gameViewModel, Type.Creature, "Air Elemental");
            elemental.Source.HasSummoningSickness = false;
            elemental.IsAttacking = true;
            Add(elemental);

            Add(CreateCard(gameViewModel, Type.Artifact | Type.Creature, "Pentavus"));

            Add(CreateCard(gameViewModel, Type.Artifact, "Mox Opal"));

            var plains = CreateCard(gameViewModel, Type.Land, "Plains");
            plains.Tapped = true;
            Add(plains);
            Add(CreateCard(gameViewModel, Type.Land, "Plains"));
            Add(CreateCard(gameViewModel, Type.Land, "Plains"));

            var island = CreateCard(gameViewModel, Type.Land, "Island");
            island.Tapped = true;
            Add(island);
        }

        private CardViewModel CreateCard(GameViewModel gameViewModel, Type type, string cardName)
        {
            Mox.Game game = gameViewModel.Source;

            Card card = game.CreateCard(game.Players[0], new CardIdentifier { Card = cardName });
            card.Type = type;
            card.Zone = game.Zones.Battlefield;

            CardViewModel cardViewModel = new CardViewModel(gameViewModel);

            cardViewModel.Identifier = card.Identifier;
            cardViewModel.Source = card;

            cardViewModel.PowerAndToughness = new PowerAndToughness { Power = 10, Toughness = 3 };
            cardViewModel.InteractionType = InteractionType.Play;

            return cardViewModel;
        }
    }
}

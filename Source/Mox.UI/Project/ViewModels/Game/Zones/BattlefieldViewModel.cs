using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mox.UI.Game
{
    public class BattlefieldViewModel : CardCollectionViewModel
    {
        #region Variables

        private readonly Dictionary<CardViewModel, BattlefieldGroup> m_groupsByCard = new Dictionary<CardViewModel, BattlefieldGroup>();

        private readonly List<BattlefieldGroupKey> m_groupKeys = new List<BattlefieldGroupKey>();
        private readonly List<BattlefieldGroup> m_groups = new List<BattlefieldGroup>();

        #endregion

        #region Callbacks

        protected override void ClearItems()
        {
            base.ClearItems();

            foreach (var group in m_groups)
            {
                group.Clear();
            }

            m_groupsByCard.Clear();
            m_groupKeys.Clear();
            m_groups.Clear();
        }

        protected override void InsertItem(int index, CardViewModel item)
        {
            base.InsertItem(index, item);

            AddToGroup(item);
        }

        protected override void RemoveItem(int index)
        {
            CardViewModel card = this[index];

            base.RemoveItem(index);

            RemoveFromGroup(card);
        }

        protected override void SetItem(int index, CardViewModel item)
        {
            throw new NotImplementedException();
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Groups

        public IEnumerable<BattlefieldGroup> Groups
        {
            get { return m_groups; }
        }

        private void AddToGroup(CardViewModel card)
        {
            Debug.Assert(card.Source.AttachedTo == null, "TODO");

            var group = GetOrCreateGroup(card);

            m_groupsByCard.Add(card, group);
            group.Add(card);
        }

        private BattlefieldGroup GetOrCreateGroup(CardViewModel card)
        {
            var key = new BattlefieldGroupKey(card);

            int index = m_groupKeys.BinarySearch(key);
            if (index >= 0)
            {
                return m_groups[index];
            }

            index = ~index;
            var group = new BattlefieldGroup(key.Type);

            m_groupKeys.Insert(index, key);
            m_groups.Insert(index, group);

            return group;
        }

        private void RemoveFromGroup(CardViewModel card)
        {
            BattlefieldGroup group;
            if (m_groupsByCard.TryGetValue(card, out group))
            {
                m_groupsByCard.Remove(card);
                group.Remove(card);
            }
        }
    
        #endregion

        #region Nested

        // Order is important
        public enum PermanentType
        {
            Creature,
            Artifact,
            Land,
            Planeswalker,
            Enchantment
        }

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

            Add(CreateCard(gameViewModel, Type.Creature, "Air Elemental"));

            Add(CreateCard(gameViewModel, Type.Artifact | Type.Creature, "Pentavus"));

            Add(CreateCard(gameViewModel, Type.Artifact, "Mox Opal"));

            Add(CreateCard(gameViewModel, Type.Land, "Plains"));
            Add(CreateCard(gameViewModel, Type.Land, "Plains"));
            Add(CreateCard(gameViewModel, Type.Land, "Plains"));

            Add(CreateCard(gameViewModel, Type.Land, "Island"));
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
            cardViewModel.CanChoose = true;

            return cardViewModel;
        }
    }
}

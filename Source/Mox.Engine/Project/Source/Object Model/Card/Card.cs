// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Diagnostics;
using Mox.Rules;
using Mox.Transactions;

namespace Mox
{
    /// <summary>
    /// A card in the game.
    /// </summary>
    public partial class Card : GameObject, ITargetable, IObjectName
    {
        #region Variables

        private Zone.Id m_zoneId;
        public static readonly Property<Zone.Id> ZoneIdProperty = Property<Zone.Id>.RegisterProperty<Card>("ZoneId", c => c.m_zoneId);

        private bool m_tapped;
        public static readonly Property<bool> TappedProperty = Property<bool>.RegisterProperty<Card>("Tapped", c => c.m_tapped);

        private SuperType m_superType;
        public static readonly Property<SuperType> SuperTypeProperty = Property<SuperType>.RegisterProperty<Card>("SuperType", c => c.m_superType, PropertyFlags.Private);

        private Type m_type;
        public static readonly Property<Type> TypeProperty = Property<Type>.RegisterProperty<Card>("Type", c => c.m_type, PropertyFlags.Private);

        private SubTypes m_subTypes = SubTypes.Empty;
        public static readonly Property<SubTypes> SubTypesProperty = Property<SubTypes>.RegisterProperty<Card>("SubTypes", c => c.m_subTypes, PropertyFlags.Private);

        private Color m_color;
        public static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty<Card>("Color", c => c.m_color, PropertyFlags.Modifiable);


        private readonly Player m_owner = null;
        public static readonly Property<Player> OwnerProperty = Property<Player>.RegisterProperty<Card>("Owner", c => c.m_owner);

        private Player m_controller;
        public static readonly Property<Player> ControllerProperty = Property<Player>.RegisterProperty<Card>("Controller", c => c.m_controller, PropertyFlags.Modifiable);

        private readonly CardIdentifier m_cardIdentifier = new CardIdentifier();
        public static readonly Property<CardIdentifier> CardIdentifierProperty = Property<CardIdentifier>.RegisterProperty<Card>("CardIdentifier", c => c.m_cardIdentifier, PropertyFlags.Private);


        private Card m_attachedTo;
        public static readonly Property<Card> AttachedToProperty = Property<Card>.RegisterProperty<Card>("AttachedTo", c => c.m_attachedTo);


        private PowerAndToughness m_powerAndToughness;
        public static readonly Property<PowerAndToughness> PowerAndToughnessProperty = Property<PowerAndToughness>.RegisterProperty<Card>("PowerAndToughness", c => c.m_powerAndToughness, PropertyFlags.Private | PropertyFlags.Modifiable);

        private int m_damage;
        public static readonly Property<int> DamageProperty = Property<int>.RegisterProperty<Card>("Damage", c => c.m_damage, PropertyFlags.Private);

        private bool m_hasSummoningSickness;
        private static readonly Property<bool> HasSummoningSicknessProperty = Property<bool>.RegisterProperty<Card>("HasSummoningSickness", c => c.m_hasSummoningSickness, PropertyFlags.Private);

        private Zone m_cachedZone;
        private int m_lastIndexInZone = -1;

        #endregion

        #region Properties
        
        /// <summary>
        /// Owner of this card.
        /// </summary>
        public Player Owner
        {
            get { return m_owner; }
        }

        /// <summary>
        /// Card identifier.
        /// </summary>
        public CardIdentifier CardIdentifier
        {
            get { return m_cardIdentifier; }
        }

        /// <summary>
        /// Card name.
        /// </summary>
        public string Name
        {
            get { return m_cardIdentifier.Card; }
        }

        public Zone.Id ZoneId
        {
            get
            {
                return m_zoneId;
            }
            private set
            {
                Throw.IfNull(value, "value");
                SetValue(ZoneIdProperty, value, ref m_zoneId);
            }
        }

        /// <summary>
        /// Zone in which this card currently is.
        /// </summary>
        public Zone Zone
        {
            get
            {
                return m_cachedZone;
            }
            set
            {
                Throw.IfNull(value, "value");
                ZoneId = value.ZoneId;
            }
        }

        /// <summary>
        /// The player currently controlling the card.
        /// </summary>
        public Player Controller
        {
            get { return m_controller; }
            set 
            {
                Throw.IfNull(value, "value");
                SetValue(ControllerProperty, value, ref m_controller); 
            }
        }

        /// <summary>
        /// Whether the card is tapped.
        /// </summary>
        public bool Tapped
        {
            get { return m_tapped; }
            set { SetValue(TappedProperty, value, ref m_tapped); }
        }

        /// <summary>
        /// Super type of the card.
        /// </summary>
        public SuperType SuperType
        {
            get { return m_superType; }
            set { SetValue(SuperTypeProperty, value, ref m_superType); }
        }

        /// <summary>
        /// Type of the card.
        /// </summary>
        public Type Type
        {
            get { return m_type; }
            set { SetValue(TypeProperty, value, ref m_type); }
        }

        /// <summary>
        /// Sub types of the card.
        /// </summary>
        public SubTypes SubTypes
        {
            get { return m_subTypes; }
            set { SetValue(SubTypesProperty, value, ref m_subTypes); }
        }

        /// <summary>
        /// Color of the card.
        /// </summary>
        public Color Color
        {
            get { return m_color; }
            set { SetValue(ColorProperty, value, ref m_color); }
        }

        private PowerAndToughness PowerAndToughness
        {
            get { return m_powerAndToughness; }
            set { SetValue(PowerAndToughnessProperty, value, ref m_powerAndToughness); }
        }

        /// <summary>
        /// Power, if the card is a creature.
        /// </summary>
        public int Power
        {
            get { return PowerAndToughness.Power; }
            set 
            { 
                PowerAndToughness pw = PowerAndToughness;
                pw.Power = value;
                PowerAndToughness = pw;
            }
        }

        /// <summary>
        /// Toughness, if the card is a creature.
        /// </summary>
        public int Toughness
        {
            get { return PowerAndToughness.Toughness; }
            set
            {
                PowerAndToughness pw = PowerAndToughness;
                pw.Toughness = value;
                PowerAndToughness = pw;
            }
        }

        /// <summary>
        /// Damage assigned this turn to this creature.
        /// </summary>
        public int Damage
        {
            get { return m_damage; }
            set { SetValue(DamageProperty, value, ref m_damage); }
        }

        /// <summary>
        /// Card that this card is attached to, if any.
        /// </summary>
        public Card AttachedTo
        {
            get { return m_attachedTo; }
            internal set { SetValue(AttachedToProperty, value, ref m_attachedTo); }
        }

        public bool HasSummoningSickness
        {
            get
            {
                if (!m_hasSummoningSickness)
                    return false;

                if (SummoningSickness.IsBypassed || !Type.Is(Type.Creature) || this.HasAbility<HasteAbility>())
                {
                    return false;
                }

                return true;
            }
            set
            {
                SetValue(HasSummoningSicknessProperty, value, ref m_hasSummoningSickness);
            }
        }

        #endregion

        #region Methods

        #region Zone / Controller change

        protected override void Init()
        {
            m_cachedZone = Manager.Zones[m_zoneId];

            int position = m_lastIndexInZone;
            m_cachedZone.Add(this, ref position);

            base.Init();
        }

        protected override void Uninit()
        {
            base.Uninit();

            m_cachedZone.Remove(this, out m_lastIndexInZone);
        }

        protected override ICommand CreateSetValueCommand(PropertyBase property, object valueToSet, ISetValueAdapter adapter)
        {
            if (property == ZoneIdProperty)
            {
                return new ChangeZoneCommand(this, property, valueToSet, adapter, -1);
            }

            if (property == ControllerProperty)
            {
                return new ChangeControllerCommand(this, property, valueToSet, adapter, -1);
            }

            return base.CreateSetValueCommand(property, valueToSet, adapter);
        }

        internal void Move(Zone newZone, int position)
        {
            ICommand command = new ChangeZoneCommand(this, ZoneIdProperty, newZone.ZoneId, new NormalSetValueAdapter(), position);
            ObjectController.Execute(command);
        }

        [Serializable]
        internal class ChangeZoneCommand : SetValueCommand
        {
            #region Variables

            private readonly int m_newZonePosition;
            private readonly int m_oldZonePosition = -1;
            private readonly Resolvable<Player> m_oldController;

            #endregion

            #region Constructor

            public ChangeZoneCommand(Object obj, PropertyBase property, object newValue, ISetValueAdapter adapter, int newZonePosition)
                : base(obj, property, newValue, adapter)
            {
                Debug.Assert(obj is Card);
                Card card = (Card)obj;

                if (card.Zone != null)
                {
                    var zoneCards = card.Zone[card.Controller];
                    int oldZonePosition = zoneCards.IndexOf(card);
                    Debug.Assert(oldZonePosition != -1);
                    m_oldZonePosition = oldZonePosition < zoneCards.Count - 1 ? oldZonePosition : -1;
                }
                m_newZonePosition = newZonePosition;

                m_oldController = card.Controller;
            }

            #endregion

            #region Properties

            public override bool IsEmpty
            {
                get
                {
                    return base.IsEmpty && m_oldZonePosition == m_newZonePosition;
                }
            }

            #endregion

            #region Methods

            protected override void SetValue(Object obj, PropertyBase property, object oldBaseValue, object value, bool executing)
            {
                Debug.Assert(Property == ZoneIdProperty);

                Card card = (Card)obj;

                int position = executing ? m_newZonePosition : m_oldZonePosition;

                Zone oldZone = card.m_cachedZone;
                Zone newZone = card.Manager.Zones[(Zone.Id) value];

                ICardCollection oldCollection = null;
                ICardCollection newCollection = null;

                if (oldZone != newZone)
                {
                    oldCollection = oldZone.Remove(card, out int oldPosition);

                    HandleControllerChange(card, newZone, executing);

                    newCollection = newZone.Add(card, ref position);
                }
                else if (m_oldZonePosition != m_newZonePosition)
                {
                    oldCollection = oldZone.Remove(card, out int oldPosition);
                    newCollection = newZone.Add(card, ref position);
                }

                card.m_cachedZone = newZone;
                base.SetValue(obj, property, oldBaseValue, value, executing);

                if (newCollection != null)
                {
                    card.Manager.Zones.OnCardCollectionChanged(CardCollectionChangedEventArgs.CardMoved(oldCollection, newCollection, card, position));
                }
            }

            private void HandleControllerChange(Card card, Zone newZone, bool executing)
            {
                if (executing)
                {
                    if (newZone.IsOwned && card.Controller != card.Owner)
                    {
                        base.SetValue(card, ControllerProperty, card.Controller, card.Owner, executing);
                    }
                }
                else
                {
                    if (card.Controller.Identifier != m_oldController.Identifier)
                    {
                        var oldController = m_oldController.Resolve(card.Manager);
                        base.SetValue(card, ControllerProperty, card.Controller, oldController, false);
                    }
                }
            }

            #endregion
        }

        [Serializable]
        internal class ChangeControllerCommand : SetValueCommand
        {
            #region Variables

            private readonly int m_newZonePosition;
            private readonly int m_oldZonePosition = -1;

            #endregion

            #region Constructor

            public ChangeControllerCommand(Object obj, PropertyBase property, object newValue, ISetValueAdapter adapter, int newZonePosition)
                : base(obj, property, newValue, adapter)
            {
                Debug.Assert(obj is Card);
                Card card = (Card)obj;

                if (card.Zone != null)
                {
                    var zoneCards = card.Zone[card.Controller];
                    int oldZonePosition = zoneCards.IndexOf(card);
                    Debug.Assert(oldZonePosition != -1);
                    m_oldZonePosition = oldZonePosition < zoneCards.Count - 1 ? oldZonePosition : -1;
                }
                m_newZonePosition = newZonePosition;
            }

            #endregion

            #region Properties

            public override bool IsEmpty
            {
                get
                {
                    return base.IsEmpty && m_oldZonePosition == m_newZonePosition;
                }
            }

            #endregion

            #region Methods

            protected override void SetValue(Object obj, PropertyBase property, object oldBaseValue, object value, bool executing)
            {
                Card card = (Card)obj;
                Player oldController = card.Controller;
                Player newController = (Player)value;

                Debug.Assert(!card.m_cachedZone.IsOwned);

                int position = executing ? m_newZonePosition : m_oldZonePosition;
                card.m_cachedZone.UpdateController(card, oldController, newController, position);
                base.SetValue(obj, property, oldBaseValue, value, executing);
            }

            #endregion
        }

        #endregion

        protected override bool OnPropertyChanging(PropertyChangingEventArgs e)
        {
            if (e.Property == ZoneIdProperty)
            {
                if (!e.Cancel)
                {
                    Zone newZone = Manager.Zones[(Zone.Id)e.NewValue];

                    if (!newZone.CanAddCard(this))
                    {
                        e.Cancel = true;
                    }
                    else if (Zone != null)
                    {
                        Manager.Events.Trigger(new Events.ZoneChangeEvent(this, Zone, newZone));
                    }
                }
            }
            else if (e.Property == ControllerProperty && Zone != null)
            {
                e.Cancel = !Zone.CanChangeController(this, (Player)e.NewValue);
            }

            return base.OnPropertyChanging(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ZoneIdProperty && Manager.IsMaster)
            {
                if (ZoneId != Zone.Id.Battlefield)
                {
                    this.Attach(null);
                }
                else
                {
                    HasSummoningSickness = true;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        protected override bool ComputeHash(Hash hash)
        {
            if (m_zoneId != Zone.Id.Battlefield)
            {
                hash.Add((int)m_zoneId);
                hash.Add(Name);
                return true;
            }

            return base.ComputeHash(hash);
        }

        #endregion
    }
}

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

using Mox.Transactions;

namespace Mox
{
    /// <summary>
    /// A card in the game.
    /// </summary>
    public partial class Card : GameObject, ITargetable
    {
        #region Variables

        public static readonly Property<Zone.Id> ZoneIdProperty = Property<Zone.Id>.RegisterProperty("ZoneId", typeof(Card));
        public static readonly Property<bool> TappedProperty = Property<bool>.RegisterProperty("Tapped", typeof(Card));
        public static readonly Property<SuperType> SuperTypeProperty = Property<SuperType>.RegisterProperty("SuperType", typeof(Card), PropertyFlags.Private);
        public static readonly Property<Type> TypeProperty = Property<Type>.RegisterProperty("Type", typeof(Card), PropertyFlags.Private);
        public static readonly Property<SubTypes> SubTypesProperty = Property<SubTypes>.RegisterProperty("SubTypes", typeof(Card), PropertyFlags.Private, Mox.SubTypes.Empty);
        public static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty("Color", typeof(Card), PropertyFlags.Modifiable);
        public static readonly Property<Player> OwnerProperty = Property<Player>.RegisterProperty("Owner", typeof(Card), PropertyFlags.ReadOnly);
        public static readonly Property<Player> ControllerProperty = Property<Player>.RegisterProperty("Controller", typeof(Card),  PropertyFlags.Modifiable);
        public static readonly Property<CardIdentifier> CardIdentifierProperty = Property<CardIdentifier>.RegisterProperty("CardIdentifier", typeof(Card), PropertyFlags.ReadOnly | PropertyFlags.Private);

        public static readonly Property<Card> AttachedToProperty = Property<Card>.RegisterProperty("AttachedTo", typeof(Card));

        public static readonly Property<PowerAndToughness> PowerAndToughnessProperty = Property<PowerAndToughness>.RegisterProperty("PowerAndToughness", typeof(Card), PropertyFlags.Private | PropertyFlags.Modifiable);
        public static readonly Property<int> DamageProperty = Property<int>.RegisterProperty("Damage", typeof(Card), PropertyFlags.Private);

        #endregion

        #region Properties
        
        /// <summary>
        /// Owner of this card.
        /// </summary>
        public Player Owner
        {
            get { return GetValue(OwnerProperty); }
        }

        /// <summary>
        /// Card identifier.
        /// </summary>
        public CardIdentifier CardIdentifier
        {
            get { return GetValue(CardIdentifierProperty); }
        }

        /// <summary>
        /// Card name.
        /// </summary>
        public string Name
        {
            get { return CardIdentifier.Card; }
        }

        private Zone.Id ZoneId
        {
            get
            {
                return GetValue(ZoneIdProperty);
            }
            set
            {
                Throw.IfNull(value, "value");
                SetValue(ZoneIdProperty, value);
            }
        }

        /// <summary>
        /// Zone in which this card currently is.
        /// </summary>
        public Zone Zone
        {
            get
            {
                return Manager.Zones[ZoneId];
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
            get { return GetValue(ControllerProperty); }
            set 
            {
                Throw.IfNull(value, "value");
                SetValue(ControllerProperty, value); 
            }
        }

        /// <summary>
        /// Whether the card is tapped.
        /// </summary>
        public bool Tapped
        {
            get { return GetValue(TappedProperty); }
            set { SetValue(TappedProperty, value); }
        }

        /// <summary>
        /// Super type of the card.
        /// </summary>
        public SuperType SuperType
        {
            get { return GetValue(SuperTypeProperty); }
            set { SetValue(SuperTypeProperty, value); }
        }

        /// <summary>
        /// Type of the card.
        /// </summary>
        public Type Type
        {
            get { return GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        /// <summary>
        /// Sub types of the card.
        /// </summary>
        public SubTypes SubTypes
        {
            get { return GetValue(SubTypesProperty); }
            set { SetValue(SubTypesProperty, value); }
        }

        /// <summary>
        /// Color of the card.
        /// </summary>
        public Color Color
        {
            get { return GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        private PowerAndToughness PowerAndToughness
        {
            get { return GetValue(PowerAndToughnessProperty); }
            set { SetValue(PowerAndToughnessProperty, value); }
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
            get { return GetValue(DamageProperty); }
            set { SetValue(DamageProperty, value); }
        }

        /// <summary>
        /// Card that this card is attached to, if any.
        /// </summary>
        public Card AttachedTo
        {
            get { return GetValue(AttachedToProperty); }
            internal set { SetValue(AttachedToProperty, value); }
        }

        #endregion

        #region Methods

        #region Zone / Controller change

        protected override ICommand CreateSetValueCommand(PropertyBase property, object valueToSet, ISetValueAdapter adapter)
        {
            if ((property == ZoneIdProperty || property == ControllerProperty) && !Manager.Zones.IsUpdating)
            {
                return CreateZoneOrControllerChangeCommand(property, valueToSet, adapter, -1);
            }

            return base.CreateSetValueCommand(property, valueToSet, adapter);
        }

        private ICommand CreateZoneOrControllerChangeCommand(PropertyBase property, object valueToSet, ISetValueAdapter adapter, int position)
        {
            return new Zone.ChangeZoneOrControllerCommand(this, property, valueToSet, adapter, position);
        }

        internal void Move(Zone newZone, int position)
        {
            ICommand command = CreateZoneOrControllerChangeCommand(ZoneIdProperty, newZone.ZoneId, new NormalSetValueAdapter(), position);
            TransactionStack.PushAndExecute(command);
        }

        #endregion

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
                    Rules.SummoningSickness.SetSickness(this);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}

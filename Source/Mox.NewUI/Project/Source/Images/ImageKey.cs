using System;
using System.Linq;
using Mox.Database;

namespace Mox.UI
{
    public abstract class ImageKey
    {
        #region Variables

        private static readonly IRandom ms_random = Random.New();

        #endregion

        #region Properties

        public virtual ImageCachePolicy CachePolicy
        {
            get { return ImageCachePolicy.Recent; }
        }

        #endregion

        #region Methods

        public static ManaSymbol ForManaSymbol(Mox.ManaSymbol symbol)
        {
            return new ManaSymbol(symbol);
        }

        public static NumericalManaSymbol ForManaSymbol(int amount)
        {
            return new NumericalManaSymbol(amount);
        }

        public static MiscSymbol ForMiscSymbol(MiscSymbols symbol)
        {
            return new MiscSymbol(symbol);
        }

        public static SetSymbol ForSetSymbol(SetInfo set, Rarity rarity)
        {
            return new SetSymbol(set, rarity);
        }

        public static CardImage ForCardImage(CardInfo card, bool cropped)
        {
            // Use random card image from latest set

            var instancesBySet = from instance in card.Instances
                                 group instance by instance.Set into g
                                 orderby g.Key.ReleaseDate descending
                                 where !string.IsNullOrEmpty(g.Key.Block)
                                 select g;

            var latestGrouping = instancesBySet.FirstOrDefault();
            Throw.InvalidArgumentIf(latestGrouping == null, "Card has no instance", "card");
            return ForCardImage(ms_random.Choose(latestGrouping.ToList()), cropped);
        }

        public static CardImage ForCardImage(CardIdentifier identifier, bool cropped)
        {
            var instance = MasterCardDatabase.Instance.GetCardInstance(identifier);
            return ForCardImage(instance, cropped);
        }

        public static CardImage ForCardImage(CardInstanceInfo card, bool cropped)
        {
            return new CardImage(card, cropped);
        }

        public static CardFrameImage ForCardFrameImage(CardInstanceInfo card)
        {
            return new CardFrameImage(card);
        }

        #endregion

        #region Inner Types

        public abstract class SingleObjectImageKey : ImageKey
        {
            #region Variables

            private readonly object m_identifier;

            #endregion

            #region Constructor

            protected SingleObjectImageKey(object identifier)
            {
                m_identifier = identifier;
            }

            #endregion

            #region Properties

            protected object Identifier
            {
                get { return m_identifier; }
            }

            #endregion

            #region Methods

            public override int GetHashCode()
            {
                return m_identifier.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != GetType())
                {
                    return false;
                }
                
                return Equals(m_identifier, ((SingleObjectImageKey)obj).m_identifier);
            }

            public override string ToString()
            {
                return m_identifier.ToString();
            }

            #endregion
        }

        public sealed class ManaSymbol : SingleObjectImageKey
        {
            #region Constructor

            public ManaSymbol(Mox.ManaSymbol manaSymbol)
                : base(manaSymbol)
            {
            }

            #endregion

            #region Properties

            public Mox.ManaSymbol Symbol
            {
                get { return (Mox.ManaSymbol)Identifier; }
            }

            public override ImageCachePolicy CachePolicy
            {
                get { return ImageCachePolicy.Always; }
            }

            #endregion
        }

        public sealed class NumericalManaSymbol : SingleObjectImageKey
        {
            #region Constructor

            public NumericalManaSymbol(int amount)
                : base(amount)
            {
            }

            #endregion

            #region Properties

            public int Amount
            {
                get { return (int)Identifier; }
            }

            public override ImageCachePolicy CachePolicy
            {
                get { return ImageCachePolicy.Always; }
            }

            #endregion
        }

        public sealed class MiscSymbol : SingleObjectImageKey
        {
            #region Constructor

            public MiscSymbol(MiscSymbols symbol)
                : base(symbol)
            {
            }

            #endregion

            #region Properties

            public MiscSymbols Symbol
            {
                get { return (MiscSymbols)Identifier; }
            }

            public override ImageCachePolicy CachePolicy
            {
                get { return ImageCachePolicy.Always; }
            }

            #endregion
        }

        public sealed class SetSymbol : ImageKey
        {
            #region Variables

            private readonly SetInfo m_set;
            private readonly Rarity m_rarity;

            #endregion

            #region Constructor

            public SetSymbol(SetInfo set, Rarity rarity)
            {
                m_set = set;
                m_rarity = rarity;
            }

            #endregion

            #region Properties

            public SetInfo Set
            {
                get { return m_set; }
            }

            public Rarity Rarity
            {
                get { return m_rarity; }
            }

            public override ImageCachePolicy  CachePolicy
            {
	            get { return ImageCachePolicy.Always; }
            }

            #endregion

            #region Methods

            public override int GetHashCode()
            {
                return m_set.GetHashCode() ^ m_rarity.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals(m_set, ((SetSymbol)obj).m_set) && Equals(m_rarity, ((SetSymbol)obj).m_rarity);
            }

            public override string ToString()
            {
                return string.Format("[SetSymbol: {0} ({1})]", m_set.Name, m_rarity);
            }

            #endregion
        }

        public sealed class CardImage : ImageKey
        {
            #region Variables

            private readonly CardInstanceInfo m_card;
            private readonly bool m_cropped;

            #endregion

            #region Constructor

            public CardImage(CardInstanceInfo card, bool cropped)
            {
                m_card = card;
                m_cropped = cropped;
            }

            #endregion

            #region Properties

            public CardInstanceInfo Card
            {
                get { return m_card; }
            }

            public bool Cropped
            {
                get { return m_cropped; }
            }

            #endregion

            #region Methods

            public override int GetHashCode()
            {
                return m_card.GetHashCode() ^ m_cropped.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals(m_card, ((CardImage)obj).m_card) && Equals(m_cropped, ((CardImage)obj).m_cropped);
            }

            public override string ToString()
            {
                return string.Format("[CardImage: {0}{1}]", m_card.Card.Name, m_cropped ? " (cropped)" : string.Empty);
            }

            #endregion
        }

        public sealed class CardFrameImage : ImageKey
        {
            #region Variables

            private readonly CardInstanceInfo m_card;

            #endregion

            #region Constructor

            public CardFrameImage(CardInstanceInfo card)
            {
                m_card = card;
            }

            #endregion

            #region Properties

            public CardInstanceInfo Card
            {
                get { return m_card; }
            }

            #endregion

            #region Methods

            public override int GetHashCode()
            {
                return m_card.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals(m_card, ((CardFrameImage)obj).m_card);
            }

            public override string ToString()
            {
                return string.Format("[CardFrameImage: {0}]", m_card.Card.Name);
            }

            #endregion
        }

        #endregion
    }
}
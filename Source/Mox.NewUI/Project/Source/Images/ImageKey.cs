using System;
using Mox.Database;

namespace Mox.UI
{
    public abstract class ImageKey
    {
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

        #endregion
    }
}
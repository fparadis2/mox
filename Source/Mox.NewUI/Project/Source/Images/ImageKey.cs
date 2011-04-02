using System;

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

        public static ManaSymbol ForManaSymbol(ManaSymbol symbol)
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

        public class ManaSymbol : SingleObjectImageKey
        {
            #region Constructor

            public ManaSymbol(ManaSymbol manaSymbol)
                : base(manaSymbol)
            {
            }

            #endregion

            #region Properties

            public ManaSymbol Symbol
            {
                get { return (ManaSymbol)Identifier; }
            }

            public override ImageCachePolicy CachePolicy
            {
                get { return ImageCachePolicy.Always; }
            }

            #endregion
        }

        public class NumericalManaSymbol : SingleObjectImageKey
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

        public class MiscSymbol : SingleObjectImageKey
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

        #endregion
    }
}

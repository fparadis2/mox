using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.UI
{
    /// <summary>
    /// Other MTG symbols that can appear in UI.
    /// </summary>
    public class MiscSymbols
    {
        #region Constants

        private const string TapToken = "{T}";
        private const string UntapToken = "{U}";

        #endregion

        #region Variables

        private static readonly MiscSymbols ms_tap = new MiscSymbols(TapToken);
        private static readonly MiscSymbols ms_untap = new MiscSymbols(UntapToken);

        private readonly string m_token;

        #endregion

        #region Constructor

        private MiscSymbols(string token)
        {
            Throw.IfNull(token, "token");
            m_token = token;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tap symbol.
        /// </summary>
        public static MiscSymbols Tap
        {
            get { return ms_tap; }
        }

        /// <summary>
        /// Untap symbol.
        /// </summary>
        public static MiscSymbols Untap
        {
            get { return ms_untap; }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return m_token;
        }

        public static bool TryParse(string str, out MiscSymbols symbol)
        {
            switch (str)
            {
                case TapToken:
                    symbol = Tap;
                    return true;

                case UntapToken:
                    symbol = Untap;
                    return true;

                default:
                    symbol = default(MiscSymbols);
                    return false;
            }
        }

        #endregion
    }
}

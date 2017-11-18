using System;

namespace Mox
{
    public static class GameLog
    {
        #region Fields

        public static readonly IGameLog Empty = new EmptyGameLog();
        public static readonly IFormatProvider Formatter = new GameLogFormatter();

        #endregion

        #region Methods

        public static FormattableString ToObjectToken(IObjectName o)
        {
            return $"<{o.Identifier}|{o.Name}>";
        }

        #endregion

        #region Nested Types

        private class EmptyGameLog : IGameLog
        {
            public void Log(FormattableString message)
            {
            }
        }

        private class GameLogFormatter : IFormatProvider, ICustomFormatter
        {
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg is IObjectName o)
                    return ToObjectToken(o).ToString(formatProvider);

                if (arg is IFormattable f)
                    return f.ToString(format, formatProvider);

                return arg.ToString();
            }

            public object GetFormat(System.Type formatType)
            {
                if (formatType == typeof(ICustomFormatter))
                    return this;

                return null;
            }
        }

        #endregion
    }
}

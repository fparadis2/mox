using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI
{
    public class Symbol
    {
        public Symbol(string template, Symbol shadowSymbol)
        {
            var controlTemplate = Application.Current.TryFindResource(template) as ControlTemplate;

            var iconControl = new Control
            {
                Template = controlTemplate
            };

            Brush = new VisualBrush(iconControl);
            Ratio = TryComputeRatio(iconControl);
            ShadowSymbol = shadowSymbol;
        }

        public Brush Brush { get; }
        public float Ratio { get; }
        public Symbol ShadowSymbol { get; }

        public static implicit operator Brush(Symbol symbol)
        {
            return symbol.Brush;
        }

        private float TryComputeRatio(Control control)
        {
            var canvas = control.FindVisualChild<Canvas>();
            if (canvas != null)
            {
                return (float)(canvas.Width / canvas.Height);
            }

            return 1;
        }
    }

    public static class Symbols
    {
        private static readonly Dictionary<string, Symbol> ms_symbols = new Dictionary<string, Symbol>();

        public static Symbol ForMana(ManaSymbol symbol)
        {
            return GetSymbol("Icon_ManaSymbol_" + symbol.ToString(), "Icon_ManaSymbol_Shadow");
        }

        public static Symbol ForMana(int generic)
        {
            return GetSymbol("Icon_ManaSymbol_" + generic.ToString(), "Icon_ManaSymbol_Shadow");
        }

        public static Symbol ForMisc(MiscSymbols symbol)
        {
            if (symbol == MiscSymbols.Tap)
                return GetSymbol("Icon_Symbol_Tap", "Icon_ManaSymbol_Shadow");

            if (symbol == MiscSymbols.Untap)
                return GetSymbol("Icon_Symbol_Untap", "Icon_ManaSymbol_Shadow");

            return GetSymbolImpl("Unknown", null);
        }

        private static Symbol GetSymbol(string key, string shadowSymbol)
        {
            Symbol shadow = GetSymbolImpl(shadowSymbol, null);
            return GetSymbolImpl(key, shadow);
        }

        private static Symbol GetSymbolImpl(string key, Symbol shadowSymbol)
        {
            Symbol symbol;
            if (ms_symbols.TryGetValue(key, out symbol))
                return symbol;

            symbol = new Symbol(key, shadowSymbol);
            ms_symbols.Add(key, symbol);
            return symbol;
        }
    }
}

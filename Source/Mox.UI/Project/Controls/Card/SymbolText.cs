using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Mox.UI
{
    public static class SymbolText
    {
        private static readonly Regex ms_regex = new Regex(@"({[\w/]*?})", RegexOptions.Compiled);

        public static readonly DependencyProperty SymbolSizeProperty = DependencyProperty.RegisterAttached("SymbolSize", typeof(double), typeof(SymbolText), new PropertyMetadata(16.0, WhenSymbolTextChanged));

        public static double GetSymbolSize(TextBlock element)
        {
            return (double)element.GetValue(SymbolSizeProperty);
        }

        public static void SetSymbolSize(TextBlock element, double value)
        {
            element.SetValue(SymbolSizeProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached("FormattedText", typeof(string), typeof(SymbolText), new PropertyMetadata(default(string), WhenSymbolTextChanged));

        public static string GetFormattedText(TextBlock element)
        {
            return (string)element.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(TextBlock element, string value)
        {
            element.SetValue(FormattedTextProperty, value);
        }

        private static void WhenSymbolTextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;

            textBlock.Inlines.Clear();

            string input = textBlock.GetValue(FormattedTextProperty) as string;
            if (string.IsNullOrEmpty(input))
                return;

            var matches = ms_regex.Matches(input);
            if (matches.Count == 0)
            {
                AddText(textBlock.Inlines, input);
                return;
            }

            var size = (double)textBlock.GetValue(SymbolSizeProperty);
            Size symbolSize = new Size(size, size);

            int lastEnd = 0;
            foreach (Match match in matches)
            {
                if (match.Index > lastEnd)
                {
                    // Add text before
                    AddText(textBlock.Inlines, input.Substring(lastEnd, match.Index));
                }
                
                HandleSymbol(textBlock, match.Value, symbolSize);
                lastEnd = match.Index + match.Length;
            }

            if (lastEnd < input.Length)
            {
                AddText(textBlock.Inlines, input.Substring(lastEnd));
            }
        }

        private static void HandleSymbol(TextBlock textBlock, string symbol, Size size)
        {
            if (ManaSymbolHelper.TryParse(symbol, ManaSymbolNotation.Long, out ManaSymbol manaSymbol))
            {
                AddIcon(textBlock, GetIconResourceName(manaSymbol), size);
                return;
            }

            var icon = ParseSpecialSymbol(symbol);
            if (icon != null)
            {
                AddIcon(textBlock, icon, size);
                return;
            }

            AddText(textBlock.Inlines, symbol);
        }

        private static void AddText(InlineCollection inlines, string text)
        {
            inlines.Add(new Run { Text = text });
        }

        private static void AddIcon(TextBlock textBlock, string icon, Size size)
        {
            var iconControl = new Control
            {
                Template = textBlock.TryFindResource(icon) as ControlTemplate,
                Margin = new Thickness(1, 0, 1, 0),
                Width = size.Width,
                Height = size.Height
            };
            textBlock.Inlines.Add(new InlineUIContainer { Child = iconControl, BaselineAlignment = BaselineAlignment.Center });
        }

        public static string GetIconResourceName(ManaSymbol symbol)
        {
            return "Icon_ManaSymbol_" + symbol.ToString();
        }

        private static string ParseSpecialSymbol(string token)
        {
            // Remove {}
            token = token.Substring(1, token.Length - 2);

            if (int.TryParse(token, out int result))
                return "Icon_ManaSymbol_" + result;

            switch(token.ToUpper())
            {
                case "WUBRG": return "Icon_ColorPie_WUBRG";

                case "COMMON": return "Icon_Rarity_Common";
                case "UNCOMMON": return "Icon_Rarity_Uncommon";
                case "RARE": return "Icon_Rarity_Rare";
                case "MYTHICRARE": return "Icon_Rarity_MythicRare";
                case "SPECIAL": return "Icon_Rarity_Special";

                case "ARTIFACT": return "Icon_Type_Artifact";
                case "CREATURE": return "Icon_Type_Creature";
                case "ENCHANTMENT": return "Icon_Type_Enchantment";
                case "INSTANT": return "Icon_Type_Instant";
                case "LAND": return "Icon_Type_Land";
                case "PLANESWALKER": return "Icon_Type_Planeswalker";
                case "SORCERY": return "Icon_Type_Sorcery";

                default: return null;
            }
        }
    }
}

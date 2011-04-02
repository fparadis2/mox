using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Mox.UI
{
    public class SymbolTextBlock : TextBlock
    {
        #region Dependency properties

        public static readonly DependencyProperty UseStrictParsingProperty = DependencyProperty.Register("UseStrictParsing", typeof(bool), typeof(SymbolTextBlock), new PropertyMetadata(SymbolTextPropertyChangedCallback));

        public bool UseStrictParsing
        {
            get { return (bool)GetValue(UseStrictParsingProperty); }
            set { SetValue(UseStrictParsingProperty, value); }
        }

        public static readonly DependencyProperty SymbolTextProperty = DependencyProperty.Register("SymbolText", typeof(string), typeof(SymbolTextBlock), new PropertyMetadata(SymbolTextPropertyChangedCallback));

        public string SymbolText
        {
            get { return (string)GetValue(SymbolTextProperty); }
            set { SetValue(SymbolTextProperty, value); }
        }

        public static readonly DependencyProperty SymbolSizeProperty = DependencyProperty.Register("SymbolSize", typeof(double), typeof(SymbolTextBlock), new PropertyMetadata(12.0, SymbolTextPropertyChangedCallback));

        public double SymbolSize
        {
            get { return (double)GetValue(SymbolSizeProperty); }
            set { SetValue(SymbolSizeProperty, value); }
        }

        #endregion

        #region Helper methods

        private IEnumerable<Inline> GenerateInlinesFromText(string entryText)
        {
            ManaSymbolNotation notation = UseStrictParsing ? ManaSymbolNotation.Long : ManaSymbolNotation.Compact;
            return SymbolTextTokenizer.Tokenize(entryText, notation).Select(CreateInline);
        }

        private void OnSymbolTextPropertyChangedCallback()
        {
            Inlines.Clear();
            Inlines.AddRange(GenerateInlinesFromText(SymbolText));
        }

        private static void SymbolTextPropertyChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((SymbolTextBlock)target).OnSymbolTextPropertyChangedCallback();
        }
        
        private Inline CreateInline(object obj)
        {
            if (obj is string)
            {
                return new Run(obj.ToString());
            }

            ImageKey imageKey = CreateImageKey(obj);
            if (imageKey != null)
            {
                return new InlineUIContainer { Child = CreateImage(imageKey) };
            }

            return new Run();
        }

        private static ImageKey CreateImageKey(object obj)
        {
            if (obj is ManaSymbol)
            {
                return ImageKey.ForManaSymbol((ManaSymbol)obj);
            }

            if (obj is int)
            {
                return ImageKey.ForManaSymbol((int)obj);
            }

            if (obj is MiscSymbols)
            {
                return ImageKey.ForMiscSymbol((MiscSymbols)obj);
            }

            return null;
        }

        private Image CreateImage(ImageKey key)
        {
            var image = new Image
            {
                Height = SymbolSize
            };

            ImageService.SetKey(image, key);
            return image;
        }

        #endregion
    }
}

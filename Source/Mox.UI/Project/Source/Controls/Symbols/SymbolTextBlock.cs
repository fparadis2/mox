using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

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
        
        private static Inline CreateInline(object obj)
        {
            if (obj is string)
            {
                return new Run(obj.ToString());
            }

            const ImageSize imageSize = ImageSize.Small;

            if (obj is ManaSymbol)
            {
                var asyncImage = ImageService.GetManaSymbolImage((ManaSymbol)obj, imageSize);
                return new InlineUIContainer { Child = CreateImage(asyncImage) };
            }

            if (obj is int)
            {
                var asyncImage = ImageService.GetManaSymbolImage((int)obj, imageSize);
                return new InlineUIContainer { Child = CreateImage(asyncImage) };
            }

            if (obj is MiscSymbols)
            {
                var asyncImage = ImageService.GetMiscSymbolImage((MiscSymbols)obj, imageSize);
                return new InlineUIContainer { Child = CreateImage(asyncImage) };
            }

            return new Run();
        }

        private static Image CreateImage(IAsyncImage asyncImage)
        {
            var image = new Image
            {
                Margin = new Thickness(0, 2, 0, -2)
            };

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);

            Binding sourceBinding = new Binding("ImageSource") { Source = asyncImage };
            image.SetBinding(ImageSourceBehavior.SourceProperty, sourceBinding);
            return image;
        }

        #endregion

    }
}

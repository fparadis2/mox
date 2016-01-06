using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Mox.Database;
using Path = System.IO.Path;

namespace Mox.UI.ImageGenerator
{
    internal class CardFrameGenerator_Eight : CardFrameGenerator
    {
        #region Constants

        private const int Width = 736;
        private const int Height = 1050;

        private const string EightFolder = ImagesRootPath + @"eighth\";

#warning TODO Timeshift
        private const string FrameType = "regular";

        #endregion

        #region Methods

        protected override UIElement Generate()
        {
            Grid root = new Grid();

            CreateBackground(root);
            CreateFrame(root);
            CreatePtBox(root);

            var canvas = CreateCanvas(root);
            CreateSetSymbol(canvas);

            CreateTitle(canvas);
            CreateType(canvas);

            return root;
        }

        private Canvas CreateCanvas(Grid root)
        {
            Canvas canvas = new Canvas
            {
                Width = Width,
                Height = Height
            };

            Viewbox viewbox = new Viewbox { Child = canvas };
            root.Children.Add(viewbox);

            return canvas;
        }

        private void CreateBackground(Grid root)
        {
            var key = ImageKey.ForCardImage(Card, false);
            var background = CreateImage(key);
            root.Children.Add(background);
        }

        private void CreateFrame(Grid root)
        {
            ImageSource frame;
            var background = GetFrameImage(out frame);
            Debug.Assert(background != null);
            root.Children.Add(CreateImage(background));

            if (frame != null)
            {
                root.Children.Add(CreateImage(frame));
            }
        }

        private void CreatePtBox(Grid root)
        {
            if (Card.Card.Type.Is(Type.Creature))
            {
                var lastColor = GetLastColorName();
                var ptBox = LoadImage(Path.Combine(EightFolder, FrameType, "pt", lastColor + ".png"));
                var image = CreateImage(ptBox);
                image.VerticalAlignment = VerticalAlignment.Bottom;
                root.Children.Add(image);
            }
        }

        private void CreateSetSymbol(Canvas canvas)
        {
            var rarity = LoadImage(Path.Combine(EightFolder, "rarity", string.Format("{0}_{1}.gif", Card.Set.Identifier, GetRarityFilename(Card.Rarity).ToSymbol())));
            var image = CreateImage(rarity);

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            const double Right = 690;
            const double Top = 600;
            const double Height = 44;

            // Perserve aspect
            image.Width = rarity.Width / rarity.Height * Height;
            image.Height = Height;

            canvas.Children.Add(image);
            Canvas.SetLeft(image, Right - image.Width);
            Canvas.SetTop(image, Top);
        }

        private void CreateTitle(Canvas canvas)
        {
            TextBlock type = new TextBlock
            {
                Text = Card.Card.Name,
                FontFamily = Fonts.TitleFont,
                FontSize = 35
            };

            Viewbox viewbox = new Viewbox { Child = type, Height = 49 };
            canvas.Children.Add(viewbox);
            Canvas.SetLeft(viewbox, 45);
            Canvas.SetTop(viewbox, 40);
        }

        private void CreateType(Canvas canvas)
        {
            TextBlock title = new TextBlock
            {
                Text = Card.Card.TypeLine,
                FontFamily = Fonts.TypeFont,
                FontSize = 30
            };

            TextOptions.SetTextFormattingMode(title, TextFormattingMode.Display);

            Viewbox titleViewbox = new Viewbox { Child = title, Height = 41 };
            canvas.Children.Add(titleViewbox);
            Canvas.SetLeft(titleViewbox, 53);
            Canvas.SetTop(titleViewbox, 602);
        }

        #endregion
        
        #region Utils

        private static Rarity GetRarityFilename(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Land:
                    return Rarity.Common;
            }

            return rarity;
        }

        private string GetColorName()
        {
            if (Card.Card.Type.Is(Type.Artifact))
            {
                return "Art";
            }

            return ManaSymbolHelper.GetSymbol(Card.Card.Color).ToString();
        }

        private string GetLastColorName()
        {
            if (Card.Card.Type.Is(Type.Artifact))
            {
                return "Art";
            }

            return ManaSymbolHelper.GetSymbol(Card.Card.Color).ToString();
        }

        private ImageSource GetFrameImage(out ImageSource greyTitleAndOverlay)
        {
            greyTitleAndOverlay = null;

            var colors = GetColorName();

            if (Card.Card.Type.Is(Type.Land))
            {
                colors = AdditionalData.GetColorForLand(Card.Card.Name);

                // Grey title/type image.
			    if (colors.Length >= 2) 
                {
                    greyTitleAndOverlay = LoadImage(Path.Combine(EightFolder, FrameType, "cards", "C-overlay.png"));
			    }

                return LoadImage(Path.Combine(EightFolder, FrameType, "land", colors + ".png"));
            }

            return LoadImage(Path.Combine(EightFolder, FrameType, "cards", colors + ".png"));
        }

        #endregion
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Mox.Database;

namespace Mox.UI.ImageGenerator
{
    internal class CardFrameGenerator_Eight : CardFrameGenerator
    {
        #region Constants

        private static readonly Rect ArtBounds = new Rect(new Point(42, 104), new Point(692, 584));
        private static readonly Rect PtBounds = new Rect(new Point(0, Height - 162), new Point(Width, Height));

        private const int RarityCenterY = 621;
        private const int RarityRight = 690;
        private const int RarityHeight = 42;

        private const string EightFolder = ImagesRootPath + @"eighth\";

#warning TODO Timeshift
        private const string FrameType = "regular";

        #endregion

        #region Overrides of CardFrameGenerator

        protected override void Render()
        {
            RenderArt(ArtBounds);
            RenderBackground();
            RenderPowerToughnessBox();

            RenderRarity();
        }

        #endregion
        
        #region Methods

        private void RenderBackground()
        {
            ImageSource greyTitleAndOverlay;
            var background = GetBackgroundImage(out greyTitleAndOverlay);
            Debug.Assert(background != null);

            Context.DrawImage(background, new Rect(0, 0, Width, Height));

            if (greyTitleAndOverlay != null)
            {
                Context.DrawImage(greyTitleAndOverlay, new Rect(0, 0, Width, Height));
            }
        }

        private void RenderPowerToughnessBox()
        {
            if (Card.Card.Type.Is(Type.Creature))
            {
                var lastColor = GetLastColorName();
                var ptBox = LoadImage(Path.Combine(EightFolder, FrameType, "pt", lastColor + ".png"));
                Context.DrawImage(ptBox, PtBounds);
            }
        }

        private void RenderRarity()
        {
            var rarity = LoadImage(Path.Combine(EightFolder, "rarity", string.Format("{0}_{1}.gif", Card.Set.Identifier, Card.Rarity.ToSymbol())));

            double width = RarityHeight * rarity.Width / rarity.Height;
            Point toLeft = new Point(RarityRight - width, RarityCenterY - RarityHeight / 2);

            Context.DrawImage(rarity, new Rect(toLeft, new Size(width, RarityHeight)));
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

        private ImageSource GetBackgroundImage(out ImageSource greyTitleAndOverlay)
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
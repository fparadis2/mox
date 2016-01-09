using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.SqlServer.Server;
using Mox.Database;
using Path = System.IO.Path;

namespace Mox.UI
{
    internal class CardFrameRenderer_Eight : CardFrameRenderer
    {
        #region Constants

        private const string EightFolder = ImagesRootPath + @"eighth\";

#warning TODO Timeshift
        private const string FrameType = "regular";

        #endregion

        #region Methods

        protected override void Render()
        {
            RenderBackground();
            RenderFrame();
            RenderPtBox();

            var setLeft = RenderSetSymbol();

            RenderTitle();
            RenderType(setLeft);
            RenderAbilityText();
        }

        private void RenderBackground()
        {
            var key = ImageKey.ForCardImage(Card, false);
            Frame.RenderImage(Context, key, Bounds);
        }

        private void RenderFrame()
        {
            Rect bounds = Bounds;

            ImageSource frame;
            var background = GetFrameImage(out frame);
            Debug.Assert(background != null);
            Context.DrawImage(background, bounds);

            if (frame != null)
            {
                Context.DrawImage(frame, bounds);
            }
        }

        private void RenderPtBox()
        {
            if (Card.Card.Type.Is(Type.Creature))
            {
                var lastColor = GetLastColorName();
                var ptBox = LoadImage(Path.Combine(EightFolder, FrameType, "pt", lastColor + ".png"));

                var topLeft = Frame.ToRenderCoordinates(new Point(0, CardFrame.DefaultHeight - 162));
                var bottomRight = new Point(Frame.RenderSize.Width, Frame.RenderSize.Height);

                Context.DrawImage(ptBox, new Rect(topLeft, bottomRight));

                RenderPt();
            }
        }

        private void RenderPt()
        {
            var topLeft = Frame.ToRenderCoordinates(new Point(574, 960));
            var bottomRight = Frame.ToRenderCoordinates(new Point(688, 1010));

            var bounds = new Rect(topLeft, bottomRight);
            //Context.DrawRectangle(Brushes.Blue, null, bounds);
            DrawText(GetPowerToughnessString(), Fonts.PtFont, bounds, TextAlignment.Center);
        }

        private double RenderSetSymbol()
        {
            var rarity = LoadImage(Path.Combine(EightFolder, "rarity", string.Format("{0}_{1}.gif", Card.Set.Identifier, GetRarityFilename(Card.Rarity).ToSymbol())));

            var topLeft = Frame.ToRenderCoordinates(new Point(0, 600));
            var bottomRight = Frame.ToRenderCoordinates(new Point(688, 642));

            // Perserve aspect
            var width = (bottomRight.Y - topLeft.Y) * rarity.Width / rarity.Height;
            topLeft.X = bottomRight.X - width;

            Context.DrawImage(rarity, new Rect(topLeft, bottomRight));

            return topLeft.X;
        }

        private void RenderTitle()
        {
            var titleTopLeft = Frame.ToRenderCoordinates(new Point(45, 40));
            var titleBottomRight = Frame.ToRenderCoordinates(new Point(696, 88));
            var titleBounds = new Rect(titleTopLeft, titleBottomRight);

            DrawText(Card.Card.Name, Fonts.TitleFont, titleBounds);
        }

        private void RenderType(double right)
        {
            var titleTopLeft = Frame.ToRenderCoordinates(new Point(53, 603));
            var titleBottomRight = Frame.ToRenderCoordinates(new Point(0, 643));
            titleBottomRight.X = right;

            var titleBounds = new Rect(titleTopLeft, titleBottomRight);

            DrawText(Card.Card.TypeLine, Fonts.TypeFont, titleBounds);
        }

        private void RenderAbilityText()
        {
            Point topLeft = Frame.ToRenderCoordinates(new Point(54, 669));
            Point bottomRight = Frame.ToRenderCoordinates(new Point(682, 957));

            Rect bounds = new Rect(topLeft, bottomRight);

            Context.DrawRectangle(Brushes.Blue, null, bounds);

            Frame.AbilityTextRenderer.Render(Context, topLeft);
        }

        #endregion
        
        #region Utils

        private void DrawText(string text, FontFamily font, Rect bounds, TextAlignment alignment = TextAlignment.Left)
        {
            var typeface = new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var formattedText = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, bounds.Height, Brushes.Black);

            formattedText.TextAlignment = alignment;

            Context.DrawText(formattedText, GetTextOrigin(ref bounds, alignment));
        }

        private static Point GetTextOrigin(ref Rect bounds, TextAlignment alignment)
        {
            switch (alignment)
            {
                case TextAlignment.Center:
                    return new Point(bounds.Left + bounds.Width / 2, bounds.Top);

                default:
                    return bounds.TopLeft;
            }
        }

        private string GetPowerToughnessString()
        {
            return string.Format("{0}/{1}", Card.Card.PowerString, Card.Card.ToughnessString);
        }

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
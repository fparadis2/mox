using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
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

        #region Variables

        private readonly SymbolTextRenderer m_titleText;
        private readonly SymbolTextRenderer m_typeText;
        private readonly SymbolTextRenderer m_abilityText;
        private readonly SymbolTextRenderer m_ptText;

        #endregion

        #region Constructor

        public CardFrameRenderer_Eight(CardFrame frame, CardInstanceInfo card)
            : base(frame, card)
        {
            m_titleText = CreateTitleText(card);
            m_typeText = CreateTypeText(card);
            m_abilityText = CreateAbilityText(card);
            m_ptText = CreatePtText(card);
        }

        private SymbolTextRenderer CreateTitleText(CardInstanceInfo card)
        {
            return SymbolTextRenderer.Create(card.Card.Name, Fonts.TitleFont, TitleHeight);
        }

        private SymbolTextRenderer CreateTypeText(CardInstanceInfo card)
        {
            return SymbolTextRenderer.Create(card.Card.TypeLine, Fonts.TypeFont, TypeHeight);
        }

        private SymbolTextRenderer CreateAbilityText(CardInstanceInfo card)
        {
            List<TextTokenizer.Token> tokens = new List<TextTokenizer.Token>();
            TextTokenizer.Tokenize(card.Card.Text, tokens);

            var typeface = new Typeface(Fonts.AbilityTextFont, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var size = 15;
            var maxSize = new Size(500, 500);
            return new SymbolTextRenderer(card.Card.Text, tokens, maxSize, typeface, size);
        }

        private SymbolTextRenderer CreatePtText(CardInstanceInfo card)
        {
            var renderer = SymbolTextRenderer.Create(GetPowerToughnessString(card.Card), Fonts.PtFont, PtHeight);
            renderer.TextAlignment = TextAlignment.Center;
            return renderer;
        }

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
            RenderImage(key, Bounds);
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

                var topLeft = ToRenderCoordinates(new Point(0, CardFrame.DefaultHeight - 162));
                var bottomRight = new Point(RenderSize.Width, RenderSize.Height);
                Context.DrawImage(ptBox, new Rect(topLeft, bottomRight));

                RenderPt();
            }
        }

        private const double PtLeft = 574;
        private const double PtTop = 960;
        private const double PtWidth = 114;
        private const double PtHeight = 50;

        private void RenderPt()
        {
            var bounds = new Rect(new Point(PtLeft, PtTop), new Size(PtWidth, PtHeight));
            m_ptText.Render(Context, bounds, RenderRatio);
        }

        private double RenderSetSymbol()
        {
            var rarity = LoadImage(Path.Combine(EightFolder, "rarity", string.Format("{0}_{1}.gif", Card.Set.Identifier, GetRarityFilename(Card.Rarity).ToSymbol())));

            var topLeft = ToRenderCoordinates(new Point(0, 600));
            var bottomRight = ToRenderCoordinates(new Point(688, 642));

            // Perserve aspect
            var width = (bottomRight.Y - topLeft.Y) * rarity.Width / rarity.Height;
            topLeft.X = bottomRight.X - width;

            Context.DrawImage(rarity, new Rect(topLeft, bottomRight));

            return topLeft.X;
        }

        private const double TitleLeft = 45;
        private const double TitleTop = 40;
        private const double TitleHeight = 48;

        private void RenderTitle()
        {
            var titleTopLeft = ToRenderCoordinates(new Point(TitleLeft, TitleTop));
            m_titleText.Render(Context, titleTopLeft, RenderRatio);
        }

        private const double TypeLeft = 53;
        private const double TypeTop = 603;
        private const double TypeHeight = 40;

        private void RenderType(double right)
        {
            var titleTopLeft = ToRenderCoordinates(new Point(TypeLeft, TypeTop));
            m_typeText.Render(Context, titleTopLeft, RenderRatio);
        }

        private void RenderAbilityText()
        {
            Point topLeft = ToRenderCoordinates(new Point(54, 669));
            m_abilityText.Render(Context, topLeft);
        }

        #endregion
        
        #region Utils

        private static string GetPowerToughnessString(CardInfo card)
        {
            return string.Format("{0}/{1}", card.PowerString, card.ToughnessString);
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
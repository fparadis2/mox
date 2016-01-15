using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
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

        private readonly SymbolTextRenderer m_manaCostText;
        private readonly SymbolTextRenderer m_titleText;
        private readonly SymbolTextRenderer m_typeText;
        private readonly SymbolTextRenderer m_ptText;
        private readonly SymbolTextRenderer m_abilityText;

        #endregion

        #region Constructor

        public CardFrameRenderer_Eight(CardFrame frame, CardInstanceInfo card)
            : base(frame, card)
        {
            m_manaCostText = CreateManaCostText(card);
            m_titleText = CreateTitleText(card);
            m_typeText = CreateTypeText(card);
            m_ptText = CreatePtText(card);
            m_abilityText = CreateAbilityAndFlavorText(card);
        }

        private SymbolTextRenderer CreateManaCostText(CardInstanceInfo card)
        {
            var layout = new SymbolTextLayout(card.Card.ManaCost) { Font = Fonts.TitleFont, FontSize = ManaCostHeight };
            return new SymbolTextRenderer(layout)
            {
                TextAlignment = TextAlignment.Right,
                RenderSymbolShadows = true
            };
        }

        private SymbolTextRenderer CreateTitleText(CardInstanceInfo card)
        {
            var layout = new SymbolTextLayout(card.Card.Name) { Font = Fonts.TitleFont, FontSize = TitleHeight };
            return new SymbolTextRenderer(layout);
        }

        private SymbolTextRenderer CreateTypeText(CardInstanceInfo card)
        {
            var layout = new SymbolTextLayout(card.Card.TypeLine) { Font = Fonts.TypeFont, FontSize = TypeHeight };
            return new SymbolTextRenderer(layout);
        }

        private SymbolTextRenderer CreateAbilityAndFlavorText(CardInstanceInfo card)
        {
            string abilityText = card.Card.Text;

            if (!string.IsNullOrEmpty(card.Card.Flavor))
            {
                abilityText += '\n';
                abilityText += card.Card.Flavor;
            }

            var maxSize = new Size(AbilityWidth, AbilityHeight);
            var layout = new SymbolTextLayout(abilityText)
            {
                Font = Fonts.AbilityTextFont, 
                FontSize = MaxAbilityFontSize, 
                MaxSize = maxSize, 
                ItalicizeParenthesis = true
            };

            var renderer = new SymbolTextRenderer(layout) { VerticalAlignment = VerticalAlignment.Center };

            if (layout.LineCount <= 1)
            {
                renderer.TextAlignment = TextAlignment.Center;
            }

            return renderer;
        }

        private SymbolTextRenderer CreatePtText(CardInstanceInfo card)
        {
            if (!HasPtBox)
                return null;

            var layout = new SymbolTextLayout(GetPowerToughnessString(card.Card)) { Font = Fonts.PtFont, FontSize = PtHeight };
            return new SymbolTextRenderer(layout) { TextAlignment = TextAlignment.Center };
        }

        #endregion

        #region Methods

        protected override void Render()
        {
            RenderBackground();
            RenderFrame();
            RenderPtBox();

            var setLeft = RenderSetSymbol();

            RenderManaCost();
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

        private bool HasPtBox
        {
            get { return Card.Card.Type.Is(Type.Creature); }
        }

        private void RenderPtBox()
        {
            if (HasPtBox)
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

            var topLeft = ToRenderCoordinates(new Point(0, 602));
            var bottomRight = ToRenderCoordinates(new Point(685, 642));

            // Perserve aspect
            var width = (bottomRight.Y - topLeft.Y) * rarity.Width / rarity.Height;
            topLeft.X = bottomRight.X - width;

            Context.DrawImage(rarity, new Rect(topLeft, bottomRight));

            return topLeft.X;
        }

        private const double ManaCostLeft = 45;
        private const double ManaCostTop = 50;
        private const double ManaCostRight = 690;
        private const double ManaCostHeight = 34;

        private void RenderManaCost()
        {
            var bounds = new Rect(new Point(ManaCostLeft, ManaCostTop), new Size(ManaCostRight - ManaCostLeft, ManaCostHeight));
            m_manaCostText.Render(Context, bounds, RenderRatio);
        }

        private const double TitleLeft = 45;
        private const double TitleTop = 43;
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

        private const double MaxAbilityFontSize = 39.5;
        private const double AbilityLeft = 56;
        private const double AbilityWidth = 626;
        private const double AbilityTop = 669;
        private const double AbilityHeight = 288;

        private void RenderAbilityText()
        {
            Rect bounds = new Rect(AbilityLeft, AbilityTop, AbilityWidth, AbilityHeight);
            m_abilityText.Render(Context, bounds, RenderRatio);
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
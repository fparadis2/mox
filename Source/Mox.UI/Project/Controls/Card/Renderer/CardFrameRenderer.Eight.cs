using System;
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

        private static readonly ColoredImageCache ms_cardFramesCache;
        private static readonly ColoredImageCache ms_cardBordersCache;
        private static readonly ColoredImageCache ms_landFramesCache;
        private static readonly ColoredImageCache ms_landSymbolsCache;

        static CardFrameRenderer_Eight()
        {
            ms_cardFramesCache = new ColoredImageCache(Path.Combine(EightFolder, FrameType, "cards"), "{0}.png");
            ms_cardBordersCache = new ColoredImageCache(Path.Combine(EightFolder, FrameType, "borders"), "{0}.png");
            ms_landFramesCache = new ColoredImageCache(Path.Combine(EightFolder, FrameType, "land"), "{0}.png");
            ms_landSymbolsCache = new ColoredImageCache(Path.Combine(ImagesRootPath, "symbols/land/"), "{0}.png");
        }

        #endregion

        #region Variables

        private readonly ImageSource m_frame;
        private readonly ImageSource m_frameOverlay;
        private readonly ImageSource m_ptImage;
        private readonly ImageSource m_setImage;

        private readonly SymbolTextRenderer m_manaCostText;
        private readonly SymbolTextRenderer m_titleText;
        private readonly SymbolTextRenderer m_typeText;
        private readonly SymbolTextRenderer m_ptText;

        private ImageSource m_brushImage;
        private readonly SymbolTextRenderer m_artistText;

        private AbilityTextRenderer m_abilityText;
        private ImageSource m_abilitySymbol;

        #endregion

        #region Constructor

        public CardFrameRenderer_Eight(CardFrame frame, CardInstanceInfo card)
            : base(frame, card)
        {
            string frameColors;
            m_frame = GetFrameImage(out m_frameOverlay, out frameColors);
            m_ptImage = GetPtImage(frameColors);
            m_setImage = GetSetImage();

            m_manaCostText = CreateManaCostText(card);
            m_titleText = CreateTitleText(card);
            m_typeText = CreateTypeText(card);
            m_ptText = CreatePtText(card);

            m_artistText = CreateArtistText(card);

            m_abilitySymbol = CreateAbilitySymbol();

            m_abilityText = new AbilityTextRenderer();
            m_abilityText.Initialize(card, HasPtBox);
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

        private SymbolTextRenderer CreatePtText(CardInstanceInfo card)
        {
            if (!HasPtBox)
                return null;

            var layout = new SymbolTextLayout(GetPowerToughnessString(card.Card)) { Font = Fonts.PtFont, FontSize = PtHeight };
            return new SymbolTextRenderer(layout) { TextAlignment = TextAlignment.Center };
        }

        private SymbolTextRenderer CreateArtistText(CardInstanceInfo card)
        {
            MiscSymbols brush = MiscSymbols.BlackBrush;
            Brush forecolor = Brushes.Black;

            if (HasWhiteFooter())
            {
                brush = MiscSymbols.WhiteBrush;
                forecolor = Brushes.White;
            }

            m_brushImage = ImageService.LoadImageSynchronous(ImageKey.ForMiscSymbol(brush));
            string artistText = card.Artist;

            var layout = new SymbolTextLayout(artistText) { Font = Fonts.ArtistFont, FontSize = ArtistHeight };
            return new SymbolTextRenderer(layout) { Brush = forecolor };
        }

        private ImageSource CreateAbilitySymbol()
        {
            if (IsBasicLand())
            {
                return LoadImage(ms_landSymbolsCache.Get(Card.Card.Color));
            }

            return null;
        }

        #endregion

        #region Methods

        protected override void Render()
        {
            RenderBackground();
            RenderFrame();
            RenderPtBox();
            RenderArtist();

            var setLeft = RenderSetSymbol();

            RenderManaCost();
            RenderTitle();
            RenderType(setLeft);
            RenderAbilityText();
        }

        private void RenderBackground()
        {
            var key = ImageKey.ForCardImage(Card, false);
            if (!RenderImage(key, Bounds))
            {
                Context.DrawRectangle(Brushes.Black, null, Bounds);
            }
        }

        private void RenderFrame()
        {
            Rect bounds = Bounds;

            Context.DrawImage(m_frame, bounds);

            if (m_frameOverlay != null)
            {
                Context.DrawImage(m_frameOverlay, bounds);
            }
        }

        private bool HasPtBox
        {
            get { return Card.Card.Type.Is(Type.Creature); }
        }

        private const double PtLeft = 574;
        private const double PtTop = 966;
        private const double PtWidth = 114;
        private const double PtHeight = 50;

        private void RenderPtBox()
        {
            if (m_ptImage != null)
            {
                var topLeft = ToRenderCoordinates(new Point(0, CardFrame.DefaultHeight - 162));
                var bottomRight = new Point(RenderSize.Width, RenderSize.Height);
                Context.DrawImage(m_ptImage, new Rect(topLeft, bottomRight));

                var bounds = new Rect(new Point(PtLeft, PtTop), new Size(PtWidth, PtHeight));
                m_ptText.Render(Context, bounds, RenderRatio);
            }
        }

        private const double ArtistLeft = 39;
        private const double ArtistTop = 982;
        private const double ArtistWidth = 10000; // Dummy
        private const double ArtistHeight = 28;

        private const double ArtistBrushTop = ArtistTop + 10;
        private const double ArtistBrushHeight = 13;

        private void RenderArtist()
        {
            var position = new Point(ArtistLeft, ArtistTop);
            double brushWidth = ArtistBrushHeight * m_brushImage.Width / m_brushImage.Height;

            Rect brushBounds = new Rect(ArtistLeft, ArtistBrushTop, brushWidth, ArtistBrushHeight);
            Context.DrawImage(m_brushImage, ToRenderCoordinates(brushBounds));

            position.X += brushWidth + 2;
            var bounds = new Rect(position, new Size(ArtistWidth, ArtistHeight));
            m_artistText.Render(Context, bounds, RenderRatio);
        }

        private double RenderSetSymbol()
        {
            var topLeft = ToRenderCoordinates(new Point(0, 602));
            var bottomRight = ToRenderCoordinates(new Point(685, 642));

            if (m_setImage == null)
                return bottomRight.X;

            // Perserve aspect
            var width = (bottomRight.Y - topLeft.Y) * m_setImage.Width / m_setImage.Height;
            topLeft.X = bottomRight.X - width;

            Context.DrawImage(m_setImage, new Rect(topLeft, bottomRight));

            return topLeft.X;
        }

        private const double ManaCostLeft = 45;
        private const double ManaCostTop = 42;
        private const double ManaCostRight = 690;
        private const double ManaCostHeight = 48;

        private void RenderManaCost()
        {
            var bounds = new Rect(new Point(ManaCostLeft, ManaCostTop), new Size(ManaCostRight - ManaCostLeft, ManaCostHeight));
            m_manaCostText.Render(Context, bounds, RenderRatio);
        }

        private const double TitleLeft = 45;
        private const double TitleTop = 42;
        private const double TitleHeight = 48;

        private void RenderTitle()
        {
            var bounds = new Rect(TitleLeft, TitleTop, 10000, TitleHeight);
            m_titleText.Render(Context, bounds, RenderRatio);
        }

        private const double TypeLeft = 53;
        private const double TypeTop = 603;
        private const double TypeHeight = 40;

        private void RenderType(double right)
        {
            var bounds = new Rect(TypeLeft, TypeTop, 10000, TypeHeight);
            m_typeText.Render(Context, bounds, RenderRatio);
        }

        private const double MaxAbilityFontSize = 39.5;
        private const double AbilityLeft = 56;
        private const double AbilityWidth = 628;
        private const double AbilityTop = 669;
        private const double AbilityHeight = 288;
        private const double AbilityPtAdjust = 14;

        private void RenderAbilityText()
        {
            if (m_abilitySymbol != null)
            {
                // Perserve aspect
                double width = AbilityHeight * m_abilitySymbol.Width / m_abilitySymbol.Height;

                Point position = new Point(AbilityLeft, AbilityTop);
                position.X += (AbilityWidth - width) / 2;

                Rect bounds = new Rect(position, new Size(width, AbilityHeight));
                bounds = ToRenderCoordinates(bounds);
                Context.DrawImage(m_abilitySymbol, bounds);
            }
            else
            {
                m_abilityText.Render(Context, RenderRatio);
            }
        }

        #endregion
        
        #region Utils

        private static string GetPowerToughnessString(CardInfo card)
        {
            return string.Format("{0}/{1}", card.PowerString, card.ToughnessString);
        }

        private ImageSource GetFrameImage(out ImageSource overlay, out string frameColors)
        {
            overlay = null;

            var color = Card.Card.Color;

            if (Card.Card.Type.Is(Type.Land))
            {
                if (AdditionalData.TryGetColorStringForLand(Card.Card.Name, out frameColors))
                {
                    if (frameColors.Length > 2)
                    {
                        frameColors = "A";
                    }

                    if (frameColors.Length >= 2)
                    {
                        overlay = LoadImage(Path.Combine(EightFolder, FrameType, "cards", "C-overlay.png"));
                    }

                    return LoadImage(ms_landFramesCache.FormatPath(frameColors));
                }

                string frameImage = ms_landFramesCache.Get(color, out frameColors);

                if (color.HasMoreThanOneColor())
                {
                    overlay = LoadImage(Path.Combine(EightFolder, FrameType, "cards", "C-overlay.png"));
			    }

                return LoadImage(frameImage);
            }

            bool hasMixedManaCosts = HasMixedManaCost();
            int numColors = color.CountColors();

            if (numColors > 2)
            {
                // Gold frame + gold borders
                frameColors = "Gld";
                return LoadImage(Path.Combine(EightFolder, FrameType, "cards", "Gld.png"));
            }

            if (numColors == 2)
            {
                if (!hasMixedManaCosts)
                {
                    // Dual mana
                    overlay = LoadImage(Path.Combine(EightFolder, FrameType, "cards", "C-overlay.png"));
                    return LoadImage(ms_cardFramesCache.Get(color, out frameColors));
                }

                // Gold frame + multicolored borders
                string dummy;
                overlay = LoadImage(ms_cardBordersCache.Get(color, out dummy));

                frameColors = "Gld";
                return LoadImage(Path.Combine(EightFolder, FrameType, "cards", "Gld.png"));
            }

            if (Card.Card.Type.Is(Type.Artifact))
            {
                frameColors = string.Empty;
                return LoadImage(Path.Combine(EightFolder, FrameType, "cards", "Art.png"));
            }

            return LoadImage(ms_cardFramesCache.Get(color, out frameColors));
        }

        private bool HasMixedManaCost()
        {
            ManaCost cost = ManaCost.Parse(Card.Card.ManaCost);

            ManaSymbol? lastColoredSymbol = null;
            foreach (var symbol in cost.Symbols)
            {
                if (!ManaSymbolHelper.IsColored(symbol))
                    continue;

                if (lastColoredSymbol == null)
                {
                    lastColoredSymbol = symbol;
                    continue;
                }

                if (lastColoredSymbol != symbol)
                    return true;
            }

            return false;
        }

        private ImageSource GetPtImage(string frameColors)
        {
            if (!HasPtBox)
                return null;

            switch (frameColors)
            {
                case "Art":
                case "Gld":
                    return LoadImage(Path.Combine(EightFolder, FrameType, "pt", frameColors + ".png"));
            }

            var lastColor = frameColors.Length == 0 ? "C" : frameColors[frameColors.Length - 1].ToString(CultureInfo.InvariantCulture);
            return LoadImage(Path.Combine(EightFolder, FrameType, "pt", lastColor + ".png"));
        }

        private ImageSource GetSetImage()
        {
            return ImageService.LoadImageSynchronous(ImageKey.ForSetSymbol(Card.Set, Card.Rarity));
        }

        private bool IsBasicLand()
        {
            var card = Card.Card;
            return card.Type.Is(Type.Land) && card.SuperType.Is(SuperType.Basic) && string.IsNullOrEmpty(card.Text);
        }

        private bool HasWhiteFooter()
        {
            if (Card.Card.Type.Is(Type.Land))
                return true;

            var color = Card.Card.Color;

            // Hard-coded list of frames with black bottom-left corners
            switch (color)
            {
                case Color.Black:

                case Color.Black | Color.Green:
                case Color.Black | Color.Red:

                case Color.Green | Color.Blue | Color.Black:
                case Color.White | Color.Blue | Color.Black:
                    return true;
            }

            return false;
        }

        #endregion

        #region Inner Types

        private struct AbilityTextRenderer
        {
            private SymbolTextRenderer m_abilityText;
            private SymbolTextRenderer m_flavorText;
            private double m_newLineHeight;

            public void Initialize(CardInstanceInfo card, bool hasPtBox)
            {
                var availableSize = new Size(AbilityWidth, AbilityHeight);

                if (hasPtBox)
                    availableSize.Height -= AbilityPtAdjust;

                SymbolTextLayout abilityLayout = new SymbolTextLayout(card.Card.Text)
                {
                    Font = Fonts.AbilityTextFont,
                    ItalicizeParenthesis = true,
                    MaxSize = availableSize
                };

                SymbolTextLayout flavorLayout = null;

                if (!string.IsNullOrEmpty(card.Flavor))
                {
                    flavorLayout = new SymbolTextLayout(card.Flavor)
                    {
                        Typeface = new Typeface(Fonts.AbilityTextFont, FontStyles.Italic, FontWeights.Normal, FontStretches.Normal),
                        NewLineRatio = 1,
                        MaxSize = availableSize
                    };
                }

                FindMaxSize(availableSize, abilityLayout, flavorLayout, out m_newLineHeight);

                m_abilityText = new SymbolTextRenderer(abilityLayout);

                if (flavorLayout != null)
                {
                    m_flavorText = new SymbolTextRenderer(flavorLayout);
                }
                else
                {
                    if (abilityLayout.LineCount <= 1)
                    {
                        m_abilityText.TextAlignment = TextAlignment.Center;
                    }
                }
            }

            private void FindMaxSize(Size availableSize, SymbolTextLayout abilityLayout, SymbolTextLayout flavorLayout, out double newLineHeight)
            {
                double fontSize = MaxAbilityFontSize;

                while (true)
                {
                    var height = MeasureHeight(fontSize, abilityLayout, flavorLayout, out newLineHeight);

                    if (height <= availableSize.Height)
                        return;

                    fontSize *= Math.Sqrt(availableSize.Height / height) * 0.9;
                }
            }

            private double MeasureHeight(double fontSize, SymbolTextLayout abilityLayout, SymbolTextLayout flavorLayout, out double newLineHeight)
            {
                abilityLayout.FontSize = fontSize;
                double height = abilityLayout.Height;
                newLineHeight = abilityLayout.LineHeight * 0.55;

                if (flavorLayout != null)
                {
                    flavorLayout.FontSize = fontSize;
                    height += newLineHeight;
                    height += flavorLayout.Height;
                }

                return height;
            }

            public void Render(DrawingContext context, double renderRatio)
            {
                double totalHeight = m_abilityText.TotalHeight;

                if (m_flavorText != null)
                {
                    totalHeight += m_newLineHeight;
                    totalHeight += m_flavorText.TotalHeight;
                }

                double y = AbilityTop + (AbilityHeight - totalHeight) * 0.5;

                Rect bounds = new Rect(AbilityLeft, y, AbilityWidth, totalHeight);
                m_abilityText.Render(context, bounds, renderRatio);

                if (m_flavorText != null)
                {
                    bounds.Y += m_newLineHeight;
                    bounds.Y += m_abilityText.TotalHeight;

                    m_flavorText.Render(context, bounds, renderRatio);
                }
            }
        }

        #endregion
    }
}
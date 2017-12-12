using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Mox
{
    public class SetSymbolsGenerator
    {
        public static void GenerateSymbols()
        {
            SetSymbolsGenerator generator = new SetSymbolsGenerator();
            generator.Generate();
        }

        private const string Url = "https://github.com/andrewgioia/Keyrune/raw/master/fonts/keyrune.svg";
        private const string FileName = "keyrune.svg";

        private const string OutputFileName = "../../../Source/Mox.UI/Project/Themes/Icons.Sets.xaml";
        
        private readonly string m_downloadFileTarget;
        private readonly string m_destinationFile;

        private readonly XmlNamespaceManager m_xmlNamespaceManager;

        public SetSymbolsGenerator()
        {
            m_downloadFileTarget = Path.Combine(Path.GetTempPath(), FileName);

            string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            m_destinationFile = Path.Combine(exePath, OutputFileName);

            m_xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
            m_xmlNamespaceManager.AddNamespace("svg", "http://www.w3.org/2000/svg");
        }

        public void Generate()
        {
            DownloadSourceFile(m_downloadFileTarget);
            var symbols = ReadSourceFile(m_downloadFileTarget);
            GenerateFile(symbols, m_destinationFile);
        }

        private void DownloadSourceFile(string downloadFileTarget)
        {
            Console.WriteLine("Checking to see if we need to download {0}", FileName);

            if (!DownloadHelper.Download(Url, downloadFileTarget))
            {
                Console.WriteLine("{0} is up-to-date", FileName);
                return;
            }

            Console.WriteLine("Downloaded {0} successfully", FileName);
        }

        private Symbols ReadSourceFile(string file)
        {
            Symbols symbols = new Symbols();

            XmlDocument document = new XmlDocument();
            document.Load(file);

            var font = document.SelectSingleNode("//svg:font", m_xmlNamespaceManager);
            symbols.Width = GetAttributeValue(font, "horiz-adv-x", 1024);

            var fontface = font.SelectSingleNode("svg:font-face", m_xmlNamespaceManager);
            var ascent = GetAttributeValue(fontface, "ascent", 960);
            var descent = GetAttributeValue(fontface, "descent", -64);
            symbols.Ascent = ascent;
            symbols.Height = ascent - descent;

            foreach (XmlNode glyphNode in font.SelectNodes("svg:glyph", m_xmlNamespaceManager))
            {
                if (TryReadGlyph(glyphNode, out Glyph glyph))
                    symbols.Glyphs.Add(glyph);
            }

            return symbols;
        }

        private bool TryReadGlyph(XmlNode glyphNode, out Glyph glyph)
        {
            glyph = new Glyph();

            glyph.Set = GetAttributeValue(glyphNode, "glyph-name", null);
            if (string.IsNullOrEmpty(glyph.Set))
                return false;

            glyph.Data = GetAttributeValue(glyphNode, "d", null);
            if (string.IsNullOrEmpty(glyph.Data))
                return false;

            glyph.Set = glyph.Set.ToUpper();
            glyph.Width = GetAttributeValue(glyphNode, "horiz-adv-x", 0);
            return true;
        }

        private static string GetAttributeValue(XmlNode node, string attribute, string defaultValue)
        {
            var attr = node.Attributes[attribute];
            if (attr != null)
            {
                return attr.Value;
            }

            return defaultValue;
        }

        private static int GetAttributeValue(XmlNode node, string attribute, int defaultValue)
        {
            var attr = node.Attributes[attribute];
            if (attr != null)
            {
                if (int.TryParse(attr.Value, out int result))
                    return result;
            }

            return defaultValue;
        }

        private void GenerateFile(Symbols symbols, string file)
        {
            XmlDocument document = new XmlDocument();

            XmlElement resourceDictionary = AddChild(document, document, "ResourceDictionary");
            resourceDictionary.SetAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            resourceDictionary.SetAttribute("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml");

            foreach (var symbol in symbols.Glyphs)
            {
                GenerateSymbol(document, resourceDictionary, symbols, symbol);
            }

            using (var stream = File.Create(file))
                document.Save(stream);
        }

        private void GenerateSymbol(XmlDocument document, XmlElement resourceDictionary, Symbols symbols, Glyph symbol)
        {
            XmlElement controlTemplate = AddChild(document, resourceDictionary, "ControlTemplate");
            controlTemplate.SetAttribute("Key", "http://schemas.microsoft.com/winfx/2006/xaml", symbol.ToIconName());
            controlTemplate.SetAttribute("TargetType", "Control");

            XmlElement viewBox = AddChild(document, controlTemplate, "Viewbox");
            XmlElement canvas = AddChild(document, viewBox, "Canvas");
            canvas.SetAttribute("Width", symbol.GetWidth(symbols).ToString());
            canvas.SetAttribute("Height", symbols.Height.ToString());

            XmlElement transform = AddChild(document, canvas, "Canvas.RenderTransform");
            XmlElement transformGroup = AddChild(document, transform, "TransformGroup");

            XmlElement translate = AddChild(document, transformGroup, "TranslateTransform");
            translate.SetAttribute("Y", (-symbols.Ascent).ToString());

            XmlElement scale = AddChild(document, transformGroup, "ScaleTransform");
            scale.SetAttribute("ScaleY", "-1");

            XmlElement path = AddChild(document, canvas, "Path");
            path.SetAttribute("Fill", "{TemplateBinding Foreground}");

            XmlElement pathData = AddChild(document, path, "Path.Data");
            pathData.InnerText = symbol.Data;
        }

        private static XmlElement AddChild(XmlDocument document, XmlNode parent, string name)
        {
            XmlElement element = document.CreateElement(string.Empty, name, string.Empty);
            parent.AppendChild(element);
            return element;
        }

        private class Symbols
        {
            public int Width;
            public int Height;

            public int Ascent;

            public readonly List<Glyph> Glyphs = new List<Glyph>();
        }

        private struct Glyph
        {
            public string Set;
            public int Width;
            public string Data;

            internal string ToIconName()
            {
                return $"Icon_Sets_{Set}";
            }

            public int GetWidth(Symbols symbols)
            {
                if (Width > 0)
                    return Width;

                return symbols.Width;
            }
        }
    }
}

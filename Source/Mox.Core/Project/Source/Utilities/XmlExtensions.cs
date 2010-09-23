using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace Mox
{
    public static class XmlPathNavigatorExtensions
    {
        public static string GetAttributeValue(this XPathNavigator navigator, string name)
        {
            return navigator.GetAttribute(name, string.Empty);
        }

        public static T GetAttributeValue<T>(this XPathNavigator navigator, string name, Func<string, T> converter)
        {
            string stringValue = GetAttributeValue(navigator, name);
            return string.IsNullOrEmpty(stringValue) ? default(T) : converter(stringValue);
        }

        public static string GetElementValue(this XPathNavigator navigator, string xpath)
        {
            var node = navigator.SelectSingleNode(xpath);
            return node == null ? string.Empty : node.Value;
        }

        public static T GetElementValue<T>(this XPathNavigator navigator, string xpath, Func<string, T> converter)
        {
            string stringValue = GetElementValue(navigator, xpath);
            return string.IsNullOrEmpty(stringValue) ? default(T) : converter(stringValue);
        }

        public static IEnumerable<string> GetElementValues(this XPathNavigator navigator, string xpath)
        {
            foreach (XPathNavigator node in navigator.Select(xpath))
            {
                yield return node.Value;
            }
        }
    }
}

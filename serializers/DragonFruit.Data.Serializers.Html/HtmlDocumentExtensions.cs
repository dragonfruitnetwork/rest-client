// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using HtmlAgilityPack;

namespace DragonFruit.Data.Serializers.Html
{
    public static class HtmlDocumentExtensions
    {
        /// <summary>
        /// Extracts a value from the <see cref="HtmlNode"/> based on its XPath and attribute name
        /// </summary>
        public static string GetValue(this HtmlNode node, string xpath = default, string attribute = default, bool throwOnNotFound = false)
        {
            var subNode = string.IsNullOrEmpty(xpath) ? node : node.SelectSingleNode(xpath);
            var useInnerText = string.IsNullOrEmpty(attribute);

            switch (useInnerText)
            {
                case false when throwOnNotFound:
                    return subNode.GetAttributeValue(attribute, null) ?? throw new NullReferenceException();

                case false:
                    return subNode.GetAttributeValue(attribute, null);

                case true:
                    return subNode.InnerText;
            }
        }
    }
}

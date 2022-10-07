using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace RaspTpuIcalConverter.Extensions
{
    /// <summary>
    /// Расширение <see cref="HtmlNode" />.
    /// </summary>
    public static class HtmlExtensions
    {
        /// <summary>
        /// Возвращает дочерние элементы (не #text) HtmlNode.
        /// </summary>
        /// <param name="node">HtmlNode, дочерние элементы которого необходимо получить.</param>
        /// <returns>Дочерние элементы (не #text) HtmlNode</returns>
        public static List<HtmlNode> GetChildElementsList(this HtmlNode node)
        {
            var result = node.ChildNodes
                .Where(x => x.NodeType == HtmlNodeType.Element)
                .ToList();

            return result;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace RaspTpuIcalConverter.Extensions
{
    public static class HtmlExtensions
    {
        public static List<HtmlNode> GetChildElementsList(this HtmlNode node)
        {
            var result = node
                .ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element)
                .ToList();

            return result;
        }
    }
}
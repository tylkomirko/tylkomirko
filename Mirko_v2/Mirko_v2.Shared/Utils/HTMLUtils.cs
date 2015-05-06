using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirko_v2.Utils
{
    public static class HTMLUtils
    {
        #region HTMLtoWYPOK
        public static string HTMLtoWYPOK(string html)
        {
            if (html == null) return null;

            html = html.Replace("&quot;", @"""").
                Replace("&lt;", @"<").
                Replace("&gt;", @">").
                Replace("&amp;", @"&");

            return HTMLtoWYPOK_GenerateBlocksForHtml(html);
        }

        private static string HTMLtoWYPOK_GenerateBlocksForHtml(string xhtml)
        {
            string result = string.Empty;
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(xhtml);

                result = HTMLtoWYPOK_GenerateParagraph(doc.DocumentNode);
            }
            catch (Exception)
            {
            }

            return result;
        }


        private static string HTMLtoWYPOK_GenerateParagraph(HtmlNode node)
        {
            return HTMLtoWYPOK_AddChildren(node);
        }

        private static string HTMLtoWYPOK_AddChildren(HtmlNode node)
        {
            string result = string.Empty;
            foreach (HtmlNode child in node.ChildNodes)
            {
                string s = HTMLtoWYPOK_GenerateBlockForNode(child);
                if (s != null)
                    result += s;
            }
            return result;
        }

        private static string HTMLtoWYPOK_GenerateBlockForNode(HtmlNode node)
        {
            switch (node.Name)
            {
                case "#text":
                    return node.InnerText;
                case "a":
                    if (node.OuterHtml.StartsWith(@"<a href=""#"))
                        return node.InnerText;
                    else if (node.OuterHtml.StartsWith(@"<a href=""@"))
                        return node.InnerText;
                    else
                        return HTMLtoWYPOK_GenerateHyperlink(node);
                case "cite":
                    return ">" + node.InnerText;
                case "code":
                    if (node.OuterHtml.StartsWith(@"<code class=""dnone"">"))
                        return "!" + node.InnerText;
                    else
                        return "`" + node.InnerText + "`";
                case "strong":
                    return "**" + node.InnerText + "**";
                case "em":
                    return "_" + node.InnerText + "_";
                case "br":
                    return "";
                default:
                    return node.InnerHtml;
            }
        }

        private static string HTMLtoWYPOK_GenerateHyperlink(HtmlNode node)
        {
            var splitted = node.OuterHtml.Split('"');
            var url = splitted[1];

            return "[" + node.InnerText + "](" + url + ")";
        }
        #endregion

        #region HTMLtoTEXT

        public static string HTMLtoTEXT(string html)
        {
            if (html == null) return null;

            html = html.Replace("&quot;", @"""").
                Replace("&lt;", @"<").
                Replace("&gt;", @">").
                Replace("&amp;", @"&");

            return HTMLtoTEXT_GenerateBlocksForHtml(html);
        }

        private static string HTMLtoTEXT_GenerateBlocksForHtml(string html)
        {
            string result = string.Empty;
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                result = HTMLtoTEXT_GenerateParagraph(doc.DocumentNode);
            }
            catch (Exception)
            {
            }

            return result.Trim();
        }

        private static string HTMLtoTEXT_GenerateParagraph(HtmlNode node)
        {
            return HTMLtoTEXT_AddChildren(node);
        }

        private static string HTMLtoTEXT_AddChildren(HtmlNode node)
        {
            string result = string.Empty;
            foreach (HtmlNode child in node.ChildNodes)
            {
                string s = HTMLtoTEXT_GenerateBlockForNode(child);
                if (s != null)
                    result += s;
            }
            return result;
        }

        private static string HTMLtoTEXT_GenerateBlockForNode(HtmlNode node)
        {
            switch (node.Name)
            {
                case "code":
                    if (node.OuterHtml.StartsWith(@"<code class=""dnone"">"))
                        return "";
                    else
                        return node.InnerText;
                case "br":
                    return "";
                default:
                    return node.InnerHtml;
            }
        }

        #endregion

        #region WYPOKtoHTML
        /*
        public static string WYPOKtoHTML(string wypok)
        {
            if (wypok == null) return null;

            var result = string.Empty;
            int index = 0;

            while (index <= wypok.Length)
            {
                char c = wypok.ElementAt(index++);

                if (c == '*')
                {
                    char nextChar = wypok.ElementAt(index);
                    if (nextChar == '*')
                    { // it's bold text
                        index++;

                        var lastIndex = wypok.IndexOf("**", index);
                        var text = wypok.Substring(index, lastIndex - index);

                        result += "<strong>" + text + "</strong>";

                        index = lastIndex + 2;
                    }
                }
                else if (c == '_')
                {
                    var lastIndex = wypok.IndexOf('_', index);
                    var text = wypok.Substring(index, lastIndex - index);

                    result += "<em>" + text + "</em>";

                    index = lastIndex + 1;
                }
                else if (c == '!')
                {
                    char prevChar = wypok.ElementAt(index - 2);
                    if (prevChar == '\n')
                    { // it's a spoiler
                        var lastIndex = wypok.IndexOf('\n', index);
                        string text;

                        if (lastIndex == -1)
                            text = wypok.Substring(index);
                        else
                            text = wypok.Substring(index, lastIndex - index);

                        result += @"<code class=""dnone"">" + text + "</code>";

                        index += text.Length + 1;
                    }
                }
                else
                {
                    result += c;
                }
            }

            return result;
        }
         * */
        #endregion
    }
}

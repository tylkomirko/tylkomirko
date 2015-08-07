using System;

namespace WykopSDK.Parsers
{
    // Wykop-HTML to Wykop-Markdown
    public static class HtmlToWykop
    {
        public static string Convert(string html)
        {
            if (html == null) return null;
            var tokens = WykopHTMLParser.Parse(html);

            string result = "";
            foreach (var token in tokens)
                result += GenerateTextForToken(token);
            return result;
        }

        private static string GenerateTextForToken(ParserToken token)
        {
            var parseInternal = new Func<ParserToken, string>((t) =>
            {
                if (t.Internal != null)
                {
                    string txt = "";
                    foreach (var internalToken in t.Internal)
                        txt += GenerateTextForToken(internalToken);

                    return txt;
                }
                else
                {
                    return token.Value;
                }
            });

            var parseHyperlink = new Func<ParserToken, string>((t) =>
            {
                var desc = t.Internal[0].Value;
                var url = t.Value;
                return string.Format("[{0}]({1})", desc, url);
            });

            switch (token.Type)
            {
                case TokenTypes.TEXT:
                    return token.Value;
                case TokenTypes.SPOILER:
                    return "!" + parseInternal(token);
                case TokenTypes.STRONG:
                    return "**" + parseInternal(token) + "**";
                case TokenTypes.CODE:
                    return "`" + parseInternal(token) + "`";
                case TokenTypes.EM:
                    return "_" + parseInternal(token) + "_";
                case TokenTypes.CITE:
                    return ">" + parseInternal(token);
                case TokenTypes.HASHTAG:
                    return token.Value;
                case TokenTypes.USERPROFILE:
                    return token.Value;
                case TokenTypes.HYPERLINK:
                    return parseHyperlink(token);
                default:
                    return "";
            }
        }
    }
}

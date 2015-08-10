using System;
using System.Collections.Generic;

namespace WykopSDK.Parsers
{
    // Wykop-HTML to plaintext
    public static class HtmlToText
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

            var returnValueList = new List<TokenTypes>()
            {
                TokenTypes.TEXT, TokenTypes.HASHTAG,
                TokenTypes.USERPROFILE,
            };

            var parseInternalList = new List<TokenTypes>()
            {
                TokenTypes.SPOILER, TokenTypes.STRONG,
                TokenTypes.CODE, TokenTypes.EM,
                TokenTypes.CITE,
            };

            if (token.Type == TokenTypes.HYPERLINK)
                return token.Internal[0].Value; // return description
            else if (returnValueList.Contains(token.Type))
                return token.Value;
            else if (parseInternalList.Contains(token.Type))
                return parseInternal(token);
            else
                return "";
        }
    }
}

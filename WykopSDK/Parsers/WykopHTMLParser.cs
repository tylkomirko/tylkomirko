using System.Collections.Generic;
using System.Linq;
using WykopSDK.Utils;

namespace WykopSDK.Parsers
{
    public static class WykopHTMLParser
    {
        public static List<ParserToken> Parse(string str, bool isInSpoiler = false)
        {
            var list = new List<ParserToken>();

            string input = str.Replace("&quot;", @"""").
                Replace("&lt;", "<").
                Replace("&gt;", ">").
                Replace("&amp;", "&").
                Replace("<br />", "").
                Replace("<br/>", "").
                Replace("#<a href", "<a href").
                Replace("@<a href", "<a href").
                Replace("</a></a>", "</a>");

            int idx = 0;
            ParserToken token = new ParserToken();
            ParserToken lastToken = null;

            while (token.Type != TokenTypes.END_OF_DOCUMENT)
            {
                token = ReadToken(ref input, ref idx, isInSpoiler);

                if (lastToken != null && lastToken.Type == TokenTypes.TEXT && token.Type == TokenTypes.TEXT)
                {
                    var prevValue = token.Value;
                    token = new ParserToken()
                    {
                        Type = TokenTypes.TEXT,
                        Value = lastToken.Value + prevValue,
                    };

                    list.RemoveAt(list.Count - 1);
                }

                list.Add(token);
                lastToken = list.Last();
            }

            return list;
        }

        private static ParserToken ReadToken(ref string input, ref int idx, bool inSpoiler = false)
        {
            var token = new ParserToken()
            {
                Type = TokenTypes.UNKNOWN,
            };

            if (idx >= input.Length)
            {
                token.Type = TokenTypes.END_OF_DOCUMENT;
                return token;
            }

            var firstChar = input[idx];
            if (firstChar == '<')
            {
                // it's a HTML tag

                idx++;
                var closingIndex = input.IndexOf2('>', idx);
                var tagName = input.Between(idx - 1, closingIndex);

                if (tagName.StartsWith("<a href=")) // either hashtag or userprofile link
                {
                    var questionMarkStart = input.IndexOf('"', idx) + 1;
                    var questionMarkEnd = input.IndexOf('"', questionMarkStart);
                    token.Value = input.Between(questionMarkStart, questionMarkEnd);

                    if (tagName[9] == '#') // hashtag
                    {
                        token.Type = TokenTypes.HASHTAG;
                    }
                    else if (tagName[9] == '@') // userprofile
                    {
                        token.Type = TokenTypes.USERPROFILE;
                    }
                    else // hyperlink
                    {
                        token.Type = TokenTypes.HYPERLINK;
                        /* Sometimes, Wypok does really weird things. For example:
                         * <a href="http://www.morele.net/glosniki-komputerowe-edifier-r1100-2-0-czarne-659158/" rel="nofollow"><a href="http://www.morele.net/glosniki-komputerowe-edifier-r1100-2-0-czarne-659158/" rel="nofollow">http://www.morele.net/glosniki-komputerowe-edifier-r1100-2-0-czarne-659158/</a></a>
                         * It seems pretty retarded to me, but we have to get around it somehow.
                         * The simplest way is to check if input contains the second (nested) <a href>. */

                        var damnYouWypok = tagName + '>';
                        var newIndex = input.IndexOf(damnYouWypok, questionMarkEnd);
                        if (newIndex != -1)
                            questionMarkEnd = newIndex + tagName.Length;

                        var descriptionStart = input.IndexOf('>', questionMarkEnd) + 1;
                        var descriptionEnd = input.IndexOf('<', descriptionStart);
                        var description = input.Between(descriptionStart, descriptionEnd);

                        token.Internal = new List<ParserToken>(1) { new ParserToken() { Value = description, Type = TokenTypes.HYPERLINK_DESCRIPTION } };
                    }

                    // now, look for </a>
                    idx = input.IndexOf("</a>", questionMarkEnd) + 4; // add length of </a> = 4
                }
                else if (tagName.StartsWith("<code"))
                {
                    idx = input.IndexOf('>', idx) + 1;
                    var codeEnd = input.IndexOf("</code>", idx);
                    var codeContent = input.Between(idx, codeEnd);

                    token.Internal = Parse(codeContent, isInSpoiler: true);

                    if (tagName.Contains("dnone")) // spoiler
                        token.Type = TokenTypes.SPOILER;
                    else // code
                        token.Type = TokenTypes.CODE;

                    idx = codeEnd + 7;
                }
                else if (tagName.Equals("<strong"))
                {
                    idx = input.IndexOf('>', idx) + 1;
                    var strongEnd = input.IndexOf("</strong>", idx);
                    var strongContent = input.Between(idx, strongEnd);

                    token.Internal = Parse(strongContent);
                    token.Type = TokenTypes.STRONG;

                    idx = strongEnd + 9;
                }
                else if (tagName.Equals("<em"))
                {
                    idx = input.IndexOf('>', idx) + 1;
                    var emEnd = input.IndexOf("</em>", idx);
                    var emContent = input.Between(idx, emEnd);

                    token.Internal = Parse(emContent);
                    token.Type = TokenTypes.EM;

                    idx = emEnd + 5;
                }
                else if (tagName.Equals("<cite"))
                {
                    idx = input.IndexOf('>', idx) + 1;
                    var citeEnd = input.IndexOf("</cite>", idx);
                    var citeContent = input.Between(idx, citeEnd);

                    token.Internal = Parse(citeContent);
                    token.Type = TokenTypes.CITE;

                    idx = citeEnd + 7;
                }
                else // someone used something like <lol>, or just '<' char
                {
                    string[] nextExpectedToken = { "<a href", "<cite>", "<code", "<strong>", "<em>", "\n", "\r", ">" };
                    var nextIdx = tagName.IndexOfArray(nextExpectedToken, 0);
                    var content = tagName.Between(0, nextIdx);

                    token.Type = TokenTypes.TEXT;
                    token.Value = content;

                    idx += content.Length - 1;
                }
            }
            else
            {
                // it's plain text
                var nextIdx = input.IndexOf('<', idx);
                if (nextIdx == -1)
                    nextIdx = input.Length;

                var text = input.Between(idx, nextIdx);

                if (inSpoiler)
                {
                    // look for http:// or www. it's useful in spoilers.
                    var list = new List<int>(2) { text.IndexOf("http://"), text.IndexOf("www.") };

                    var linkStart = list.Min();
                    if (linkStart == 0)
                    {
                        // turn the text into hyperlink.

                        var linkEnd = text.IndexOfArray(new string[] { " ", "\n", "\r" }, linkStart);
                        var link = text.Between(linkStart, linkEnd);

                        token.Type = TokenTypes.HYPERLINK;
                        token.Value = link;
                        token.Internal = new List<ParserToken>(1) { new ParserToken() { Type = TokenTypes.HYPERLINK_DESCRIPTION, Value = link } };

                        idx += link.Length;

                        return token;
                    }
                    else if (linkStart != -1)
                    {
                        // plain text contains a link. first return current text, and then in another run turn it into a hyperlink.
                        //text = input.Between(idx, linkStart);
                        text = text.Between(0, linkStart);
                        nextIdx = idx + text.Length;
                    }
                }

                token.Type = TokenTypes.TEXT;
                token.Value = text;

                idx = nextIdx;
            }

            return token;
        }
    }
}

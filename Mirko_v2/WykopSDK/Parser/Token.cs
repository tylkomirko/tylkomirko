using System.Collections.Generic;

namespace WykopSDK.Parser
{
    public enum TokenTypes
    {
        TEXT,
        STRONG,
        EM,

        HASHTAG,
        USERPROFILE,
        HYPERLINK,
        HYPERLINK_DESCRIPTION,

        SPOILER,
        CODE,
        CITE,

        END_OF_DOCUMENT,
        UNKNOWN,
    };

    public class ParserToken
    {
        public TokenTypes Type;
        public string Value;
        public List<ParserToken> Internal;
    }
}

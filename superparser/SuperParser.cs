using System;

using Superpower;
using Superpower.Parsers;

using System.Linq;

using static Superpower.Parsers.Token;
using Superpower.Model;

namespace SuperParser
{
    public static class SuperParser
    {
        public readonly static TokenListParser<Tokens, string> CommandParser =
            from cmd in EqualTo(Tokens.CHAT)
                .Or(EqualTo(Tokens.SAY))
                .Or(EqualTo(Tokens.ASK))
                .Or(EqualTo(Tokens.FIND))
                .Or(EqualTo(Tokens.DO))
                .Or(EqualTo(Tokens.GO))
                .Or(EqualTo(Tokens.OPT))
                .Or(EqualTo(Tokens.WAIT))
            select cmd.ToStringValue();

        public readonly static TokenListParser<Tokens, string> TextParser =
            from tokens in EqualTo(Tokens.ParenPair)
                .Or(EqualTo(Tokens.Hash))
                .Or(EqualTo(Tokens.Dollar))
                .Or(EqualTo(Tokens.Dot))
                .Or(EqualTo(Tokens.Number))
                .Or(EqualTo(Tokens.Colon))
                .Or(EqualTo(Tokens.Char))
                .Or(EqualTo(Tokens.Space))
                .Many()
            select Join(tokens);

        public readonly static TokenListParser<Tokens, string> NameParserOrig =
            from tokens in EqualTo(Tokens.ParenPair)
                .Or(EqualTo(Tokens.Hash))
                .Or(EqualTo(Tokens.Dollar))
                .Or(EqualTo(Tokens.Dot))
                .Or(EqualTo(Tokens.Number))
                .Or(EqualTo(Tokens.Char))
                .Many()
            select Join(tokens);

        public readonly static TokenListParser<Tokens, string> NameParser =
            from tokens in EqualTo(Tokens.Char).Or(EqualTo(Tokens.Number)).Many()
            select Join(tokens);

        public readonly static TokenListParser<Tokens, Line> ActorParser =
            from name in NameParser
            from colon in EqualTo(Tokens.Colon)
            from cmd in CommandParser.OptionalOrDefault()
            from text in TextParser
            select new Line
            {
                Actor = name,
                Command = cmd,
                Text = text
            };

        public readonly static TokenListParser<Tokens, Line> LineParser =
            from node in ActorParser.Try()
                .Or(TextParser.Select(text => new Line { Text = text }))
            select node;


        public readonly static TokenListParser<Tokens, string> SpaceParser =
            from chars in EqualTo(Tokens.Space).Many().OptionalOrDefault()
            select chars == null ? string.Empty : new string(' ', chars.Length);


        public readonly static TokenListParser<Tokens, Group> LiteralParser =
            from tokens in EqualTo(Tokens.ParenPair)
                .Or(EqualTo(Tokens.Entity))
                .Or(EqualTo(Tokens.Hash))
                .Or(EqualTo(Tokens.Dollar))
                .Or(EqualTo(Tokens.Dot))
                .Or(EqualTo(Tokens.Number))
                .Or(EqualTo(Tokens.Char))
                .Or(EqualTo(Tokens.Space))
                .Many()
                // if this side of the pipe is all whitespace, return null
            select (Group)(tokens.All(x => x.ToStringValue() == " ") ? null
                : Group.Create(string.Join(string.Empty, tokens.Select(t => t.ToStringValue())).Trim())
        );

        /// <summary>
        /// Parses a full expression that may contain text expressions or nested sub-expressions
        /// e.g. "(a | b)", "( (a.c() | b) | (123 | c) )", etc.
        /// </summary>
        public readonly static TokenListParser<Tokens, Group> GroupParser =
            from ws1 in SpaceParser
            from lp in EqualTo(Tokens.LParen)
            from ws2 in SpaceParser
            from groups in
                // check for actual text node first
                LiteralParser.Where(node => node != null)
                .Or(GroupParser)
                .Or(LiteralParser) // then check to see if it's empty
                .ManyDelimitedBy(EqualTo(Tokens.Pipe))
            from ws3 in SpaceParser
            from rp in EqualTo(Tokens.RParen)
            from ws4 in SpaceParser
                // has to have at least two sides and one has to be non-null
            where groups.Length > 1 && groups.Any(node => node != null)
            select Group.Create(groups.Select
                    (node => node ?? new GroupTerm(string.Empty)).ToArray());


        private static string Join(Token<Tokens>[] tokens) => string.Join
            (string.Empty, tokens.Select(t => t.ToStringValue()));

        public static void Main(string[] s) { }
    }

}

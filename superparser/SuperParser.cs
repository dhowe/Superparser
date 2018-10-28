using System;

using Superpower;
using Superpower.Model;

using System.Linq;
using Superpower.Parsers;

namespace SuperParser
{
    public static class MyExtensions
    {
        public static TokenListParser<TKind, T> Try<TKind, T>(this TokenListParser<TKind, T> parser)
        {
            return null;
        }

    }

    public static class SuperParser
    {
        public readonly static TokenListParser<Tokens, string> SpaceParser =
            from tokens in Token.EqualTo(Tokens.Space).Many().OptionalOrDefault()
                //select chars == null ? string.Empty : new string(' ', chars.Length);
            select Join(tokens);

        public readonly static TokenListParser<Tokens, string> CommandParser =
            from ws1 in SpaceParser
            from cmd in (Token.EqualTo(Tokens.CHAT)
                .Or(Token.EqualTo(Tokens.SAY))
                .Or(Token.EqualTo(Tokens.ASK))
                .Or(Token.EqualTo(Tokens.FIND))
                .Or(Token.EqualTo(Tokens.DO))
                .Or(Token.EqualTo(Tokens.GO))
                .Or(Token.EqualTo(Tokens.OPT))
                .Or(Token.EqualTo(Tokens.WAIT))).Optional()
            from ws2 in SpaceParser
            select cmd.HasValue ? cmd.Value.ToStringValue() : string.Empty;

        public readonly static TokenListParser<Tokens, string> StringParser =
            from tokens in (Token.EqualTo(Tokens.ParenPair)
                .Or(Token.EqualTo(Tokens.Hash))
                .Or(Token.EqualTo(Tokens.Dollar))
                .Or(Token.EqualTo(Tokens.Dot))
                .Or(Token.EqualTo(Tokens.Number))
                .Or(Token.EqualTo(Tokens.Colon))
                .Or(Token.EqualTo(Tokens.Space))
                .Or(Token.EqualTo(Tokens.Char)))
                .Many()
            select Join(tokens);

        //public readonly static TextParser<string> NameParser =
            //from name in Identifier.CStyle
            //select name.ToStringValue();

        public readonly static TokenListParser<Tokens, string> NameParser =
            from tokens in Token.EqualTo(Tokens.Char).Many().OptionalOrDefault()
            select Join(tokens);

        public readonly static TokenListParser<Tokens, string> ActorParser =
            from ws1 in SpaceParser
            from name in NameParser
            from colon in Token.EqualTo(Tokens.Colon)
            select name;//colon.HasValue && name != null ? name : "";

        public readonly static TokenListParser<Tokens, Line> LineParser =
            from actor in ActorParser.OptionalOrDefault()
            from cmd in CommandParser.OptionalOrDefault()
            from text in StringParser.OptionalOrDefault()
            select new Line
            {
                Actor = actor,
                Command = cmd,
                Text = text
            };

        //public readonly static TokenListParser<Tokens, Line> LineParser =
            //from actor in ActorParser.Try()
            //    .Or(TextParser.Select(text => new Line { Text = text }))
            //select actor;

        public readonly static TokenListParser<Tokens, Group> LiteralParser =
            from tokens in Token.EqualTo(Tokens.ParenPair)
                .Or(Token.EqualTo(Tokens.Entity))
                .Or(Token.EqualTo(Tokens.Hash))
                .Or(Token.EqualTo(Tokens.Dollar))
                .Or(Token.EqualTo(Tokens.Dot))
                .Or(Token.EqualTo(Tokens.Number))
                .Or(Token.EqualTo(Tokens.Char))
                .Or(Token.EqualTo(Tokens.Space))
                .Many()
                // if this side of the pipe is all whitespace, return null
            select (Group)(tokens.All(x => x.ToStringValue() == " ") ? null
                : Group.Create(Join(tokens).Trim())
        );

        /// <summary>
        /// Parses a full expression that may contain text expressions or nested sub-expressions
        /// e.g. "(a | b)", "( (a.c() | b) | (123 | c) )", etc.
        /// </summary>
        public readonly static TokenListParser<Tokens, Group> GroupParser =
            from ws1 in SpaceParser
            from lp in Token.EqualTo(Tokens.LParen)
            from ws2 in SpaceParser
            from groups in
                // check for actual text node first
                LiteralParser.Where(node => node != null)
                .Or(GroupParser)
                .Or(LiteralParser) // then check to see if it's empty
                .ManyDelimitedBy(Token.EqualTo(Tokens.Pipe))
            from ws3 in SpaceParser
            from rp in Token.EqualTo(Tokens.RParen)
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

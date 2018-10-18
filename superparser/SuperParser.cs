using System;
using Dialogic;

using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

using System.Linq;
using Superpower.Display;
using System.Collections.Generic;

namespace SuperParser
{
    public class SuperTokenizer
    {
        static void Main(string[] args)
        {
            string input = "((a | b) | c)";
            var tokens = SuperTokenizer.Instance.Tokenize(input);
            var result = SuperParser.GroupParser.TryParse(tokens);
            if (result.HasValue)
            {
                // input is valid
                var group = (Group)result.Value;
                Console.WriteLine(group);
                // do what you need with it here, i.e. loop through the nodes, output the text, etc.
            }
            else
            {
                Console.WriteLine("FAILED: "+result.ErrorMessage);
                // not valid
            }
        }

        public static TokenList<Tokens> Tokenize(string input)
        {
            return SuperTokenizer.Instance.Tokenize(input);
        }

        private static Tokenizer<Tokens> Instance { get; } = new TokenizerBuilder<Tokens>()
                .Match(Span.EqualTo("()"), Tokens.ParenPair)
                .Match(Character.EqualTo('('), Tokens.LParen)
                .Match(Character.EqualTo(')'), Tokens.RParen)
                .Match(Character.EqualTo('#'), Tokens.Hash)
                .Match(Character.EqualTo('$'), Tokens.Dollar)
                .Match(Character.EqualTo('.'), Tokens.Dot)
                .Match(Character.EqualTo('|'), Tokens.Pipe)
                .Match(Character.EqualTo(' ').Or(Character.EqualTo('\t')), Tokens.Space)
                .Match(Span.MatchedBy(Character.AnyChar), Tokens.Char)
                .Match(Numerics.Natural, Tokens.Number)
                .Build();
    }

    public abstract class Group
    {
        public static Group Create(string s) => new GroupTerm(s);
        public static Group Create(params Group[] g) => new GroupSet(g);
        public static Group Create(params object[] o) => new GroupSet(o);
    }

    public class GroupTerm : Group
    {
        public GroupTerm(string s) => Value = s;
        public string Value { get; set; }

        public override string ToString() => Value;
        public override int GetHashCode() => HashCode.Combine(Value);
        public override bool Equals(object obj)
        {
            return obj is GroupTerm &&
                ((GroupTerm)obj).Value.Equals(Value);
        }
    }

    public class GroupSet : Group
    {
        public GroupSet(params object[] objs)
        {
            List<Group> groups = new List<Group>();
            foreach (var o in objs)
            {
                Console.WriteLine(o.GetType());
                groups.Add((o is GroupSet) ? (Group)
                    new GroupSet(((GroupSet)o).Groups) : new GroupTerm((string)o)); 
            }
            Groups = groups.ToArray();
        }

        public GroupSet(params Group[] groups) => Groups = groups;

        public Group[] Groups { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is GroupSet)) return false;
            Group[] g = ((GroupSet)obj).Groups;
            for (int i = 0; i < Groups.Count(); i++)
            {
                if (!Groups[i].Equals(g[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Groups);
        }

        public override string ToString()
        {
            var s = "(";
            for (int i = 0; i < Groups.Count(); i++)
            {
                s += Groups[i].ToString();
                if (i < Groups.Count() - 1) s += " | ";
            }
            return s + ")";
        }
    }

    public static class SuperParser
    {
        /// <summary>
        /// Parses any whitespace (if any) and returns a resulting string
        /// </summary>
        public readonly static TokenListParser<Tokens, string> WhitespaceParser =
            from chars in Token.EqualTo(Tokens.Space).Many().OptionalOrDefault()
            select chars == null ? string.Empty : new string(' ', chars.Length);

        /// <summary>
        /// Parses a valid text expression
        /// e.g. "abc", "a.c()", "$c", etc.
        /// </summary>
        public readonly static TokenListParser<Tokens, Group> LiteralParser =
            from tokens in
                Token.EqualTo(Tokens.ParenPair)
                .Or(Token.EqualTo(Tokens.Hash))
                .Or(Token.EqualTo(Tokens.Dollar))
                .Or(Token.EqualTo(Tokens.Dot))
                .Or(Token.EqualTo(Tokens.Number))
                .Or(Token.EqualTo(Tokens.Char))
                .Or(Token.EqualTo(Tokens.Space))
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
            from leadWs in WhitespaceParser
            from lp in Token.EqualTo(Tokens.LParen)
            from groups in LiteralParser
                .Or(Parse.Ref(() => GroupParser))
                .ManyDelimitedBy(Token.EqualTo(Tokens.Pipe))
                .OptionalOrDefault()
            from rp in Token.EqualTo(Tokens.RParen)
            from trailWs in WhitespaceParser
            // has to have at least two sides and one has to be non-null
            where groups.Length > 1 && groups.Any(node => node != null)
                select Group.Create(groups.Select
                    (node => node ?? new GroupTerm(string.Empty)).ToArray());
    }

    public enum Tokens
    {
        None,
        Char,
        Number,

        [Token(Example = "()")]
        ParenPair,

        [Token(Example = "(")]
        LParen,

        [Token(Example = ")")]
        RParen,

        [Token(Example = "#")]
        Hash,

        [Token(Example = "$")]
        Dollar,

        [Token(Example = "|")]
        Pipe,

        [Token(Example = ".")]
        Dot,

        [Token(Example = " ")]
        Space
    }

}

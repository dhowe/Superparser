using System;

using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;

using System.Linq;
using Superpower.Display;

namespace Test
{
    public abstract class Node { }

    public class TextNode : Node
    {
        public string Value { get; set; }
        public override string ToString() => Value;
    }

    public class Expression : Node
    {
        public Node[] Nodes { get; set; }

        public override string ToString()
        {
            var s = "(";
            for (int i = 0; i < Nodes.Count(); i++)
            {
                s += Nodes[i].ToString();
                if (i < Nodes.Count() - 1) s += " | ";
            }
            return s + ")";
        }
    }

    public class TestParser
    {
        static Tokenizer<Tokens> tokenizer = new TokenizerBuilder<Tokens>()
            .Match(Span.EqualTo("()"), Tokens.OpenCloseParen)
            .Match(Character.EqualTo('('), Tokens.LParen)
            .Match(Character.EqualTo(')'), Tokens.RParen)
            .Match(Character.EqualTo('#'), Tokens.Hash)
            .Match(Character.EqualTo('$'), Tokens.Dollar)
            .Match(Character.EqualTo('.'), Tokens.Dot)
            .Match(Character.EqualTo('|'), Tokens.Pipe)
            .Match(Character.EqualTo(' '), Tokens.Whitespace)
            .Match(Span.MatchedBy(Character.AnyChar), Tokens.String)
            .Match(Numerics.Natural, Tokens.Number)
            .Build();

        static void MainX(string[] args)
        {
            string input = "((a | b) | c)";
            var tokens = tokenizer.Tokenize(input);

            var result = Expression.TryParse(tokens);
            if (result.HasValue)
            {
                // input is valid
                var expression = (Expression)result.Value;
                Console.WriteLine(expression);
                // do what you need with it here, i.e. loop through the nodes, output the text, etc.
            }
            else
            {
                Console.WriteLine("NO VALUE");
                // not valid
            }
        }

        /// <summary>
        /// Parses any whitespace (if any) and returns a resulting string
        /// </summary>
        public readonly static TokenListParser<Tokens, string> OptionalWhitespace =
                    from chars in Token.EqualTo(Tokens.Whitespace).Many().OptionalOrDefault()
                    select chars == null ? "" : new string(' ', chars.Length);

        /// <summary>
        /// Parses a valid text expression
        /// e.g. "abc", "a.c()", "$c", etc.
        /// </summary>
        public readonly static TokenListParser<Tokens, Node> TextExpression =
            from tokens in
                Token.EqualTo(Tokens.OpenCloseParen)
                .Or(Token.EqualTo(Tokens.Hash))
                .Or(Token.EqualTo(Tokens.Dollar))
                .Or(Token.EqualTo(Tokens.Dot))
                .Or(Token.EqualTo(Tokens.Number))
                .Or(Token.EqualTo(Tokens.String))
                .Or(Token.EqualTo(Tokens.Whitespace))
                .Many()
                // if this side of the pipe is all whitespace, return null
            select (Node)(
                    tokens.All(x => x.ToStringValue() == " ")
                    ? null
                    : new TextNode
                    {
                        Value = string.Join("", tokens.Select(t => t.ToStringValue())).Trim()
                    }
                );

        /// <summary>
        /// Parses a full expression that may contain text expressions or nested sub-expressions
        /// e.g. "(a | b)", "( (a.c() | b) | (123 | c) )", etc.
        /// </summary>
        public readonly static TokenListParser<Tokens, Node> Expression =
            from leadWs in OptionalWhitespace
            from lp in Token.EqualTo(Tokens.LParen)
            from nodes in TextExpression
                .Or(Parse.Ref(() => Expression))
                .ManyDelimitedBy(Token.EqualTo(Tokens.Pipe))
                .OptionalOrDefault()
            from rp in Token.EqualTo(Tokens.RParen)
            from trailWs in OptionalWhitespace
            where nodes.Length > 1 && nodes.Any(node => node != null) // has to have at least two sides and one has to be non-null
            select (Node)new Expression
            {
                Nodes = nodes.Select(node => node ?? new TextNode { Value = "" }).ToArray()
            };
    }


    public enum Tokens
    {
        None,
        String,
        Number,

        [Token(Example = "()")]
        OpenCloseParen,

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
        Whitespace,
    }
}

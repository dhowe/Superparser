
using System;
using System.Collections.Generic;
using System.Linq;
using Superpower;
using Superpower.Display;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

using static Superpower.Parsers.Character;

namespace SuperParser
{
    public static class SuperTokenizer
    {
        public static TokenList<Tokens> Tokenize(string input)
        {
            return SuperTokenizer.tokenizer.Tokenize(input);
        }

        static readonly Tokenizer<Tokens> tokenizer = new TokenizerBuilder<Tokens>()
            .Match(Span.EqualTo("()"), Tokens.ParenPair)
            .Match(Span.Regex(@"&[a-zA-Z0-9#]+;"), Tokens.Entity)
            .Match(EqualTo('('), Tokens.LParen)
            .Match(EqualTo(')'), Tokens.RParen)
            .Match(EqualTo('#'), Tokens.Hash)
            .Match(EqualTo('$'), Tokens.Dollar)
            .Match(EqualTo('.'), Tokens.Dot)
            .Match(EqualTo('|'), Tokens.Pipe)
            .Match(EqualTo(':'), Tokens.Colon)
            .Match(EqualTo(' ')/*.Or(EqualTo('\t'))*/, Tokens.Space)
            .Match(Span.EqualTo("SAY"), Tokens.SAY)
            .Match(Span.EqualTo("ASK"), Tokens.ASK)
            .Match(Span.EqualTo("FIND"), Tokens.FIND)
            .Match(Span.EqualTo("SET"), Tokens.SET)
            .Match(Span.EqualTo("DO"), Tokens.DO)
            .Match(Span.EqualTo("GO"), Tokens.GO)
            .Match(Span.EqualTo("OPT"), Tokens.OPT)
            .Match(Span.EqualTo("CHAT"), Tokens.CHAT)
            .Match(Span.EqualTo("WAIT"), Tokens.WAIT)
            .Match(Span.MatchedBy(AnyChar), Tokens.Char)
            //.Match(Numerics.Natural, Tokens.Number)
            .Build();

        public static Tokens GetToken(string v)
        {
            var result = Tokenize(v).ConsumeToken();
            return result.HasValue ? result.Value.Kind : Tokens.None;
        }
    }

    public enum Tokens
    {
        None,
        Char,
        Number,


        [Token(Category = "command", Example = "SAY")]
        SAY,

        [Token(Category = "command", Example = "ASK")]
        ASK,

        [Token(Category = "command", Example = "FIND")]
        FIND,

        [Token(Category = "command", Example = "SET")]
        SET,

        [Token(Category = "command", Example = "DO")]
        DO,

        [Token(Category = "command", Example = "GO")]
        GO,

        [Token(Category = "command", Example = "OPT")]
        OPT,

        [Token(Category = "command", Example = "CHAT")]
        CHAT,

        [Token(Category = "command", Example = "WAIT")]
        WAIT,

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

        [Token(Example = ":")]
        Colon,

        [Token(Example = " ")]
        Space,

        //[Token(Example = "\"")]
        //DoubleQuote,

        //[Token(Example = "'")]
        //SingleQuote,

        //[Token(Example = "\t")]
        //Tab,

        [Token(Example = "&nbsp;")]
        Entity,
    }

    public class Line
    {
        public string Actor = string.Empty;
        public string Text = string.Empty;
        public string Command = string.Empty;

        public override string ToString() => "{'"+Actor + "','" + Text+"'}";
        public override bool Equals(object obj) => (obj is Line n
            && n.Actor.Equals(Actor) && n.Text.Equals(Text));
        public override int GetHashCode() => HashCode.Combine(Actor, Text);
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

}
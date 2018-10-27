
using System;
using NUnit.Framework;

using Superpower;
using Superpower.Model;

namespace SuperParser.Test
{
    [TestFixture]
    public class ParserTests
    {

        public void ParseGroups(string input, GroupSet expected = null, bool showTokens = false)
        {
            var tokens = SuperTokenizer.Tokenize(input);
            if (showTokens) Out(tokens);
            var result = SuperParser.GroupParser.TryParse(tokens);

            Assert.That(result.HasValue, Is.EqualTo(expected != null),
                "FAIL: " + input + " " + result.HasValue);

            if (expected != null) Assert.That(result.Value, Is.EqualTo(expected));
        }

        //class GroupTest
        //{
        //    public string input;
        //    public GroupSet expected;

        //    public GroupTest(string s, GroupSet g = null)
        //    {
        //        this.input = s;
        //        this.expected = g;
        //    }

        //    public void Check(bool showTokens = false)
        //    {
        //        var tokens = SuperTokenizer.Tokenize(input);
        //        if (showTokens) Out(tokens);
        //        var result = SuperParser.GroupParser.TryParse(tokens);

        //        Assert.That(result.HasValue, Is.EqualTo(expected != null),
        //            "FAIL: " + input + " " + result.HasValue);

        //        if (expected != null) Assert.That(result.Value, Is.EqualTo(expected));
        //    }
        //}

        [Test]
        public void GroupEquality()
        {
            Assert.That(new GroupTerm("a"), Is.EqualTo(new GroupTerm("a")));
            Assert.That(new GroupSet(new GroupTerm("a")), Is.EqualTo(new GroupSet(new GroupTerm("a"))));
            Assert.That(new GroupSet("a"), Is.EqualTo(new GroupSet(new GroupTerm("a"))));
            Assert.That(new GroupSet("a", "b"), Is.EqualTo(new GroupSet(new GroupTerm("a"), new GroupTerm("b"))));
        }

        [Test]
        public void GroupParsing()
        {
            ParseGroups("(a |)", new GroupSet("a", ""));
            ParseGroups("( | a)", new GroupSet("", "a"));
            ParseGroups("( | a )", new GroupSet("", "a"));
            ParseGroups("( | a&nbsp;)", new GroupSet("", "a&nbsp;"));
            ParseGroups("(a | b)", new GroupSet("a", "b"));
            ParseGroups("(a | b.c()) ", new GroupSet("a", "b.c()"));
            ParseGroups("(aa | bb cc ) ", new GroupSet("aa", "bb cc"));
            ParseGroups("(a | b | c #dd)", new GroupSet("a", "b", "c #dd"));
            ParseGroups("((a | b) | c)", new GroupSet(new GroupSet("a", "b"), "c"));
            ParseGroups("((a | b) | $c)", new GroupSet(new GroupSet("a", "b"), "$c"));
            ParseGroups("((a | b) | (c | d))", new GroupSet(new GroupSet("a", "b"), new GroupSet("c", "d")));
            ParseGroups("(((a | b) | c) | d)", new GroupSet(new GroupSet(new GroupSet("a", "b"), "c"), "d"));

        }

        //[Test]
        public void CommandParsing()
        {
            ParseLine("SAY Hello", new Line() { Actor = "A:", Text = " Hello Goodbye" });
        }

        [Test]
        public void ActorParsing()
        {
            ParseLine("A: Hello Goodbye", new Line() { Actor = "A", Text = " Hello Goodbye" });
            ParseLine("Goodbye A: Hello", new Line() { Text = "Goodbye A: Hello" });
            ParseLine("A:Goodbye", new Line() { Actor = "A", Text = "Goodbye" });
            //ActorTest(" A: Goodbye", new Line() { Actor = "A:", Text = "Goodbye" });
        }

        private static void ParseLine(string input, Line expected)
        {
            var tokens = SuperTokenizer.Tokenize(input);
            var result = SuperParser.LineParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL: '" + input + "' has no value");
            Assert.That(result.Value, Is.EqualTo(expected));
        }

        public static void Out(TokenList<Tokens> vals)
        {
            var count = 0;
            foreach (var v in vals)
            {
                Console.WriteLine((count++) + ": " + v.Kind + "='" + v.Span + "'");
            }
            Console.WriteLine();
        }
    }
}
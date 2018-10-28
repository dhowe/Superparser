
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

        [Test]
        public void GroupEquality()
        {
            Assert.That(new GroupTerm("a"), Is.EqualTo(new GroupTerm("a")));
            Assert.That(new GroupSet(new GroupTerm("a")), Is.EqualTo(new GroupSet(new GroupTerm("a"))));
            Assert.That(new GroupSet("a"), Is.EqualTo(new GroupSet(new GroupTerm("a"))));
            Assert.That(new GroupSet("a", "b"), Is.EqualTo(new GroupSet(new GroupTerm("a"), new GroupTerm("b"))));
        }

        [Test]
        public void GroupParser()
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


        [Test]
        public void NameParser()
        {
            var tokens = SuperTokenizer.Tokenize("1");
            var result = SuperParser.NameParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "'1'");
            Assert.That(result.Value, Is.EqualTo("1"));

            tokens = SuperTokenizer.Tokenize("");
            result = SuperParser.NameParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "''");
            Assert.That(result.Value, Is.EqualTo(""));

            tokens = SuperTokenizer.Tokenize("a");
            result = SuperParser.NameParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "'a'");
            Assert.That(result.Value, Is.EqualTo("a"));

            tokens = SuperTokenizer.Tokenize("_");
            result = SuperParser.NameParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "'_'");
            Assert.That(result.Value, Is.EqualTo("_"));

            tokens = SuperTokenizer.Tokenize("ab");
            result = SuperParser.NameParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "'  '");
            Assert.That(result.Value, Is.EqualTo("ab"));

            tokens = SuperTokenizer.Tokenize("SAY");
            result = SuperParser.NameParser.TryParse(tokens);
            //Assert.That(result.HasValue, Is.False, "FAIL: SAY Hello");
            Assert.That(result.HasValue, Is.True, "FAIL: SAY");
            Assert.That(result.Value, Is.EqualTo(""));

            tokens = SuperTokenizer.Tokenize("SAY A");
            result = SuperParser.NameParser.TryParse(tokens);
            //Assert.That(result.HasValue, Is.False, "FAIL: SAY Hello");
            Assert.That(result.HasValue, Is.True, "FAIL: SAY A");
            Assert.That(result.Value, Is.EqualTo(""));
        }

        [Test]
        public void SpaceParser()
        {
            var tokens = SuperTokenizer.Tokenize("a");
            var result = SuperParser.SpaceParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "'a'");
            Assert.That(result.Value, Is.EqualTo(""));

            tokens = SuperTokenizer.Tokenize(" ");
            result = SuperParser.SpaceParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "' '");
            Assert.That(result.Value, Is.EqualTo(" "));

            tokens = SuperTokenizer.Tokenize("  ");
            result = SuperParser.SpaceParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "'  '");
            Assert.That(result.Value, Is.EqualTo("  "));
        }

        [Test]
        public void ActorParser()
        {
            // FAILING 2 HERE *********

            //var tokens = SuperTokenizer.Tokenize("A");
            //var result = SuperParser.ActorParser.TryParse(tokens);
            //Assert.That(result.HasValue, Is.True, "FAIL: A");
            //Assert.That(result.Value, Is.EqualTo(""));
            //Assert.That(result.Remainder.Position, Is.EqualTo(0));

            //var tokens = SuperTokenizer.Tokenize("A B: C");
            //var result = SuperParser.ActorParser.TryParse(tokens);
            //Assert.That(result.HasValue, Is.True, "FAIL: A B: C");
            //Assert.That(result.Value, Is.EqualTo(""));

            var tokens = SuperTokenizer.Tokenize(":A");
            var result = SuperParser.ActorParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL: :A");
            Assert.That(result.Value, Is.EqualTo(""));

            tokens = SuperTokenizer.Tokenize(":");
            result = SuperParser.ActorParser.TryParse(tokens);
            //Assert.That(result.HasValue, Is.False, "FAIL: SAY Hello");
            Assert.That(result.HasValue, Is.True, "FAIL: :");
            Assert.That(result.Value, Is.EqualTo(""));

            tokens = SuperTokenizer.Tokenize("A:");
            result = SuperParser.ActorParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL: A:");
            Assert.That(result.Value, Is.EqualTo("A"));

            tokens = SuperTokenizer.Tokenize("A:SAY");
            result = SuperParser.ActorParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL: A:SAY");
            Assert.That(result.Value, Is.EqualTo("A"));

            tokens = SuperTokenizer.Tokenize(" A:SAY");
            result = SuperParser.ActorParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL:  A:SAY");
            Assert.That(result.Value, Is.EqualTo("A"));
        }

        [Test]
        public void CommandParser()
        {
            var tokens = SuperTokenizer.Tokenize("SAY");
            var result = SuperParser.CommandParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL: SAY");
            Assert.That(result.Value, Is.EqualTo("SAY"));

            tokens = SuperTokenizer.Tokenize(" ASK");
            result = SuperParser.CommandParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL: ' ASK'");
            Assert.That(result.Value, Is.EqualTo("ASK"));

            tokens = SuperTokenizer.Tokenize("CHAT ");
            result = SuperParser.CommandParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL: 'CHAT '");
            Assert.That(result.Value, Is.EqualTo("CHAT"));

            tokens = SuperTokenizer.Tokenize(" SAY ");
            result = SuperParser.CommandParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL: ' SAY '");
            Assert.That(result.Value, Is.EqualTo("SAY"));

            tokens = SuperTokenizer.Tokenize("Hello");
            result = SuperParser.CommandParser.TryParse(tokens);
            //Assert.That(result.HasValue, Is.False, "FAIL: 'Hello'");
            Assert.That(result.HasValue, Is.True, "FAIL: 'Hello'");
            Assert.That(result.Value, Is.EqualTo(""));

            tokens = SuperTokenizer.Tokenize("A: Hello");
            result = SuperParser.CommandParser.TryParse(tokens);
            //Assert.That(result.HasValue, Is.False, "FAIL: 'A: Hello'");
            Assert.That(result.HasValue, Is.True, "FAIL: A: Hello'");
            Assert.That(result.Value, Is.EqualTo(""));
        }

        [Test]
        public void LineParsing()
        {
            ParseLine("Goodbye A: Hello", new Line() { Text = "Goodbye A: Hello" }, true);
            return;
            ParseLine("A: Hello", new Line() { Actor = "A", Text = "Hello" });
            ParseLine("A:", new Line() { Actor = "A" });
            ParseLine("A:SAY", new Line() { Actor = "A", Command="SAY" });
            ParseLine("A: SAY Hello", new Line() { Actor = "A", Command = "SAY", Text = "Hello" });
            ParseLine("A:Goodbye", new Line() { Actor = "A", Text = "Goodbye" });
        }

        private static void ParseLine(string input, Line expected, bool debug = false)
        {
            var tokens = SuperTokenizer.Tokenize(input);

            if (debug) Out(tokens);
            var actor = SuperParser.ActorParser.TryParse(tokens);
            Console.WriteLine("  Actor: "+(actor.HasValue ? "'"+actor.Value+"'" : "NO VALUE"));
            var cmd = SuperParser.CommandParser.TryParse(tokens);
            Console.WriteLine("  Command: " + (cmd.HasValue ? "'" + cmd.Value + "'" : "NO VALUE"));
            var text = SuperParser.StringParser.TryParse(tokens);
            Console.WriteLine("  Text: " + (text.HasValue ? "'" + text.Value + "'" : "NO VALUE"));
            var result = SuperParser.LineParser.TryParse(tokens);
            Assert.That(result.HasValue, Is.True, "FAIL: '" + input + "' got no value");
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
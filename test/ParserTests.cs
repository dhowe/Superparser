
using System;
using NUnit.Framework;

using Superpower;
using Superpower.Model;

namespace SuperParser.Test
{
    [TestFixture]
    public class ParserTests
    {
        class GroupTest
        {
            public string input;
            public GroupSet expected;

            public GroupTest(string s, GroupSet g = null)
            {
                this.input = s;
                this.expected = g;
            }

            public void Check(bool showTokens = false)
            {
                var tokens = SuperTokenizer.Tokenize(input);
                if (showTokens) Out(tokens);
                var result = SuperParser.GroupParser.TryParse(tokens);

                Assert.That(result.HasValue, Is.EqualTo(expected != null), 
                    "FAIL: "+input+" "+result.HasValue);

                if (expected != null) Assert.That(result.Value, Is.EqualTo(expected));
            }
        }

        [Test]
        public void GroupEquality()
        {
            Assert.That(new GroupTerm("a"), Is.EqualTo(new GroupTerm("a")));
            Assert.That(new GroupSet(new GroupTerm("a")), Is.EqualTo(new GroupSet(new GroupTerm("a"))));
            Assert.That(new GroupSet("a"), Is.EqualTo(new GroupSet(new GroupTerm("a"))));
            Assert.That(new GroupSet("a", "b"), Is.EqualTo(new GroupSet(new GroupTerm("a"), new GroupTerm("b"))));
            //Assert.That(new GroupSet("a", "b"), Is.EqualTo(new GroupSet(new GroupTerm("b"), new GroupTerm("a"))));
        }

        [Test]
        public void ParserExamples()
        {
            new GroupTest("(a |)", new GroupSet("a", "")).Check();
            new GroupTest("(a | b)", new GroupSet("a", "b")).Check();
            new GroupTest("(a | b.c()) ", new GroupSet("a", "b.c()")).Check();
            new GroupTest("(a | b | c #dd)", new GroupSet("a", "b", "c #dd")).Check();
            //new GroupTest("((a | b) | c)", new GroupSet(new GroupSet("a", "b"), "c")).Test(true);
            //new GroupTest("((a | b) | $c)", new GroupSet(new GroupSet("a", "b"), "$c")).Test();
            //new GroupTest("((a | b) | (c | d))", new GroupSet(new GroupSet("a", "b"), new GroupSet("c", "d"))).Check();
            //new GroupTest("(((a | b) | c) | d)", new GroupSet("a", "")).Test();

        }

        public void FailingGroupParser()
        {
            new GroupTest("(aa | bb cc ) ", new GroupSet("aa", "bb cc ")).Check();
        }

        private static void Out(TokenList<Tokens> vals)
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
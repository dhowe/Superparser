using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace SuperParser.Test
{
    [TestFixture]
    public class TokenizerTests
    {
        [Test]
        public void TokenizeEntities()
        {
            Assert.That(SuperTokenizer.GetToken("&nbsp;"), Is.EqualTo(Tokens.Entity));
            Assert.That(SuperTokenizer.GetToken("&160;"), Is.EqualTo(Tokens.Entity));
            Assert.That(SuperTokenizer.GetToken("&lt;"), Is.EqualTo(Tokens.Entity));
        }


    }
}
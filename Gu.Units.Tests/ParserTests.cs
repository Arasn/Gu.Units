﻿namespace Gu.Units.Tests
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading;

    using Gu.Units.Tests.Sources;

    using NUnit.Framework;

    public class ParserTests
    {
        [TestCase("1m", new[] { "sv-se", "en-us" }, 1)]
        [TestCase("-1m", new[] { "sv-se", "en-us" }, -1)]
        [TestCase("1.2m", new[] { "en-us" }, 1.2)]
        [TestCase("1.2m", new[] { "en-us" }, 1.2)]
        [TestCase("1,2m", new[] { "sv-se" }, 1.2)]
        [TestCase("-1m", new[] { "sv-se", "en-us" }, -1)]
        [TestCase("1e3m", new[] { "sv-se", "en-us" }, 1e3)]
        [TestCase("1E3m", new[] { "sv-se", "en-us" }, 1e3)]
        [TestCase("1e+3m", new[] { "sv-se", "en-us" }, 1e+3)]
        [TestCase("1E+3m", new[] { "sv-se", "en-us" }, 1E+3)]
        [TestCase("1.2e-3m", new[] { "en-us" }, 1.2e-3)]
        [TestCase("1.2E-3m", new[] { "en-us" }, 1.2e-3)]
        [TestCase(" 1m", new[] { "sv-se", "en-us" }, 1)]
        [TestCase("1 m", new[] { "sv-se", "en-us" }, 1)]
        [TestCase("1m ", new[] { "sv-se", "en-us" }, 1)]
        [TestCase("1mm", new[] { "sv-se", "en-us" }, 1e-3)]
        [TestCase("1cm", new[] { "sv-se", "en-us" }, 1e-2)]
        public void ParseLength(string s, string[] cultures, double expected)
        {
            foreach (var culture in cultures)
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture);
                var length = Parser.Parse<LengthUnit, Length>(s, Length.From, NumberStyles.Float, cultureInfo);
                Assert.AreEqual(expected, length.Metres);
            }
        }

        [TestCaseSource(typeof(ParseProvider))]
        public void Roundtrip(ParseProvider.ParseData data)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var actual = data.ParseMethod(data.StringValue);
            var expected = data.Quantity;
            Assert.AreEqual(expected, actual);
            var s = actual.ToString();
            var roundtripped = data.ParseMethod(s);
            Assert.AreEqual(expected, roundtripped);
        }

        [TestCase("mm^2")]
        [TestCase("mm\x00B2")]
        public void AreaUnit_Parse(string s)
        {
            var actual = AreaUnit.Parse(s);
            Assert.AreEqual(AreaUnit.SquareMillimetres, actual);
        }

        [TestCaseSource(typeof(TokenSource))]
        public void Tokenize(TokenSource.TokenData data)
        {
            var text = data.Text;
            if (data.Tokens == null)
            {
                Assert.Throws<FormatException>(() => Parser.TokenizeUnit(text));
            }
            else
            {
                var actual = Parser.TokenizeUnit(text);
                Console.WriteLine("expected: {0}", data.ToString(data.Tokens));
                Console.WriteLine("actual:   {0}", data.ToString(actual));
                CollectionAssert.AreEqual(data.Tokens, actual);
            }
        }

        [TestCase("1.0cm", "sv-se")]
        [TestCase("1,0cm", "en-us")]
        public void Exceptions(string s, string culture)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            Assert.Throws<FormatException>(() => Parser.Parse<LengthUnit, Length>(s, Length.From, NumberStyles.Float, cultureInfo));
        }
    }
}

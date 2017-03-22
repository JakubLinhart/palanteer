using System;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace Palanteer.Desktop.Tests
{
    [TestClass]
    public class PositionParserTests
    {
        [TestMethod]
        public void Can_parse_two_numbers_separated_by_space()
        {
            PositionParser.TryParse("1234 4321", out Point point).Should().BeTrue();

            point.X.Should().Be(1234);
            point.Y.Should().Be(4321);
        }

        [TestMethod]
        public void Refuses_parse_one_number()
        {
            PositionParser.TryParse("1234", out Point point).Should().BeFalse();
        }

        [TestMethod]
        public void Refueses_parse_two_texts_separated_by_space()
        {
            PositionParser.TryParse("asdf fdsa", out Point point).Should().BeFalse();
        }

        [TestMethod]
        public void Refuses_parse_three_numbers_separated_by_space()
        {
            PositionParser.TryParse("1234 4321 6666", out Point point).Should().BeFalse();
        }
    }
}

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Palanteer.Tests
{
    [TestClass]
    public class UoAutomapImporterTests
    {
        [TestMethod]
        public void Can_import_line()
        {
            var places = UoAutomapImporter.Import("-point of interest: 1542 822 1 Bandita");

            places.Length.Should().Be(1);
            places.First().Type.Should().Be("point of interest");
            places.First().X.Should().Be(1542);
            places.First().Y.Should().Be(822);
            places.First().Name.Should().Be("Bandita");
        }

        [TestMethod]
        public void Can_import_multiple_lines()
        {
            var places = UoAutomapImporter.Import("-point of interest: 1542 822 1 Bandita" + Environment.NewLine + "-point of interest: 940 2110 1 brouci i bojovnik");

            places.Length.Should().Be(2);
            var secondPlace = places.Skip(1).First();

            secondPlace.Name.Should().Be("brouci i bojovnik");
            secondPlace.X.Should().Be(940);
            secondPlace.Y.Should().Be(2110);
            secondPlace.Type.Should().Be("point of interest");
        }

        [TestMethod]
        public void Can_skip_lines_not_starting_with_plus_or_minus()
        {
            var places = UoAutomapImporter.Import("3" + Environment.NewLine
                + "-point of interest: 1542 822 1 Bandita" + Environment.NewLine
                + "+point of interest: 940 2110 1 brouci i bojovnik");

            places.Length.Should().Be(2);
        }

        [TestMethod]
        public void Can_skip_empty_line()
        {
            var places = UoAutomapImporter.Import(string.Empty + Environment.NewLine
                + "-point of interest: 1542 822 1 Bandita" + Environment.NewLine
                + "+point of interest: 940 2110 1 brouci i bojovnik");

            places.Length.Should().Be(2);
        }
    }
}

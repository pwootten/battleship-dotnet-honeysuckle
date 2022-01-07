
namespace Battleship.Ascii.Tests
{
   using Battleship.GameController.Contracts;
   using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
   public class ParsePositionTests
   {
      [TestMethod]
      public void ParseLetterNumber()
      {
         var actual = Program.ParsePosition("A1");

         var expected = new Position(Letters.A, 1);

         Assert.AreEqual(expected, actual);
      }

        [TestMethod]
        public void Test_GetRandomPosition()
        {
            var positions = new List<Position>();
            for (var i = 0; i < 64; i++) {
                positions.Add(Program.GetRandomPosition());
            }

            Assert.AreEqual(64, positions.Select(x => $"{x.Column},{x.Row}").Distinct().Count());
        }

        [TestMethod]
        public void Test_ValidateInputPosition()
        {
            var position = Program.ParsePosition("H20");
            Assert.IsNull(position);
            position = Program.ParsePosition("Test123");
            Assert.IsNull(position);
            position = Program.ParsePosition("A1");
            Assert.IsNotNull(position);
        }

        public void Test_ValidateRandomisedFleet()
        {
            var fleets = new List<string>();
            for (var i = 0; i < 50; i++)
            {
                Program.InitializeEnemyFleet();
                var expectedSize = Program.enemyFleet.Sum(s => s.Size);
                var actualSize = Program.enemyFleet.SelectMany(s => s.Positions).Select(p => $"{p.Column},{p.Row}").Distinct().Count();
                //var temp = string.Join("\r\n", Program.enemyFleet.Select(s => $"{s.Name}({s.Size}). Positions: {string.Join(",", s.Positions.Select(p => $"{p.Column}{p.Row}"))}"));

                // Total size of the fleet should be total number of distinct positions in the fleet
                Assert.AreEqual(expectedSize, actualSize);
                // All ships should have a position count equal to their size
                Assert.IsTrue(Program.enemyFleet.All(s => s.Size == s.Positions.Count));
                // All Positions should be within the grid
                Assert.IsTrue(Program.enemyFleet.SelectMany(s => s.Positions).All(p => p.Row >= 1 && p.Row <= 8 && (int)p.Column >= 0 && (int)p.Column <= 7));

                var output = string.Join(",", Program.enemyFleet.SelectMany(s => s.Positions).Select(p => $"{p.Column}{p.Row}").ToList());
                fleets.Add(output);
            }
            // All generated fleets should be different
            Assert.AreEqual(50, fleets.Distinct().Count());
        }
    }
}

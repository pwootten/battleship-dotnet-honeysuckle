﻿
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
            for (var i = 0; i < 20; i++) {
                positions.Add(Program.GetRandomPosition());
            }

            Assert.AreEqual(20, positions.Select(x => $"{x.Column},{x.Row}").Distinct().Count());
        }
    }
}

using Battleship.GameController.Contracts;
using System;

namespace Battleship.Ascii
{
    public class PositionState
    {
        public ConsoleColor Color { get; set; }
        public Position Position { get; set; }
        public State Status { get; set; }
    }

    public enum State
    {
        Unknown,
        Water,
        Hit,
        Miss,
        Ship
    }
}
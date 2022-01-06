using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Battleship.GameController.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The ship.
    /// </summary>
    public class Ship
    {
        private bool isPlaced;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Ship"/> class.
        /// </summary>
        public Ship()
        {
            Positions = new List<Position>();
            Hits = new List<Position>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the positions.
        /// </summary>
        public List<Position> Positions { get; set; }
        private List<Position> Hits { get; set; }

        /// <summary>
        /// The color of the ship
        /// </summary>
        public ConsoleColor Color { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public int Size { get; set; }

        #endregion

        #region Public Methods and Operators

        public void RegisterHit(Position position)
        {
            if (position == null) return;
            if (!Hits.Any(p => p.Equals(position)) &&
                Positions.Any(p => p.Equals(position)))
            {
                Hits.Add(position);
            }
        }

        public bool IsDestroyed { get => Hits.Count == Positions.Count; }

        /// <summary>
        /// The add position.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        public bool AddPosition(string input)
        {
            // input must be 2 chars
            if (input?.Length != 2) return false;

            input = input.ToUpper();
            // First char must be a letter between A and H 
            // Second char must be a number between 1 and 8
            if (!Enum.TryParse<Letters>(input.Substring(0, 1), out var letter) ||
                !int.TryParse(input.Substring(1, 1), out var number) ||
                number > 8 || number < 1) return false;

            return AddPosition(new Position { Column = letter, Row = number });
        }

        public bool AddPosition(Position position)
        {
            // Can't add more than 2 positions (start and end)
            if (Positions.Count >= 2) return false;

            if (Positions.Count == 0)
            {
                Positions.Add(position);
                return true;
            }

            if (Positions.Count == 1)
            {
                if (Positions[0].Row == position.Row)
                {
                    if (Math.Abs(Positions[0].Column - position.Column) == Size - 1)
                    {
                        for (int i = (int)Positions[0].Column + 1; i < Size + (int)Positions[0].Column; i++)
                        {
                            Positions.Add(new Position { Column = (Letters)i, Row = position.Row });
                        }
                        return true;
                    }
                }
                if (Positions[0].Column == position.Column)
                {
                    if (Math.Abs(Positions[0].Row - position.Row) == Size - 1)
                    {
                        for (int i = Positions[0].Row + 1; i < Size + Positions[0].Row; i++)
                        {
                            Positions.Add(new Position { Column = position.Column, Row = i });
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsPlaced
        {
            get { return isPlaced; }
            set
            {
                if (value.Equals(isPlaced)) return;
                isPlaced = value;
            }
        }
        #endregion

        private enum Orientation
        {
            Verticle,
            Horizontal
        }
    }
}
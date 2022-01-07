
namespace Battleship.Ascii
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Battleship.Ascii.TelemetryClient;
    using Battleship.GameController;
    using Battleship.GameController.Contracts;

    public class Program
    {
        private static List<Ship> myFleet;

        public static List<Ship> enemyFleet;

        private static int enemyFleetCount = 0;

        private static ITelemetryClient telemetryClient;

        private static ConsoleColor mainColor = ConsoleColor.Yellow;

        private static readonly List<Position> ComputerGuesses = new List<Position>();

        static void Main()
        {
            telemetryClient = new ApplicationInsightsTelemetryClient();
            telemetryClient.TrackEvent("ApplicationStarted");

            Console.ForegroundColor = mainColor;

            try
            {
                Console.Title = "Battleship";
                Console.BackgroundColor = ConsoleColor.Black;
                //Console.Clear();

                Console.WriteLine(@"                                     |__");
                Console.WriteLine(@"                                     |\/");
                Console.WriteLine(@"                                     ---");
                Console.WriteLine(@"                                     / | [");
                Console.WriteLine(@"                              !      | |||");
                Console.WriteLine(@"                            _/|     _/|-++'");
                Console.WriteLine(@"                        +  +--|    |--|--|_ |-");
                Console.WriteLine(@"                     { /|__|  |/\__|  |--- |||__/");
                Console.WriteLine(@"                    +---------------___[}-_===_.'____                 /\");
                Console.WriteLine(@"                ____`-' ||___-{]_| _[}-  |     |_[___\==--            \/   _");
                Console.WriteLine(@" __..._____--==/___]_|__|_____________________________[___\==--____,------' .7");
                Console.WriteLine(@"|                        Welcome to Battleship                         BB-61/");
                Console.WriteLine(@" \_________________________________________________________________________|");
                Console.WriteLine();

                InitializeGame();

                StartGame();
            }
            catch (Exception e)
            {
                Console.WriteLine("A serious problem occured. The application cannot continue and will be closed.");
                telemetryClient.TrackException(e);
                Console.WriteLine("");
                Console.WriteLine("Error details:");
                throw new Exception("Fatal error", e);
            }
        }
        private static void DisplayGrid(IEnumerable<Ship> ships)
        {
            for (var row = 1; row <= 8; row++)
            {
                for (var col = 0; col <= 7; col++)
                {
                    var ship = GetShip(ships, new Position { Column = (Letters)col, Row = row });
                    if (ship != null)
                    {
                        Console.ForegroundColor = ship.Color;
                        Console.Write(@"(#) ");
                        continue;
                    }
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(@"(*) ");
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = mainColor;
        }

        public static Ship GetShip(IEnumerable<Ship> ships, Position shot)
        {
            if (ships == null)
            {
                throw new ArgumentNullException("ships");
            }

            if (shot == null)
            {
                throw new ArgumentNullException("shot");
            }

            foreach (var ship in ships)
            {
                foreach (var position in ship.Positions)
                {
                    if (position.Equals(shot))
                    {
                        return ship;
                    }
                }
            }

            return null;
        }

        private static void StartGame()
        {
           // Console.Clear();
            Console.WriteLine("                  __");
            Console.WriteLine(@"                 /  \");
            Console.WriteLine("           .-.  |    |");
            Console.WriteLine(@"   *    _.-'  \  \__/");
            Console.WriteLine(@"    \.-'       \");
            Console.WriteLine("   /          _/");
            Console.WriteLine(@"  |      _  /""");
            Console.WriteLine(@"  |     /_\'");
            Console.WriteLine(@"   \    \_/");
            Console.WriteLine(@"    """"""""");

            do
            {
                Console.WriteLine("My Fleet");
                DisplayGrid(myFleet);
                Console.WriteLine();
                Console.WriteLine("Player, it's your turn");
                Console.WriteLine("Enter coordinates for your shot :");
                var position = ParsePosition(Console.ReadLine());
                var isHit = GameController.CheckIsHit(enemyFleet, position);
                telemetryClient.TrackEvent("Player_ShootPosition", new Dictionary<string, string>() { { "Position", position.ToString() }, { "IsHit", isHit.ToString() } });
                if (isHit)
                {
                    DrawExplosion();

                    WriteMessage("Yeah ! Nice hit !", ConsoleColor.Red);
                }
                else
                {
                    WriteMessage("Miss", ConsoleColor.Blue);
                }

                position = GetRandomPosition();
                isHit = GameController.CheckIsHit(myFleet, position);
                telemetryClient.TrackEvent("Computer_ShootPosition", new Dictionary<string, string>() { { "Position", position.ToString() }, { "IsHit", isHit.ToString() } });

                WriteBreak();

                var hitMissMessage = isHit ? "has hit your ship !" : "miss";
                var hitMissMessageColor = isHit ? ConsoleColor.Red : ConsoleColor.Blue;

                if (isHit)
                {
                    DrawExplosion();
                }

                WriteMessage($"Computer shot in {position.Column}{position.Row} and {hitMissMessage}", hitMissMessageColor);
                WriteBreak();
            }
            while (true);
        }

        public static Position ParsePosition(string input)
        {
            var letter = (Letters)Enum.Parse(typeof(Letters), input.ToUpper().Substring(0, 1));
            var number = int.Parse(input.Substring(1, 1));
            return new Position(letter, number);
        }

        public static Position GetRandomPosition()
        {
            int rows = 9;
            int lines = 8;
            var random = new Random();

            Position position;
            do
            {
                var letter = (Letters)random.Next(lines);
                var number = random.Next(1, rows);
                position = new Position(letter, number);
            } while (ComputerGuesses.Contains(position));

            ComputerGuesses.Add(position);
            return position;
        }

        private static void WriteMessage(string message, ConsoleColor messageColor = ConsoleColor.Yellow)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = messageColor;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }

        private static void WriteBreak()
        {
            WriteMessage("\n================================================================================\n");
        }

        private static void DrawExplosion()
        {
            Console.Beep();
            WriteMessage(@"                \         .  ./", ConsoleColor.Red);
            WriteMessage(@"              \      .:"";'.:..""   /", ConsoleColor.Red);
            WriteMessage(@"                  (M^^.^~~:.'"").", ConsoleColor.Red);
            WriteMessage(@"            -   (/  .    . . \ \)  -", ConsoleColor.Red);
            WriteMessage(@"               ((| :. ~ ^  :. .|))", ConsoleColor.Red);
            WriteMessage(@"            -   (\- |  \ /  |  /)  -", ConsoleColor.Red);
            WriteMessage(@"                 -\  \     /  /-", ConsoleColor.Red);
            WriteMessage(@"                   \  \   /  /", ConsoleColor.Red);
        }

        private static void InitializeGame()
        {
            InitializeMyFleet();

            InitializeEnemyFleet();
        }

        private static void InitializeMyFleet()
        {
            myFleet = GameController.InitializeShips().ToList();

            WriteBreak();

            Console.WriteLine("Please position your fleet (Game board size is from A to H and 1 to 8) :");

            foreach (var ship in myFleet)
            {
                Console.WriteLine();
                Console.WriteLine("Please enter the positions for the {0} (size: {1})", ship.Name, ship.Size);
                WriteBreak();

                Console.WriteLine("Enter start position (e.g. A3):");
                var position = Console.ReadLine();
                WriteBreak();
                while (!ship.AddPosition(position))
                {
                    Console.WriteLine("Position not valid. Enter position (e.g. A3):");
                    position = Console.ReadLine();
                    WriteBreak();
                }
                Console.WriteLine("Enter end position (e.g. A3):");
                position = Console.ReadLine();
                WriteBreak();
                while (!ship.AddPosition(position))
                {
                    Console.WriteLine("Position not valid. Enter position (e.g. A3):");
                    position = Console.ReadLine();
                    WriteBreak();
                }
            }
        }

        public static void InitializeEnemyFleet()
        {
            enemyFleet = GameController.InitializeShips().ToList();
            GetComputerStartPositions();
        }

        private static void GetComputerStartPositions()
        {
            int rows = 8;
            int lines = 8;

            var random = new Random();

            enemyFleetCount = 0;
            foreach (var ship in enemyFleet)
            {
                Position startPosition;
                do
                {
                    var letter = (Letters)random.Next(lines);
                    var number = random.Next(1, rows);
                    startPosition = new Position(letter, number);

                } while (enemyFleet.SelectMany(f => f.Positions).Contains(startPosition));

                placeComputerShip(startPosition, ship.Size);
            }

        }

        private static void placeComputerShip(Position startPosition, int size)
        {
            Random random = new Random();
            int directions = 4;
            int number = 0;
            bool isInvalidShipPosition = false;
            Directions direction = (Directions)random.Next(directions);
            List<Directions> directionList = new List<Directions>();
            List<Position> positionList = null;
            Letters letter;
            Position position;

            directionList.Add(direction);

            do
            {
                isInvalidShipPosition = false;
                positionList = new List<Position>();

                //Add funtionality here to place the ship
                switch (direction)
                {
                    case Directions.North:
                    
                        number = startPosition.Row - (size - 1);

                        if (number >= 1 && !enemyFleet.SelectMany(f => f.Positions).Contains(startPosition))
                        {
                            
                            for (int row = startPosition.Row; row >= number; row--)
                            {
                                position = new Position(startPosition.Column, row);
                                positionList.Add(position);

                                if (enemyFleet.SelectMany(f => f.Positions).Contains(position))
                                {
                                    isInvalidShipPosition = true;
                                    break;
                                }
                            }

                            if (!isInvalidShipPosition)
                            {
                                setShipPosition(positionList);
                            }
                        }
                        else
                            isInvalidShipPosition = true;

                        break;

                    case Directions.East:

                        letter = startPosition.Column + (size - 1);

                        if (letter <= Letters.H && !enemyFleet.SelectMany(f => f.Positions).Contains(startPosition))
                        {
                            

                            for (Letters column = startPosition.Column; column <= letter; column++)
                            {
                                position = new Position(column, startPosition.Row);
                                positionList.Add(position);

                                if (enemyFleet.SelectMany(f => f.Positions).Contains(position))
                                {
                                    isInvalidShipPosition = true;
                                    break;
                                }
                            }

                            if (!isInvalidShipPosition)
                            {
                                setShipPosition(positionList);
                            }
                        }
                        else
                            isInvalidShipPosition = true;

                        break;

                    case Directions.South:

                        number = startPosition.Row + (size - 1);

                        if (number <= 8 && !enemyFleet.SelectMany(f => f.Positions).Contains(startPosition))
                        {
                            for (int row = startPosition.Row; row <= number; row++)
                            {
                                position = new Position(startPosition.Column, row);
                                positionList.Add(position);

                                if (enemyFleet.SelectMany(f => f.Positions).Contains(position))
                                {
                                    isInvalidShipPosition = true;
                                    break;
                                }
                            }

                            if (!isInvalidShipPosition)
                            {
                                setShipPosition(positionList);
                            }
                        }
                        else
                            isInvalidShipPosition = true;

                        break;

                    case Directions.West:

                        letter = startPosition.Column - (size - 1);

                        if (letter >= Letters.A && !enemyFleet.SelectMany(f => f.Positions).Contains(startPosition))
                        {
                            for (Letters column = startPosition.Column; column >= letter; column--)
                            {
                                position = new Position(column, startPosition.Row);
                                positionList.Add(position);

                                if (enemyFleet.SelectMany(f => f.Positions).Contains(position))
                                {
                                    isInvalidShipPosition = true;
                                    break;
                                }
                            }

                            if (!isInvalidShipPosition)
                            {
                                setShipPosition(positionList);
                            }
                        }
                        else
                            isInvalidShipPosition = true;

                        break;
                }

                // If the start position is invalid, then try a new direction
                if (isInvalidShipPosition)
                {
                    while (directionList.Count < 4)
                    {
                        direction = (Directions)random.Next(directions);

                        if (!directionList.Contains(direction))
                        {
                            directionList.Add(direction);
                            break;
                        }
                    }
                    
                }

            } while (directionList.Count < 4 && isInvalidShipPosition);
        }

        private static void setShipPosition(List<Position> positionList)
        {
            foreach (Position pstn in positionList)
            {
                enemyFleet[enemyFleetCount].Positions.Add(pstn);
            }

            enemyFleetCount++;
        }
    }

}
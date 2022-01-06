
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

        private static List<Ship> enemyFleet;

        private static ITelemetryClient telemetryClient;

        private static ConsoleColor mainColor = ConsoleColor.Yellow;

        private static List<PositionState> MyState;
        private static List<PositionState> EnemyState;

        private static List<Position> MyGuesses = new List<Position>();

        static void Main()
        {
            telemetryClient = new ApplicationInsightsTelemetryClient();
            telemetryClient.TrackEvent("ApplicationStarted");

            Console.ForegroundColor = mainColor;

            try
            {
                Console.Title = "Battleship";
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Clear();

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
            Console.Clear();
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

            var round = 1;
            do
            {
                Console.Clear();
                Console.WriteLine($"Round {round}");
                ShowGridStatus();

                Console.WriteLine();
                Console.WriteLine("Player, it's your turn");
                Console.WriteLine("Enter coordinates for your shot :");
                var position = ParsePosition(Console.ReadLine());
                while (position == null || MyGuesses.Contains(position))
                {
                    Console.WriteLine("Invalid position or you have already used this position :");
                    position = ParsePosition(Console.ReadLine());
                }
                MyGuesses.Add(position);

                var isHit = GameController.CheckIsHit(enemyFleet, position);
                UpdateGrid(EnemyState, position, isHit);
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
                UpdateGrid(MyState, position, isHit);

                WriteMessage($"Computer shot in {position.Column}{position.Row} and {hitMissMessage}", hitMissMessageColor);
                WriteBreak();

                Console.WriteLine("Press the enter key to continue");
                Console.ReadLine();

                round++;
            }
            while (true);
        }

        private static void ShowGridStatus()
        {
            var defaultColor = Console.ForegroundColor;
            Console.WriteLine("#################################################");
            Console.WriteLine("My Fleet");
            DisplayGrid(MyState);
            Console.WriteLine($"My Guesses: {string.Join(", ", MyGuesses.Select(g => $"{g.Column}{g.Row}"))}");
            myFleet.ForEach(f =>
            {
                Console.Write($"{f.Name}:");
                Console.ForegroundColor = f.IsDestroyed ? ConsoleColor.Red : ConsoleColor.Green;
                Console.Write(f.IsDestroyed ? "Destroyed" : "Active");
                Console.ForegroundColor = defaultColor;
                Console.Write(", ");
            });
            Console.ForegroundColor = defaultColor;

            Console.WriteLine();
            Console.WriteLine("#######################");
            Console.WriteLine("Enemy Fleet");
            DisplayGrid(EnemyState);
            //Console.WriteLine($"Computer Guesses: {string.Join(", ", ComputerGuesses.Select(g => $"{g.Column}{g.Row}"))}");
            enemyFleet.ForEach(f =>
            {
                Console.Write($"{f.Name}:");
                Console.ForegroundColor = f.IsDestroyed ? ConsoleColor.Red : ConsoleColor.Green;
                Console.Write(f.IsDestroyed ? "Destroyed" : "Active");
                Console.ForegroundColor = defaultColor;
                Console.Write(", ");
            });
            Console.ForegroundColor = defaultColor;

            Console.WriteLine();
            Console.WriteLine("#################################################");
        }

        private static void DisplayGrid(IEnumerable<PositionState> state)
        {
            Console.WriteLine("   A   B   C   D   E   F   G   H");
            for (var row = 1; row <= 8; row++)
            {
                Console.ForegroundColor = mainColor;
                Console.Write($"{row} ");
                for (var col = 0; col <= 7; col++)
                {
                    var positionState = state.FirstOrDefault(st => st.Position.Column == (Letters)col && st.Position.Row == row);
                    if (positionState != null)
                    {
                        Console.ForegroundColor = positionState.Color;

                        Console.Write($"({(positionState.Status == State.Hit ? "!" : positionState.Status == State.Unknown ? "?" : positionState.Status == State.Miss ? "O" : "#")}) ");
                        continue;
                    }
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(@"(*) ");
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = mainColor;
        }

        private static void UpdateGrid(List<PositionState> positionStates, Position position, bool isHit)
        {
            var positionState = positionStates.FirstOrDefault(ps => ps.Position.Equals(position));
            if (positionState == null) return;

            positionState.Status = isHit ? State.Hit : State.Miss;

            if (positionState.Color == ConsoleColor.Cyan)
            {
                positionState.Color = isHit ? ConsoleColor.Red : ConsoleColor.Yellow;
            }
        }

        public static Position ParsePosition(string input)
        {
            if (input?.Length != 2) return null;

            input = input.ToUpper();
            // First char must be a letter between A and H 
            // Second char must be a number between 1 and 8
            if (!Enum.TryParse<Letters>(input.Substring(0, 1), out var letter) ||
                !int.TryParse(input.Substring(1, 1), out var number) ||
                number > 8 || number < 1) return null;

            return new Position(letter, number);
        }

        private static Position GetRandomPosition()
        {
            int rows = 8;
            int lines = 8;
            var random = new Random();
            var letter = (Letters)random.Next(lines);
            var number = random.Next(rows);
            var position = new Position(letter, number);
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
            //InitializeMyFleet();
            InitializeMyFleetStatic();
            InitializeEnemyFleet();
            InitializeGrids();
        }

        private static void InitializeGrids()
        {
            MyState = new List<PositionState>();
            EnemyState = new List<PositionState>();
            for (var row = 1; row <= 8; row++)
            {
                for (var col = 0; col <= 7; col++)
                {
                    var position = new Position { Column = (Letters)col, Row = row };
                    var ship = GetShip(myFleet, position);
                    MyState.Add(new PositionState
                    {
                        Position = position,
                        Color = ship?.Color ?? ConsoleColor.Cyan,
                        Status = ship == null ? State.Water : State.Ship
                    });
                    EnemyState.Add(new PositionState
                    {
                        Position = position,
                        Color = ConsoleColor.Cyan,
                        Status = State.Unknown
                    });
                }
            }
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

        private static void InitializeMyFleetStatic()
        {
            myFleet = GameController.InitializeShips().ToList();

            myFleet[0].AddPosition(new Position { Column = Letters.A, Row = 1 });
            myFleet[0].AddPosition(new Position { Column = Letters.A, Row = 5 });

            myFleet[1].AddPosition(new Position { Column = Letters.F, Row = 1 });
            myFleet[1].AddPosition(new Position { Column = Letters.F, Row = 4 });

            myFleet[2].AddPosition(new Position { Column = Letters.G, Row = 4 });
            myFleet[2].AddPosition(new Position { Column = Letters.G, Row = 6 });

            myFleet[3].AddPosition(new Position { Column = Letters.F, Row = 8 });
            myFleet[3].AddPosition(new Position { Column = Letters.H, Row = 8 });

            myFleet[4].AddPosition(new Position { Column = Letters.C, Row = 5 });
            myFleet[4].AddPosition(new Position { Column = Letters.C, Row = 6 });
        }

        private static void InitializeEnemyFleet()
        {
            enemyFleet = GameController.InitializeShips().ToList();

            enemyFleet[0].AddPosition(new Position { Column = Letters.B, Row = 4 });
            enemyFleet[0].AddPosition(new Position { Column = Letters.B, Row = 8 });

            enemyFleet[1].AddPosition(new Position { Column = Letters.E, Row = 5 });
            enemyFleet[1].AddPosition(new Position { Column = Letters.E, Row = 8 });

            enemyFleet[2].AddPosition(new Position { Column = Letters.A, Row = 3 });
            enemyFleet[2].AddPosition(new Position { Column = Letters.C, Row = 3 });

            enemyFleet[3].AddPosition(new Position { Column = Letters.F, Row = 8 });
            enemyFleet[3].AddPosition(new Position { Column = Letters.H, Row = 8 });

            enemyFleet[4].AddPosition(new Position { Column = Letters.C, Row = 5 });
            enemyFleet[4].AddPosition(new Position { Column = Letters.C, Row = 6 });
        }
    }

}

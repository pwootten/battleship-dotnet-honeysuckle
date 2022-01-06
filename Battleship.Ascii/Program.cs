
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

        private static List<PositionState> MyState;
        private static List<PositionState> EnemyState;

        private static ITelemetryClient telemetryClient;

        static void Main()
        {
            telemetryClient = new ApplicationInsightsTelemetryClient();
            telemetryClient.TrackEvent("ApplicationStarted");

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
        private static void DisplayGrid(IEnumerable<PositionState> state)
        {
            var startColor = Console.ForegroundColor;

            Console.WriteLine("   A   B   C   D   E   F   G   H");
            for (var row = 1; row <= 8; row++)
            {
                Console.ForegroundColor = startColor;
                Console.Write($"{row} ");
                for (var col = 0; col <= 7; col++)
                {
                    var positionState = state.FirstOrDefault(st => st.Position.Column == (Letters)col && st.Position.Row == row);// GetShip(ships, new Position { Column = (Letters)col, Row = row });
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
            Console.ForegroundColor = startColor;
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

            do
            {
                ShowGridStatus();

                Console.WriteLine();
                Console.WriteLine("Player, it's your turn");
                Console.WriteLine("Enter coordinates for your shot :");
                var position = ParsePosition(Console.ReadLine());
                var isHit = GameController.CheckIsHit(enemyFleet, position);
                UpdateGrid(EnemyState, position, isHit);
                telemetryClient.TrackEvent("Player_ShootPosition", new Dictionary<string, string>() { { "Position", position.ToString() }, { "IsHit", isHit.ToString() } });
                if (isHit)
                {
                    Console.Beep();

                    Console.WriteLine(@"                \         .  ./");
                    Console.WriteLine(@"              \      .:"";'.:..""   /");
                    Console.WriteLine(@"                  (M^^.^~~:.'"").");
                    Console.WriteLine(@"            -   (/  .    . . \ \)  -");
                    Console.WriteLine(@"               ((| :. ~ ^  :. .|))");
                    Console.WriteLine(@"            -   (\- |  \ /  |  /)  -");
                    Console.WriteLine(@"                 -\  \     /  /-");
                    Console.WriteLine(@"                   \  \   /  /");
                }

                Console.WriteLine(isHit ? "Yeah ! Nice hit !" : "Miss");
                ShowGridStatus();

                position = GetRandomPosition();
                isHit = GameController.CheckIsHit(myFleet, position);
                telemetryClient.TrackEvent("Computer_ShootPosition", new Dictionary<string, string>() { { "Position", position.ToString() }, { "IsHit", isHit.ToString() } });
                Console.WriteLine();
                Console.WriteLine("Computer shot in {0}{1} and {2}", position.Column, position.Row, isHit ? "has hit your ship !" : "miss");
                if (isHit)
                {
                    Console.Beep();

                    Console.WriteLine(@"                \         .  ./");
                    Console.WriteLine(@"              \      .:"";'.:..""   /");
                    Console.WriteLine(@"                  (M^^.^~~:.'"").");
                    Console.WriteLine(@"            -   (/  .    . . \ \)  -");
                    Console.WriteLine(@"               ((| :. ~ ^  :. .|))");
                    Console.WriteLine(@"            -   (\- |  \ /  |  /)  -");
                    Console.WriteLine(@"                 -\  \     /  /-");
                    Console.WriteLine(@"                   \  \   /  /");

                }
                UpdateGrid(MyState, position, isHit);
            }
            while (true);
        }

        private static void ShowGridStatus()
        {
            Console.WriteLine("My Fleet");
            DisplayGrid(MyState);
            Console.WriteLine("Enemy Fleet");
            DisplayGrid(EnemyState);
            Console.WriteLine("#################################################");
        }

        private static void UpdateGrid(List<PositionState> positionStates, Position position, bool isHit)
        {
            var positionState = positionStates.FirstOrDefault(ps => ps.Position.Equals(position));
            if (positionState == null) return;

            positionState.Status = isHit ? State.Hit : State.Miss;
            positionState.Color = isHit ? ConsoleColor.Red : ConsoleColor.Yellow;
        }

        public static Position ParsePosition(string input)
        {
            var letter = (Letters)Enum.Parse(typeof(Letters), input.ToUpper().Substring(0, 1));
            var number = int.Parse(input.Substring(1, 1));
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

            Console.WriteLine("Please position your fleet (Game board size is from A to H and 1 to 8) :");

            foreach (var ship in myFleet)
            {
                Console.WriteLine();
                Console.WriteLine("Please enter the positions for the {0} (size: {1})", ship.Name, ship.Size);

                Console.WriteLine("Enter start position (e.g. A3):");
                var position = Console.ReadLine();
                while (!ship.AddPosition(position))
                {
                    Console.WriteLine("Position not valid. Enter position (e.g. A3):");
                    position = Console.ReadLine();
                }
                Console.WriteLine("Enter end position (e.g. A3):");
                position = Console.ReadLine();
                while (!ship.AddPosition(position))
                {
                    Console.WriteLine("Position not valid. Enter position (e.g. A3):");
                    position = Console.ReadLine();
                }
            }
        }

        private static void InitializeMyFleetStatic()
        {
            myFleet = GameController.InitializeShips().ToList();

            myFleet[0].AddPosition(new Position { Column = Letters.B, Row = 4 });
            myFleet[0].AddPosition(new Position { Column = Letters.B, Row = 8 });

            myFleet[1].AddPosition(new Position { Column = Letters.E, Row = 6 });
            myFleet[1].AddPosition(new Position { Column = Letters.E, Row = 9 });

            myFleet[2].AddPosition(new Position { Column = Letters.A, Row = 3 });
            myFleet[2].AddPosition(new Position { Column = Letters.C, Row = 3 });

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

            enemyFleet[1].AddPosition(new Position { Column = Letters.E, Row = 6 });
            enemyFleet[1].AddPosition(new Position { Column = Letters.E, Row = 9 });

            enemyFleet[2].AddPosition(new Position { Column = Letters.A, Row = 3 });
            enemyFleet[2].AddPosition(new Position { Column = Letters.C, Row = 3 });

            enemyFleet[3].AddPosition(new Position { Column = Letters.F, Row = 8 });
            enemyFleet[3].AddPosition(new Position { Column = Letters.H, Row = 8 });

            enemyFleet[4].AddPosition(new Position { Column = Letters.C, Row = 5 });
            enemyFleet[4].AddPosition(new Position { Column = Letters.C, Row = 6 });
        }
    }
}

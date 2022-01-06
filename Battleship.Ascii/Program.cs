
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
        private static void DisplayGrid(IEnumerable<Ship> ships)
        {
            Console.WriteLine("My Fleet");
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
                

                var hitMissMessage = isHit ? "has hit your ship !" : "miss";
                var hitMissMessageColor = isHit ? ConsoleColor.Red : ConsoleColor.Blue;
                WriteMessage($"Computer shot in {position.Column}{position.Row} and {hitMissMessage}", hitMissMessageColor);
                
                if (isHit)
                {
                    DrawExplosion();

                }
            }
            while (true);
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

        private static void InitializeEnemyFleet()
        {
            enemyFleet = GameController.InitializeShips().ToList();

            enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 4 });
            //enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 5 });
            //enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 6 });
            //enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 7 });
            enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 8 });

            enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 6 });
            //enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 7 });
            //enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 8 });
            enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 9 });

            enemyFleet[2].Positions.Add(new Position { Column = Letters.A, Row = 3 });
            //enemyFleet[2].Positions.Add(new Position { Column = Letters.B, Row = 3 });
            enemyFleet[2].Positions.Add(new Position { Column = Letters.C, Row = 3 });

            enemyFleet[3].Positions.Add(new Position { Column = Letters.F, Row = 8 });
            //enemyFleet[3].Positions.Add(new Position { Column = Letters.G, Row = 8 });
            enemyFleet[3].Positions.Add(new Position { Column = Letters.H, Row = 8 });

            enemyFleet[4].Positions.Add(new Position { Column = Letters.C, Row = 5 });
            enemyFleet[4].Positions.Add(new Position { Column = Letters.C, Row = 6 });
        }
    }

}

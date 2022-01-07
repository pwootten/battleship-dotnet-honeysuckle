
namespace Battleship.Ascii
{
    /*
     * Fireworks Shamelessly stolen from SirJosh3917
     * https://github.com/SirJosh3917/Fireworks/
     * */
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Battleship.Ascii.TelemetryClient;
    using Battleship.GameController;
    using Battleship.GameController.Contracts;
    using Fireworks;

    public class Program
    {
        private static List<Ship> myFleet;

        public static List<Ship> enemyFleet;

        private static int enemyFleetCount = 0;

        private static ITelemetryClient telemetryClient;

        private static ConsoleColor mainColor = ConsoleColor.Yellow;

        private static List<PositionState> MyState;
        private static List<PositionState> EnemyState;

        private static readonly List<Position> MyGuesses = new List<Position>();
        private static readonly List<Position> ComputerGuesses = new List<Position>();

        static void Main()
        {
            //DisplayWinnerMessage();
            //Thread.Sleep(3000);
            //DisplayFireworks().Wait();
            //return;
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
                //DisplayGrid(EnemyFleetState);
                //Console.ReadLine();
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
                var input = Console.ReadLine();
                if (input == "/win")
                {
                    Console.Clear();
                    DisplayWinnerMessage();
                    Thread.Sleep(3000);
                    DisplayFireworks().Wait();
                }
                var position = ParsePosition(input);

                while (position == null || MyGuesses.Contains(position))
                {
                    Console.WriteLine("Position is outside the playing field, is not valid or you have already used this position :");
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

                    if (enemyFleet.All(s => s.IsDestroyed))
                    {
                        DisplayWinnerMessage();
                        Thread.Sleep(3000);
                        DisplayFireworks().Wait();
                    }
                }
                else
                {
                    WriteMessage("Miss", ConsoleColor.Blue);
                }

                position = GetRandomPosition();
                isHit = GameController.CheckIsHit(myFleet, position);
                UpdateGrid(MyState, position, isHit);
                telemetryClient.TrackEvent("Computer_ShootPosition", new Dictionary<string, string>() { { "Position", position.ToString() }, { "IsHit", isHit.ToString() } });

                WriteBreak();

                var hitMissMessage = isHit ? "has hit your ship !" : "miss";
                var hitMissMessageColor = isHit ? ConsoleColor.Red : ConsoleColor.Blue;

                if (isHit)
                {
                    DrawExplosion();
                }

                if (myFleet.All(s => s.IsDestroyed))
                {
                    DisplayLoserMessage();
                    Console.WriteLine("Press the enter key to continue");
                    Console.ReadLine();
                    break;
                }

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
            Console.WriteLine($"Computer Guesses: {string.Join(", ", ComputerGuesses.Select(g => $"{g.Column}{g.Row}"))}");
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
            //InitializeMyFleetStatic();
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
                var isValidShip = false;
                do
                {
                    Console.WriteLine();
                    Console.WriteLine("Please enter the positions for the {0} (size: {1})", ship.Name, ship.Size);
                    WriteBreak();

                    Console.WriteLine("Enter start position (e.g. A3):");
                    var startPosition = ParsePosition(Console.ReadLine());
                    WriteBreak();
                    while (startPosition == null || myFleet.SelectMany(s => s.Positions).Contains(startPosition))
                    {
                        Console.WriteLine("Start Position not valid. Enter position (e.g. A3):");
                        startPosition = ParsePosition(Console.ReadLine());
                        WriteBreak();
                    }
                    Console.WriteLine("Enter end position (e.g. A3):");
                    var endPosition = ParsePosition(Console.ReadLine());
                    WriteBreak();
                    while (endPosition == null || myFleet.SelectMany(s => s.Positions).Contains(endPosition))
                    {
                        Console.WriteLine("End Position not valid. Enter position (e.g. A3):");
                        endPosition = ParsePosition(Console.ReadLine());
                        WriteBreak();
                    }

                    if (DoesIntersect(startPosition, endPosition, ship.Size))
                    {
                        Console.WriteLine("Ship Position overlaps with another ship, please re-enter positions");
                    }
                    else
                    {
                        ship.AddPosition(startPosition);
                        if (!ship.AddPosition(endPosition))
                        {
                            Console.WriteLine("Invalid Size");
                            ship.Positions.Clear();
                        } else
                        {
                            isValidShip = true;
                        }
                    }
                } while (!isValidShip);
            }
        }

        private static bool DoesIntersect(Position startPosition, Position endPosition, int size)
        {
            var proposedShipPositions = new List<Position>();

            if (startPosition.Row == endPosition.Row)
            {
                if (Math.Abs(startPosition.Column - endPosition.Column) != size) return true;

                var start = (startPosition.Column > endPosition.Column) ? endPosition.Column : startPosition.Column;
                for (int i = (int)start + 1; i < size + (int)start; i++)
                {
                    proposedShipPositions.Add(new Position { Column = (Letters)i, Row = startPosition.Row });
                }
            }
            if (startPosition.Column == endPosition.Column)
            {
                if (Math.Abs(startPosition.Row - endPosition.Row) == size) return true;

                var start = (startPosition.Row > endPosition.Row) ? endPosition.Row : startPosition.Row;
                for (int i = start + 1; i < size + start; i++)
                {
                    proposedShipPositions.Add(new Position { Column = startPosition.Column, Row = i });
                }
            }

            return myFleet.SelectMany(s => s.Positions).Intersect(proposedShipPositions).Any();
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
                bool placed = false;

                while (!placed)
                {
                    Position startPosition;
                    do
                    {
                        var letter = (Letters)random.Next(lines);
                        var number = random.Next(1, rows);
                        startPosition = new Position(letter, number);

                    } while (enemyFleet.SelectMany(f => f.Positions).Contains(startPosition));

                    placed = placeComputerShip(startPosition, ship.Size);
                }
            }

        }

        private static bool placeComputerShip(Position startPosition, int size)
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
                                return true;
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
                                return true;
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
                                return true;
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
                                return true;
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

            return false;
        }

        private static void setShipPosition(List<Position> positionList)
        {
            foreach (Position pstn in positionList)
            {
                enemyFleet[enemyFleetCount].Positions.Add(pstn);
            }

            enemyFleetCount++;
        }
        private static void DisplayWinnerMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@" __      __       .__  .__    ________                         ._._._.");
            Console.WriteLine(@"/  \    /  \ ____ |  | |  |   \______ \   ____   ____   ____   | | | |");
            Console.WriteLine(@"\   \/\/   // __ \|  | |  |    |    |  \ /  _ \ /    \_/ __ \  | | | |");
            Console.WriteLine(@" \        /\  ___/|  |_|  |__  |    `   (  <_> )   |  \  ___/   \|\|\|");
            Console.WriteLine(@"  \__/\  /  \___  >____/____/ /_______  /\____/|___|  /\___  >  ______");
            Console.WriteLine(@"       \/       \/                    \/            \/     \/   \/\/\/");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"_____.___.                                       __  .__                     .__                            ");
            Console.WriteLine(@"\__  |   | ____  __ __  _____ _______   ____   _/  |_|  |__   ____   __  _  _|__| ____   ____   ___________ ");
            Console.WriteLine(@" /   |   |/  _ \|  |  \ \__  \\_  __ \_/ __ \  \   __\  |  \_/ __ \  \ \/ \/ /  |/    \ /    \_/ __ \_  __ \");
            Console.WriteLine(@" \____   (  <_> )  |  /  / __ \|  | \/\  ___/   |  | |   Y  \  ___/   \     /|  |   |  \   |  \  ___/|  | \/");
            Console.WriteLine(@" / ______|\____/|____/  (____  /__|    \___  >  |__| |___|  /\___  >   \/\_/ |__|___|  /___|  /\___  >__|   ");
            Console.WriteLine(@" \/                          \/            \/             \/     \/                  \/     \/     \/       ");
        }

        private static void DisplayLoserMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"__________             .___ .____                   __   ._._._.");
            Console.WriteLine(@"\______   \_____     __| _/ |    |    __ __   ____ |  | _| | | |");
            Console.WriteLine(@" |    |  _/\__  \   / __ |  |    |   |  |  \_/ ___\|  |/ / | | |");
            Console.WriteLine(@" |    |   \ / __ \_/ /_/ |  |    |___|  |  /\  \___|    < \|\|\|");
            Console.WriteLine(@" |______  /(____  /\____ |  |_______ \____/  \___  >__|_ \______");
            Console.WriteLine(@"        \/      \/      \/          \/           \/     \/\/\/\/");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"_____.___.              .__                       ");
            Console.WriteLine(@"\__  |   | ____  __ __  |  |   ____  ______ ____  ");
            Console.WriteLine(@" /   |   |/  _ \|  |  \ |  |  /  _ \/  ___// __ \ ");
            Console.WriteLine(@" \____   (  <_> )  |  / |  |_(  <_> )___ \\  ___/ ");
            Console.WriteLine(@" / ______|\____/|____/  |____/\____/____  >\___  >");
            Console.WriteLine(@" \/                                     \/     \/ ");
        }

        public static Task DisplayFireworks()
        {
            Console.CursorVisible = false;

            var em = new EntityManager();

            var rng = new Random();

            for (int i = 0; i < 75; i++)
            {
                em.Spawn(new Firework(rng));
            }

            return em.Run(100);
        }
    }
}

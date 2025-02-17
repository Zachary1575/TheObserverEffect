using System; // Math, Random is within System
using System.Threading;
using System.Threading.Tasks;

using System.Collections.Generic;

// Custom Namespaces
using basicRL;

class Program
{
    static Random random = new Random();

    static int gridWidth = 20;
    static int gridHeight = 20;
    static int playerX = random.Next(0, 19 + 1);
    static int playerY = random.Next(0, 19 + 1);
    static int computerX = 15;
    static int computerY = 15;
    static bool exitGame = false;

    static int turn = 0; // Starts at turn 0.

    static QLearning qlearning = null;
    static bool ping = false;
    static int decoyRoundStart = -1;
    static int decoyCooldown = 0;
    static bool[] decoyState = [true, false, false]; // [0] - Avaliable, [1] - Active, [2] - On Cooldown
    static List<int[]> decoyArray = new List<int[]>();
    static int quantumFreezeRoundStart = -1;
    static int quantumFreezeCooldown = 0;
    static bool[] quantumFreezeState = [true, false, false]; // [0] - Avaliable, [1] - Active, [2] - On Cooldown

    static bool alarm = false;

    static int[,] gameBoardState = new int[gridHeight, gridWidth]; // m x n matrix.

    static async Task Main()
    {
        Console.WriteLine("Press arrow keys to move. Press Q to exit.");
        Console.CursorVisible = false;
        qlearning = new QLearning();
        while (!exitGame)
        {
            turn += 1; // Starts at Round 1.
            DrawGrid(); // Draw Grid
            ProximityDetector(); // Detects proximity near player.
            PrintStats(); // Print relevant information
            await WaitForKeyPress(); // Waits for a key press or condition
            Reset(); // Reset some things...
            HandleInput(); // Player Move
            ComputerMove(); // AI Move
        }
    }

    static void Reset() {
        if (ping) {
            ping = false;
        }

        if (decoyState[1] && (turn - decoyRoundStart) >= 2) {
            // Reset each decoy
            foreach (int[] decoy in decoyArray) {
                gameBoardState[decoy[0], decoy[1]] = 0;
            }
            decoyState[1] = false;
            decoyState[2] = true;

            decoyArray.Clear();
            decoyCooldown = 3;
        } else if (decoyState[2] && decoyCooldown == 0) {
            decoyState[2] = false;
            decoyState[0] = true;
        }

        if (quantumFreezeState[1] && (turn - quantumFreezeRoundStart) >= 2) {
            quantumFreezeState[1] = false;
            quantumFreezeState[2] = true;
        } else if (quantumFreezeState[2] && quantumFreezeCooldown == 0) {
            quantumFreezeState[2] = false;
            quantumFreezeState[0] = true;
        }
    }

    static void ComputerMove() {
        if (quantumFreezeState[1]) { // Quantum Freeze is active. Hostiles cannot move.
            quantumFreezeCooldown = 5;
            return;
        }  

        gameBoardState[computerX, computerY] = 0;

        int[] optimalCoord = qlearning.getOptimalCoordinate(playerX, playerY);
        computerX = optimalCoord[0];
        computerY = optimalCoord[1];

        return;
    }

    /*
    * Uses Manhattan Distance to calculate the distance between AI and you.
    * Let (x_1, y_1) be the Player. Let (x_2, y_2) be the AI.
    */
    static void ProximityDetector() {
        int distance = Math.Abs(playerX - computerX) + Math.Abs(playerY - computerY);
        if (distance <= 2) {
            alarm = true;
        } else {
            alarm = false;
        }
    }

    static string PrintAbilityState(bool[] abilityStateArray,ref int cooldown) {
        if (abilityStateArray[0]) {
            return "[On Standby]";
        } else if (abilityStateArray[1]) {
            return "[Active]";
        }

        return $"[N/A] ({cooldown--})";
    }

    static void PrintStats() {
        Console.Write("-------------------------------------------");
        Console.WriteLine();
        Console.Write($"Player Coordinates: ({playerX}, {playerY})");
        Console.WriteLine();
        Console.Write($"Computer Coordinates: ({computerX}, {computerY})");
        Console.WriteLine();
        Console.Write("-------------------------------------------");
        Console.WriteLine();
        Console.Write($"Dimensional Ping [P]: {(ping ? "[Pinging....]" : "[On Standby]")}");
        Console.WriteLine();
        Console.Write($"False Pings [R]: {PrintAbilityState(decoyState, ref decoyCooldown)}");
        Console.WriteLine();
        Console.Write($"Quantum Freeze [F]: {PrintAbilityState(quantumFreezeState, ref quantumFreezeCooldown)}");
        Console.WriteLine();
        Console.Write("-------------------------------------------");
        Console.WriteLine();
        Console.Write($"Proximity Alarm: [{(alarm ? "UNKNOWN ENTITY NEARBY! EVADE!" : "Clear")}]");
    }

    static void DrawGrid()
    {
        Console.Clear();

        // Update the player and computer locations.
        gameBoardState[playerX, playerY] = 1;
        gameBoardState[computerX, computerY] = 2;

        // If we have active decoys, draw the active decoys.
        if (decoyState[1]) {    
            foreach (int[] decoy in decoyArray) {
                gameBoardState[decoy[0], decoy[1]] = 3;
            }
        }

        // Write the Player & Computer. Draw the Board.
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {   
                int currentState = gameBoardState[x, y];
                if (currentState == 1) { // Write the Player
                    Console.Write("P ");
                } else if ((currentState == 2) && ping){ // Write the Computer
                    Console.Write("? ");
                } else if ((currentState == 3) && decoyState[1]) { // Write the Decoys
                    Console.Write("D ");
                } else  {    
                    Console.Write(". ");
                }
            }
            Console.WriteLine();
        }
    }

    static async Task WaitForKeyPress() {
        while (!Console.KeyAvailable && !exitGame) {
            await Task.Delay(100);
        }
    }

    static void HandleInput()
    {
        if (!Console.KeyAvailable) return;

        ConsoleKey key = Console.ReadKey(true).Key;
        switch (key)
        {
            case ConsoleKey.UpArrow:
                if (playerY > 0) {
                    gameBoardState[playerX, playerY] = 0;
                    playerY--;
                }
                break;
            case ConsoleKey.DownArrow:
                if (playerY < gridHeight - 1) {
                    gameBoardState[playerX, playerY] = 0;
                    playerY++;
                } 
                break;
            case ConsoleKey.LeftArrow:
                if (playerX > 0) {
                    gameBoardState[playerX, playerY] = 0;
                    playerX--;
                } 
                break;
            case ConsoleKey.RightArrow:
                if (playerX < gridWidth - 1) {
                    gameBoardState[playerX, playerY] = 0;
                    playerX++;
                } 
                break;
            case ConsoleKey.W:
                if (computerY > 0) {
                    gameBoardState[computerX, computerY] = 0;
                    computerY--;
                } 
                break;
            case ConsoleKey.S:
                if (computerY < gridHeight - 1) {
                    gameBoardState[computerX, computerY] = 0;
                    computerY++;
                } 
                break;
            case ConsoleKey.A:
                if (computerX > 0) {
                    gameBoardState[computerX, computerY] = 0;
                    computerX--;
                } 
                break;
            case ConsoleKey.D:
                if (computerX < gridWidth - 1) {
                    gameBoardState[computerX, computerY] = 0;
                    computerX++;
                } 
                break;
            case ConsoleKey.P: // Pings for one round
                ping = true;
                break;
            case ConsoleKey.R: // Decoys last for 2 rounds.
                if (decoyState[0]) {
                    decoyState[0] = false; // Sets decoy avaliability to false.
                    decoyState[1] = true; // Sets decoy active.
                    decoyRoundStart = turn;
                    // Populates say like 5 decoys (unmoving)
                    for (int i = 0; i < 5; i++) {
                        int[] randDecoyCoord = [0, 0];
                        randDecoyCoord[0] = random.Next(0, 19 + 1);
                        randDecoyCoord[1] = random.Next(0, 19 + 1);
                        decoyArray.Add(randDecoyCoord);
                    }
                }
                break;
            case ConsoleKey.F:
                if (quantumFreezeState[0]) {
                    quantumFreezeState[0] = false; // Sets quantum freeze avaliability to false.
                    quantumFreezeState[1] = true; // Sets quantum freeze active.
                    quantumFreezeRoundStart = turn;
                }
                break;
            case ConsoleKey.Q:
                exitGame = true; // Exit the loop when Q is pressed
                break;
            default:
                break;
        }
    }
}

using Go;
using ScenarioCollection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cgs = ConsoleGoSolver.Program;

namespace LeelaSharp
{
    /// <summary>
    /// Integration of neural network from Leela Zero into GoSolver.
    /// Leela Zero code: https://github.com/leela-zero/leela-zero
    /// </summary>
    class Program
    {
        static Boolean playFullGame = false;
        static void Main(string[] args)
        {
            try
            {
                //start process for leelaz
                Process process = new Process();
                process.StartInfo.FileName = @"..\..\..\leelazero\leelaz";
                process.StartInfo.Arguments = @"--gtp --lagbuffer 0 --weights ..\..\lznetwork.gz";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += new DataReceivedEventHandler(MonteCarloGame.MyProcess_OutputDataReceived);
                process.ErrorDataReceived += new DataReceivedEventHandler(MonteCarloGame.MyProcess_OutputDataReceived);
                Boolean processStarted = process.Start();
                MonteCarloGame.inputWriter = process.StandardInput;
                Boolean firstRun = true;
                while (true)
                {
                    Game g;
                    if (playFullGame)
                        g = new Game(new GameInfo());
                    else
                        g = cgs.GetScenarioGame();
                    if (firstRun)
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        firstRun = false;
                    }
                    while (true)
                    {
                        Boolean completed = PlayOneRound(g);
                        if (!completed)
                            break;
                        Console.WriteLine("\nDo you want to play the scenario again (y/n)?");
                        String play_again = Console.ReadLine();
                        if (play_again.ToLower() != "y")
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        static Boolean PlayOneRound(Game game)
        {
            Game g = new Game(game);
            Console.WriteLine("{0}", g.Board);
            MonteCarloGame.SetupLeelazGame(g);
            MonteCarloGame.inputWriter.WriteLine("showboard");
            MonteCarloGame.inputWriter.WriteLine("heatmap");

            String inputText;
            do
            {
                inputText = Console.ReadLine();
                if (inputText.Length == 0)
                    break;
                if (!playFullGame)
                {
                    if (inputText == "s" || inputText == "search")
                    {
                        MonteCarloGame.useLeelaZero = true;
                        cgs.SearchAnswer(g);
                        return true;
                    }
                    else if (inputText == "a" || inputText == "answer")
                        cgs.GetAnswer(g);
                    else if (inputText == "m" || inputText == "mapping")
                    {
                        MonteCarloGame.useLeelaZero = true;
                        MonteCarloMapping.MapScenario(g);
                        Console.WriteLine("Mapping completed.");
                    }
                    else if (inputText == "v" || inputText == "verification")
                    {
                        MonteCarloGame.useLeelaZero = true;
                        int error = MappingVerification.VerifyScenario(g);
                        Console.WriteLine("Verification completed. Errors: " + error);
                    }
                }
                if (inputText == "h" || inputText == "help")
                    GetHelp();
                //enter command for leelaz
                MonteCarloGame.inputWriter.WriteLine(inputText);
            } while (true);
            return true;
        }

        static void GetHelp()
        {
            Console.WriteLine("GoSolver commands: ");
            Console.WriteLine("s - Search answer");
            Console.WriteLine("a - Get answer");
            Console.WriteLine("m - Map scenario");
            Console.WriteLine("v - Verify scenario");

            List<String> commandList = new List<String> {
    "protocol_version",
    "name",
    "version",
    "quit",
    "known_command",
    "list_commands",
    "boardsize",
    "clear_board",
    "komi",
    "play",
    "genmove",
    "showboard",
    "undo",
    "final_score",
    "final_status_list",
    "time_settings",
    "time_left",
    "fixed_handicap",
    "last_move",
    "move_history",
    "clear_cache",
    "place_free_handicap",
    "set_free_handicap",
    "loadsgf",
    "printsgf",
    "kgs-genmove_cleanup",
    "kgs-time_settings",
    "kgs-game_over",
    "heatmap",
    "lz-analyze",
    "lz-genmove_analyze",
    "lz-memory_report",
    "lz-setoption",
    "gomill-explain_last_move"
            };
            Console.WriteLine("\nLeelaz commands: ");
            foreach (String command in commandList)
                Console.WriteLine(command);

            Console.WriteLine("\nTo make move at r17 for black, enter \"play B r17\"");
            Console.WriteLine("To generate computer move for white, enter \"genmove W\"");
            Console.WriteLine("To show board, enter \"showboard\"");
            Console.WriteLine("To show neural network values, enter \"heatmap\"");
        }

    }
}

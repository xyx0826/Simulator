using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simulator
{
    internal class Simulator
    {
        private const int PlayerCount = 4;

        private const int TargetSimulationCount = 2000000;

        private static int _simulationCount = 0;

        private static List<SimulationResult> _simulationResults
            = new List<SimulationResult>(TargetSimulationCount);

        private static object _consoleLock = new object();

        /// <summary>
        /// Entry method.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Pool.Initialize();
            var simulator = new Simulator();
            Console.WriteLine($"Using {PlayerCount} players in {TargetSimulationCount} simulations.\n");
            simulator.RunSimulations();
        }

        public void RunSimulations()
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"Simulation started at {DateTime.Now}.");
            Console.Write("Completed 0%.");
            Parallel.For(0, TargetSimulationCount, 
                index =>
                {
                    lock (_simulationResults)
                    {
                        _simulationResults.Add(new Simulation(PlayerCount).Simulate());
                    }
                    if (Interlocked.Add(ref _simulationCount, 1) % 10000 == 0)
                    {
                        lock (_consoleLock)
                        {
                            var progress = 100.0 * _simulationCount / TargetSimulationCount;
                            Console.SetCursorPosition(10, Console.CursorTop);
                            Console.Write($"{progress.ToString("#.00")}%.\t");
                        }
                    }
                });

            stopwatch.Stop();
            Console.WriteLine($"\nSimulation completed at {DateTime.Now}; elapsed {stopwatch.Elapsed}.\n");

            var resultDict = new Dictionary<char, int>();
            foreach (var history in _simulationResults)
            {
                if (!resultDict.ContainsKey(history.Winner))
                {
                    resultDict[history.Winner] = 0;
                }
                resultDict[history.Winner]++;
            }

            var orderedResults = resultDict.OrderBy(x => x.Key);
            Console.WriteLine("Winner\tTimes\tRate");
            foreach (var result in orderedResults)
            {
                Console.WriteLine($"{result.Key}\t{result.Value}" +
                    $"\t{(double)result.Value / TargetSimulationCount * 100}%");
            }

            Console.WriteLine($"Average turn count is " +
            $"{_simulationResults.Select(x => x.Turns).Average()} turns.\n");

            Console.WriteLine("Appending to output.csv...");
            using (var writer = new StreamWriter("./output.csv", true))
            {
                writer.WriteLine("turns,isDraw,winner");
                foreach (var result in _simulationResults)
                {
                    writer.WriteLine(result.ToCsv());
                }
            }

            Console.WriteLine("Results written.");
            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;

namespace Simulator
{
    internal class Simulation
    {
        private const int TargetScore = 30;

        private const string PlayerTags = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public int PlayerCount = 0;

        public int Turns = 0;

        public int PlayerTakingTurn = 0;

        private SimulationResult _result = new SimulationResult(false);

        private List<Player> _players = new List<Player>();

        private static readonly Random _random = new CryptoRandom();

        public Simulation(int playerCount)
        {
            // Console.WriteLine($"Creating a new simulation of {playerCount} players.");
            PlayerCount = playerCount;
            PlayerTakingTurn = _random.Next(PlayerCount);
            InitializePlayers();
        }

        public void InitializePlayers()
        {
            var cardsDealt = Pool.GetCards(PlayerCount);
            for (int playerIndex = 0; playerIndex < PlayerCount; playerIndex++)
            {
                _players.Add(
                    new Player(
                        PlayerTags[playerIndex],
                        cardsDealt[playerIndex]));
            }
        }

        public SimulationResult Simulate()
        {
            while (true)
            {
                Turns++;
                if (PlayerTakingTurn == PlayerCount)
                    PlayerTakingTurn = 0;

                // Console.WriteLine($"Turn {Turns}: Player #{PlayerTakingTurn} is taking turn.");
                // Console.WriteLine($"At this moment, scores: {String.Join(", ", PlayerScores)}");
                var decision = _players[PlayerTakingTurn]
                    .Turn(PlayerTakingTurn, PlayerScores);
                // Console.WriteLine($"\tDecision: {decision.Action} to {decision.TargetIndex} for {decision.Points}\n");
                switch (decision.Action)
                {
                    case ActionType.Award:
                        _players[PlayerTakingTurn].Award(decision.Points);
                        break;
                    case ActionType.Penalize:
                        _players[decision.TargetIndex].Penalize(decision.Points);
                        break;
                }
                if (IsGameFinished) break;
                PlayerTakingTurn++;
            }

            if (Winner != '*')
            {
                _result.Turns = Turns;
                _result.Winner = Winner;
            }
            else
            {
                _result.Turns = Turns;
                _result.IsDraw = true;
            }
            // Console.WriteLine(_result);
            return _result;
        }

        public int[] PlayerScores
        {
            get
            {
                var scores = new int[PlayerCount];
                for (int i = 0; i < PlayerCount; i++)
                {
                    scores[i] = _players[i].Score;
                }
                return scores;
            }
        }

        public bool IsGameFinished
        {
            get
            {
                var allCardsPlayed = true;

                foreach (var player in _players)
                {
                    if (player.HasCardsLeft) allCardsPlayed = false;

                    if (player.Score >= TargetScore)
                    {
                        return true;
                    }
                }
                return allCardsPlayed;
            }
        }

        public char Winner
        {
            get
            {
                foreach (var player in _players)
                {
                    if (player.Score >= TargetScore)
                        return player.Tag;
                }
                return '*';
            }
        }
    }

    internal class SimulationResult
    {
        public int Turns;

        public char Winner;

        public bool IsDraw;

        public SimulationResult(bool isDraw = true)
        {
            Winner = '*';
            IsDraw = isDraw;
        }

        public SimulationResult(int turns, char winner)
        {
            Turns = turns;
            Winner = winner;
        }

        public string ToCsv()
        {
            return $"{Turns},{IsDraw},{Winner}";
        }

        public override string ToString()
        {
            return IsDraw ? $"Draw in {Turns} turns" : $"Winner: {Winner} won in {Turns} turns";
        }
    }
}

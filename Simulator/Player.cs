using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulator
{
    internal class Player
    {
        private const int PointThreshold = 20;

        private static readonly Random _random = new CryptoRandom();

        public char Tag;

        public int Score = 0;

        public List<int> PositiveCards = new List<int>();

        public List<int> NegativeCards = new List<int>();

        public List<int> QueuedCards = new List<int>();

        public Player(char tag, List<int> cards)
        {
            Tag = tag;

            DealInitialCards(cards);
        }

        private void DealInitialCards(List<int> cards)
        {
            // Console.WriteLine($"Player {Tag} received card deck: {String.Join(", ", cards)}");
            QueuedCards = cards;

            for (int i = 0; i < 5; i++)
            {
                DealCard();
            }
            // Console.WriteLine($"\t{Tag}: initial cards are dealed.");
        }

        public bool HasPositiveCards
        {
            get { return PositiveCards.Count > 0; }
        }

        public bool HasNegativeCards
        {
            get { return NegativeCards.Count > 0; }
        }

        public bool HasCardsLeft
        {
            get { return HasPositiveCards || HasNegativeCards; }
        }

        public PlayerAction Turn(int selfIndex, int[] scores)
        {
            var action = new PlayerAction();

            // Console.WriteLine($"\t{Tag}: my turn!");
            // Console.WriteLine($"\t{Tag}: I have [{String.Join(", ", PositiveCards)}] "
            //     + $"and [{String.Join(", ", NegativeCards)}] before playing.");

            if (HasCardsLeft)
            {
                // Cards available, make decisions
                var leaderIndex = SelectLeaderIndex(selfIndex, scores);
                if (HasPositiveCards && PositiveCards.Max() + Score == 30)
                {
                    // Priority win situation
                    action.Action = ActionType.Award;
                    action.TargetIndex = selfIndex;
                    action.Points = PositiveCards.Max();
                    PositiveCards.Remove(action.Points);
                }
                else if (HasNegativeCards
                    && scores[leaderIndex] >= PointThreshold)
                {
                    // Has negative cards, found hostile leader, penalize
                    // Console.WriteLine($"\t{Tag}: I have negative cards for a leader.");
                    action.Action = ActionType.Penalize;
                    action.TargetIndex = leaderIndex;
                    action.Points = NegativeCards.Min();
                    NegativeCards.Remove(action.Points);
                }
                else if (HasPositiveCards)
                {
                    // Cannot penalize anyone, has positive cards, award
                    // Console.WriteLine($"\t{Tag}: no suitable leader found, use positive card.");
                    action.Action = ActionType.Award;
                    action.TargetIndex = selfIndex;
                    action.Points = PositiveCards.Max();
                    PositiveCards.Remove(action.Points);
                }
                else if (HasNegativeCards)
                {
                    // Cannot penalize anyone, has no positive cards, penalize
                    // Console.WriteLine($"\t{Tag}: no positive cards, play negative cards.");
                    action.Action = ActionType.Penalize;
                    action.TargetIndex = leaderIndex;
                    action.Points = NegativeCards.Min();
                    NegativeCards.Remove(action.Points);
                }
                // Console.WriteLine($"\t{Tag}: playing card {action.Points}.");
                DealCard();
            }
            else
            {
                action.Action = ActionType.Skip;
                // Console.WriteLine($"\t{Tag}: no cards to play. Skipping.");
            }
                return action;
        }

        public void Award(int points)
        {
            Score += points;
        }

        public void Penalize(int points)
        {
            Score = (Score + points < 0) ? 0 : Score + points;
        }

        private void DealCard()
        {
            if (QueuedCards.Count == 0) return;

            var card = QueuedCards[0];
            QueuedCards.RemoveAt(0);

            if (card > 0) PositiveCards.Add(card);
            else NegativeCards.Add(card);
            // Console.WriteLine($"\t{Tag}: Drew new card {card}; Queued cards has {QueuedCards.Count} remained.");
            // Console.WriteLine($"\t{Tag}: I have [{String.Join(", ", PositiveCards)}] "
            //     + $"and [{String.Join(", ", NegativeCards)}] now.");
        }

        private int SelectLeaderIndex(int selfIndex, int[] scores)
        {
            var maxScore = selfIndex == 0 ? scores[1] : scores[0];
            var maxIndices = new List<int>();

            for (int i = 0; i < scores.Length; i ++)
            {
                if (scores[i] > maxScore && i != selfIndex)
                    maxScore = scores[i];
            }
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] == maxScore && i != selfIndex)
                    maxIndices.Add(i);
            }

            if (maxIndices.Count == 1) return maxIndices[0];
            else return maxIndices[_random.Next(maxIndices.Count)];
        }

        public override string ToString()
        {
            return $"Player {Tag}, Score: {Score}";
        }
    }

    internal class PlayerAction
    {
        public ActionType Action { get; set; }

        public int Points { get; set; }

        public int TargetIndex { get; set; }

        public PlayerAction()
        {
            Points = 0;
        }

        public override string ToString()
        {
            switch (Action)
            {
                case ActionType.Award:
                    return $"Award {Points} points to #{TargetIndex}";
                case ActionType.Penalize:
                    return $"Penalize {Points} points from #{TargetIndex}";
                case ActionType.Skip:
                    return $"Skip this round";
            }
            return $"Discarded";
        }
    }

    internal enum ActionType
    {
        Award,
        Penalize,
        Discard,
        Skip
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Simulator
{
    internal static class Pool
    {
        private static Random _random = new CryptoRandom();

        private static List<int> _sortedCards = new List<int>();

        private static List<int> _cards = new List<int>();

        public static int CardCount => _sortedCards.Count;

        public static void Initialize()
        {
            var rawCards = File.ReadAllText("./cards.csv").Split(',');
            foreach (var rawCard in rawCards)
            {
                Int32.TryParse(rawCard, out int cardValue);
                _sortedCards.Add(cardValue);
            }
            Console.WriteLine($"Found {_sortedCards.Count} valid cards.");
            Shuffle();
        }

        public static List<int>[] GetCards(int playerCount)
        {
            var cardsDealt = new List<int>[playerCount];
            int currentPlayer = _random.Next(playerCount);
            // Console.WriteLine($"Dealing cards starting from player #{currentPlayer}");

            for (int i = 0; i < playerCount; i ++)
            {
                cardsDealt[i] = new List<int>();
            }

            foreach (var card in _cards)
            {
                if (currentPlayer < 0) currentPlayer = playerCount - 1;
                cardsDealt[currentPlayer].Add(card);
                currentPlayer--;
            }

            Shuffle();
            return cardsDealt;
        }

        private static void Shuffle()
        {
            _cards = _sortedCards.OrderBy(
                x => _random.Next(Int32.MaxValue))
                .ToList();
        }
    }
}

using System.Collections.Concurrent;
using CheckersGameProject.GameLogic;

namespace CheckersGameProject.Api.Services
{
    public class GameService
    {
        private readonly ConcurrentDictionary<string, CheckersGame> _games = new();

        public string CreateGame(string p1Name, string p2Name)
        {
            var gameId = Guid.NewGuid().ToString();
            var game = new CheckersGame(p1Name, p2Name);
            game.StartGame();
            _games.TryAdd(gameId, game);
            return gameId;
        }

        public CheckersGame? GetGame(string gameId)
        {
            _games.TryGetValue(gameId, out var game);
            return game;
        }
    }
}
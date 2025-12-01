using System.Collections.Concurrent;
using CheckersGameProject.GameLogic;
using CheckersGameProject.Contracts;
using CheckersGameProject.Core;

namespace CheckersGameProject.Api.Services
{
    public class GameService
    {
        private readonly ConcurrentDictionary<string, CheckersGame> _games = new();
        private readonly ICheckersGameFactory _gameFactory;

        // Dependency Injection Factory
        public GameService(ICheckersGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
        }

        public ServiceResult<string> CreateGame(string p1Name, string p2Name)
        {
            if (string.IsNullOrWhiteSpace(p1Name) || string.IsNullOrWhiteSpace(p2Name))
                return ServiceResult<string>.Fail("Nama pemain tidak boleh kosong.");

            try 
            {
                var gameId = Guid.NewGuid().ToString();
                var game = _gameFactory.CreateGame(p1Name, p2Name); // Pakai Factory
                game.StartGame();

                if (!_games.TryAdd(gameId, game))
                    return ServiceResult<string>.Fail("Gagal membuat sesi game.");

                return ServiceResult<string>.Ok(gameId);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Fail($"Error: {ex.Message}");
            }
        }

        public ServiceResult<CheckersGame> GetGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId)) 
                return ServiceResult<CheckersGame>.Fail("Game ID kosong.");

            if (_games.TryGetValue(gameId, out var game))
                return ServiceResult<CheckersGame>.Ok(game);

            return ServiceResult<CheckersGame>.Fail("Game tidak ditemukan.");
        }
    }
}
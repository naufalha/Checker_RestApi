using Microsoft.AspNetCore.Mvc;
using CheckersGameProject.Api.Services;
using CheckersGameProject.Api.DTOs;
using CheckersGameProject.Core;
using CheckersGameProject.GameLogic;

namespace Checkers.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CheckersController : ControllerBase
    {
        private readonly GameService _gameService;

        public CheckersController(GameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Memulai permainan baru.
        /// </summary>
        /// <remarks>
        /// Membuat sesi game baru di server. Simpan **gameId** yang dikembalikan untuk request selanjutnya.
        /// </remarks>
        /// <param name="player1">Nama pemain Hitam (Default: Black)</param>
        /// <param name="player2">Nama pemain Merah (Default: Red)</param>
        /// <returns>Objek berisi Game ID</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult StartGame(string player1 = "Black", string player2 = "Red")
        {
            var gameId = _gameService.CreateGame(player1, player2);
            return Ok(new { GameId = gameId, Message = "Game Started" });
        }

        /// <summary>
        /// Mengambil status papan permainan.
        /// </summary>
        /// <remarks>
        /// Mengembalikan array 2D (8x8) papan, giliran pemain saat ini, dan status game (Play/Win/Draw).
        /// </remarks>
        /// <param name="gameId">ID Game (GUID)</param>
        [HttpGet("{gameId}/board")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetBoard(string gameId)
        {
            var game = _gameService.GetGame(gameId);
            if (game == null) return NotFound("Game not found");

            // --- FIX: KONVERSI 2D ARRAY ([,]) KE JAGGED ARRAY ([][]) ---
            // JSON tidak bisa membaca [,] jadi kita harus manual membuatnya jadi list of lists.
            
            var internalBoard = game.Board.Squares; // Ini tipe [,]
            var rows = internalBoard.GetLength(0);
            var cols = internalBoard.GetLength(1);
            
            // Kita buat array baru yang ramah JSON
            var jsonFriendlyBoard = new object[rows][];

            for (int y = 0; y < rows; y++)
            {
                jsonFriendlyBoard[y] = new object[cols];
                for (int x = 0; x < cols; x++)
                {
                    // Ambil cell asli
                    var cell = internalBoard[y, x];
                    
                    // Masukkan ke array baru
                    // Kita bisa langsung masukkan object cell, atau mapping manual jika perlu
                    jsonFriendlyBoard[y][x] = cell;
                }
            }
            // -----------------------------------------------------------

            return Ok(new 
            {
                // Kirim board yang sudah dikonversi
                Board = jsonFriendlyBoard, 
                
                CurrentPlayer = game.CurrentPlayer.Name,
                CurrentColor = game.CurrentPlayer.Color.ToString(),
                IsDoubleJumpActive = game.IsInDoubleJump,
                Status = game.GetWinner() != null ? "Win" : (game.IsDraw() ? "Draw" : "Play")
            });
        }

        /// <summary>
        /// Melakukan gerakan bidak.
        /// </summary>
        /// <remarks>
        /// Mengirim koordinat Asal dan Tujuan (0-7).
        /// API akan memvalidasi aturan (Fisika, Giliran, Wajib Makan, Double Jump).
        /// </remarks>
        /// <param name="gameId">ID Game</param>
        /// <param name="request">Data koordinat (FromX, FromY -> ToX, ToY)</param>
        [HttpPost("{gameId}/move")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult MakeMove(string gameId, [FromBody] MoveRequest request)
        {
            var game = _gameService.GetGame(gameId);
            if (game == null) return NotFound("Game not found");
            if (game.GetWinner() != null) return BadRequest("Game is already over.");

            try
            {
                var from = new Position(request.FromX, request.FromY);
                var to = new Position(request.ToX, request.ToY);
                var playerBefore = game.CurrentPlayer;
                
                // Eksekusi Logika Game
                game.ExecuteMove(from, to);
                
                // Cek apakah move sukses (giliran ganti ATAU ada double jump aktif)
                bool success = (game.CurrentPlayer != playerBefore) || game.IsInDoubleJump;

                if (!success)
                {
                   // Validasi ulang untuk pesan error yang akurat
                   if (!game.IsValidMove(from, to, out _, out _))
                   {
                       return BadRequest("Invalid Move (Melanggar aturan gerak / wajib makan)");
                   }
                }

                return Ok(new 
                { 
                    Message = "Move processed", 
                    NextTurn = game.CurrentPlayer.Name,
                    DoubleJumpActive = game.IsInDoubleJump,
                    Winner = game.GetWinner()?.Name
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Mendapatkan daftar langkah legal (Hint).
        /// </summary>
        /// <remarks>
        /// Mengembalikan daftar string gerakan yang valid untuk pemain saat ini.
        /// Berguna untuk debugging atau fitur highlight di frontend.
        /// </remarks>
        [HttpGet("{gameId}/hints")]
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetHints(string gameId)
        {
            var game = _gameService.GetGame(gameId);
            if (game == null) return NotFound("Game not found");

            var moves = new List<string>();
            var board = game.Board;
            var player = game.CurrentPlayer;
            bool mustCapture = game.AnyCaptureExists(player);

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var piece = board.Squares[y, x].Piece;
                    if (piece != null && piece.Color == player.Color)
                    {
                        var potentialMoves = game.GetPossibleMovesForPiece(piece, mustCapture);
                        foreach (var move in potentialMoves)
                        {
                            if (game.IsValidMove(move.Item1, move.Item2, out bool isCapture, out _))
                            {
                                moves.Add($"{move.Item1.X},{move.Item1.Y} -> {move.Item2.X},{move.Item2.Y} {(isCapture ? "[Capture]" : "")}");
                            }
                        }
                    }
                }
            }
            return Ok(moves);
        }
    }
}
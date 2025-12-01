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

        [HttpPost("start")]
        public IActionResult StartGame(string player1 = "Black", string player2 = "Red")
        {
            var result = _gameService.CreateGame(player1, player2);
            if (!result.Success) return BadRequest(new { Error = result.Message });
            return Ok(new { GameId = result.Data, Message = result.Message });
        }

        [HttpGet("{gameId}/board")]
        public IActionResult GetBoard(string gameId)
        {
            var result = _gameService.GetGame(gameId);
            if (!result.Success) return NotFound(new { Error = result.Message });

            var game = result.Data;
            
            // Konversi Array of Struct ke Array of Objects (untuk JSON)
            var internalBoard = game.Board.Squares;
            var rows = internalBoard.GetLength(0);
            var cols = internalBoard.GetLength(1);
            var jsonFriendlyBoard = new object[rows][];

            for (int y = 0; y < rows; y++)
            {
                jsonFriendlyBoard[y] = new object[cols];
                for (int x = 0; x < cols; x++)
                {
                    jsonFriendlyBoard[y][x] = internalBoard[y, x]; // Boxing struct terjadi di sini
                }
            }

            return Ok(new 
            {
                Board = jsonFriendlyBoard, 
                CurrentPlayer = game.CurrentPlayer.Name,
                CurrentColor = game.CurrentPlayer.Color.ToString(),
                IsDoubleJumpActive = game.IsInDoubleJump,
                Status = game.GetWinner() != null ? "Win" : (game.IsDraw() ? "Draw" : "Play")
            });
        }

        [HttpPost("{gameId}/move")]
        public IActionResult MakeMove(string gameId, [FromBody] MoveRequest request)
        {
            var result = _gameService.GetGame(gameId);
            if (!result.Success) return NotFound(new { Error = result.Message });

            var game = result.Data;
            if (game.GetWinner() != null) return BadRequest("Game is already over.");

            try
            {
                var from = new Position(request.FromX, request.FromY);
                var to = new Position(request.ToX, request.ToY);
                var playerBefore = game.CurrentPlayer;
                
                game.ExecuteMove(from, to);
                
                bool success = (game.CurrentPlayer != playerBefore) || game.IsInDoubleJump;

                if (!success)
                {
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

        [HttpGet("{gameId}/hints")]
        public IActionResult GetHints(string gameId)
        {
            var result = _gameService.GetGame(gameId);
            if (!result.Success) return NotFound(new { Error = result.Message });

            var game = result.Data;
            var moves = new List<object>();
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
                            if (game.IsValidMove(move.From, move.To, out bool isCapture, out _))
                            {
                                // MAPPING DTO KE JSON RESPONSE
                                moves.Add(new 
                                {
                                    fromX = move.From.X,
                                    fromY = move.From.Y,
                                    toX = move.To.X,
                                    toY = move.To.Y
                                });
                            }
                        }
                    }
                }
            }
            return Ok(moves);
        }
    }
}
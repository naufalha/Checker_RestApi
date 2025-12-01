using System;
using System.Collections.Generic;
using System.Linq;
using CheckersGameProject.Contracts;
using CheckersGameProject.Models;
using CheckersGameProject.Core;
using CheckersGameProject.Api.DTOs;

namespace CheckersGameProject.GameLogic
{
    public class CheckersGame
    {
        private IBoard _board;
        private Dictionary<IPlayer, List<IPiece>> _playerData;
        private List<IPlayer> _players;
        private IPlayer _currentPlayer;
        
        private Position _activePieceInJump; 
        private bool _isInMultipleJump; 
        private StatusType _status;
        
        private int _movesWithoutCapture;
        private const int _maxMovesWithoutProgress = 10; 

        // Properties
        public IBoard Board => _board;
        public IPlayer CurrentPlayer => _currentPlayer;
        public bool IsInDoubleJump => _isInMultipleJump;
        public Position ActivePiecePosition => _activePieceInJump;

        public CheckersGame(string player1Name, string player2Name)
        {
            _board = new CheckersBoard();
            _players = new List<IPlayer>
            {
                new Player(player1Name, PieceColor.Black),
                new Player(player2Name, PieceColor.Red)
            };

            _playerData = new Dictionary<IPlayer, List<IPiece>>();
            foreach (var p in _players) _playerData[p] = new List<IPiece>();

            _status = StatusType.NotStart;
        }

        public void StartGame()
        {
            InitializeBoardCells();
            _currentPlayer = _players[0];
            _status = StatusType.Play;
            _movesWithoutCapture = 0;
            _isInMultipleJump = false;
        }

        public void ExecuteMove(Position from, Position to) => MovePiece(from, to);
        public IPlayer GetCurrentPlayer() => _currentPlayer;
        public bool GetPlayerHasPiecesLeft(IPlayer player) => _playerData.ContainsKey(player) && _playerData[player].Count > 0;
        
        public IPlayer? GetWinner()
        {
            if (_status != StatusType.Win) return null;
            if (!CanMakeaMove(_players[0])) return _players[1];
            if (!CanMakeaMove(_players[1])) return _players[0];
            return null;
        }

        public bool IsDraw() => _status == StatusType.Draw;

        // REVISI: Mengembalikan List<MoveDto>
        public List<MoveDto> GetPossibleMovesForPiece(IPiece piece, bool requireCaptureOnly)
        {
            var moves = new List<MoveDto>();
            if (piece == null) return moves;

            int[] dYs = { 1, 1, -1, -1 };
            int[] dXs = { -1, 1, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int dy = dYs[i];
                int dx = dXs[i];

                if (piece.TypePiece == PieceType.Pawn)
                {
                    if (piece.Color == PieceColor.Black && dy < 0) continue;
                    if (piece.Color == PieceColor.Red && dy > 0) continue;
                }

                Position from = piece.Position;
                Position targetJump = new Position(from.X + (dx * 2), from.Y + (dy * 2));

                if (IsInternalValidMove(from, targetJump, out bool isCapture, out _) && isCapture)
                {
                    moves.Add(new MoveDto { From = from, To = targetJump, IsCapture = true });
                }

                if (!requireCaptureOnly)
                {
                    Position targetMove = new Position(from.X + dx, from.Y + dy);
                    if (IsInternalValidMove(from, targetMove, out isCapture, out _) && !isCapture)
                    {
                        moves.Add(new MoveDto { From = from, To = targetMove, IsCapture = false });
                    }
                }
            }
            return moves;
        }

        public bool IsValidMove(Position from, Position to, out bool isCapture, out Position capturedPos)
        {
            if (!IsInternalValidMove(from, to, out isCapture, out capturedPos)) return false;
            if (_isInMultipleJump && !from.Equals(_activePieceInJump)) return false;
            if (!_isInMultipleJump && AnyCaptureExists(_currentPlayer) && !isCapture) return false;
            return true;
        }

        public bool AnyCaptureExists(IPlayer player)
        {
            foreach (var piece in _playerData[player])
            {
                // Cek apakah ada langkah makan
                var moves = GetPossibleMovesForPiece(piece, true);
                if (moves.Count > 0) return true;
            }
            return false;
        }

        // --- PRIVATE METHODS (LOGIKA STRUCT) ---

        private void InitializeBoardCells()
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if ((x + y) % 2 != 0)
                    {
                        if (y < 3) AddPieceToPlayerData(_players[0], new CheckersPiece(PieceColor.Black, new Position(x, y)));
                        else if (y > 4) AddPieceToPlayerData(_players[1], new CheckersPiece(PieceColor.Red, new Position(x, y)));
                    }
                }
            }
        }

        private void AddPieceToPlayerData(IPlayer player, IPiece piece)
        {
            // AKSES LANGSUNG ARRAY (Struct Behavior)
            _board.Squares[piece.Position.Y, piece.Position.X].Piece = piece;
            _playerData[player].Add(piece);
        }

        private bool CanMakeaMove(IPlayer player)
        {
            if (!GetPlayerHasPiecesLeft(player)) return false;
            foreach (var piece in _playerData[player])
            {
                var moves = GetPossibleMovesForPiece(piece, false);
                foreach(var m in moves)
                {
                     if(IsValidMove(m.From, m.To, out _, out _)) return true;
                }
            }
            return false;
        }

        private void MovePiece(Position from, Position to)
        {
            if (IsValidMove(from, to, out bool isCapture, out Position capturedPos))
            {
                // LOGIKA STRUCT: Ambil referensi Piece, lalu update Array langsung
                var piece = _board.Squares[from.Y, from.X].Piece;

                _board.Squares[to.Y, to.X].Piece = piece;     // Pindah tujuan
                _board.Squares[from.Y, from.X].Piece = null;  // Hapus asal

                if (piece != null) piece.Position = to;

                bool wasPromoted = false;

                if (isCapture)
                {
                    HandleCapturedPiece(capturedPos);
                    
                    // Cek double jump
                    // Perlu cek ulang kondisi array terbaru
                    var hasMultiJump = GetPossibleMovesForPiece(piece, true).Count > 0;
                    if (hasMultiJump)
                    {
                        wasPromoted = CheckAndPromote(piece);
                        if (!wasPromoted)
                        {
                            _isInMultipleJump = true;
                            _activePieceInJump = to;
                            UpdateDrawCounters(true, false);
                            return;
                        }
                    }
                }

                _isInMultipleJump = false;
                if (piece != null && !wasPromoted) wasPromoted = CheckAndPromote(piece);
                UpdateDrawCounters(isCapture, wasPromoted);
                SwitchPlayer();
            }
        }

        private bool IsInternalValidMove(Position from, Position to, out bool isCapture, out Position capturedPos)
        {
            isCapture = false;
            capturedPos = new Position(-1, -1);

            if (_status != StatusType.Play) return false;
            if (to.X < 0 || to.X > 7 || to.Y < 0 || to.Y > 7) return false;
            
            // Baca struct aman
            var piece = _board.Squares[from.Y, from.X].Piece;
            if (piece == null || piece.Color != _currentPlayer.Color) return false;
            if (_board.Squares[to.Y, to.X].Piece != null) return false;

            int dx = to.X - from.X;
            int dy = to.Y - from.Y;
            if (Math.Abs(dx) != Math.Abs(dy)) return false;

            if (piece.TypePiece == PieceType.Pawn)
            {
                if (piece.Color == PieceColor.Black && dy < 0) return false;
                if (piece.Color == PieceColor.Red && dy > 0) return false;
            }

            int absDy = Math.Abs(dy);
            if (absDy == 1) return true;

            if (absDy == 2)
            {
                int midX = from.X + (dx / 2);
                int midY = from.Y + (dy / 2);
                var midPiece = _board.Squares[midY, midX].Piece;

                if (midPiece != null && midPiece.Color != piece.Color)
                {
                    isCapture = true;
                    capturedPos = new Position(midX, midY);
                    return true;
                }
            }
            return false;
        }

        private void HandleCapturedPiece(Position capturedPos)
        {
            var piece = _board.Squares[capturedPos.Y, capturedPos.X].Piece;
            if (piece != null)
            {
                var opponent = _players.First(p => p.Color != _currentPlayer.Color);
                if (_playerData.ContainsKey(opponent)) _playerData[opponent].Remove(piece);
                
                // AKSES LANGSUNG ARRAY UNTUK HAPUS
                _board.Squares[capturedPos.Y, capturedPos.X].Piece = null;
            }
        }

        private bool CheckAndPromote(IPiece piece)
        {
            if (piece.TypePiece == PieceType.Pawn)
            {
                if ((piece.Color == PieceColor.Black && piece.Position.Y == 7) ||
                    (piece.Color == PieceColor.Red && piece.Position.Y == 0))
                {
                    piece.TypePiece = PieceType.King;
                    return true;
                }
            }
            return false;
        }

        private void UpdateDrawCounters(bool wasCapture, bool wasPromoted)
        {
            if (wasCapture || wasPromoted) _movesWithoutCapture = 0;
            else _movesWithoutCapture++;
        }

        private void SwitchPlayer()
        {
            FinishGame(null);
            if (_status == StatusType.Play)
            {
                _currentPlayer = (_currentPlayer == _players[0]) ? _players[1] : _players[0];
                if (!CanMakeaMove(_currentPlayer))
                {
                    _status = StatusType.Win;
                    FinishGame(null);
                }
            }
        }
        
        // Helper private untuk FinishGame
        private void FinishGame(Action<IPlayer?>? callback)
        {
            // Logika finish sederhana internal
             IPlayer? winner = null;
            if (!CanMakeaMove(_currentPlayer))
            {
                _status = StatusType.Win;
                winner = _players.First(p => p != _currentPlayer);
            }
            // ... dst
        }
    }
}
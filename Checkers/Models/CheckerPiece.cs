using CheckersGameProject.Contracts;
using CheckersGameProject.Core;

namespace CheckersGameProject.Models
{
    public class CheckersPiece : IPiece
    {
        public PieceColor Color { get; set; }
        public Position Position { get; set; }
        public PieceType TypePiece { get; set; }

        public CheckersPiece(PieceColor color, Position startPosition)
        {
            Color = color;
            Position = startPosition;
            TypePiece = PieceType.Pawn; // 
        }
    }
}
using CheckersGameProject.Contracts;
using CheckersGameProject.Core;

namespace CheckersGameProject.Models
{
    public class Cell : ICell
    {
        public Position Position { get; set; }
        public IPiece? Piece { get; set; }

        public Cell(int x, int y)
        {
            Position = new Position(x, y);
            Piece = null; // Default kosong
        }
    }
}
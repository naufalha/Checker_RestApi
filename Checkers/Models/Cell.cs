using CheckersGameProject.Core;
using CheckersGameProject.Contracts;

namespace CheckersGameProject.Models
{
    // Ubah class jadi struct (Value Type)
    public struct Cell 
    {
        public Position Position { get; set; }
        public IPiece? Piece { get; set; }

        public Cell(int x, int y)
        {
            Position = new Position(x, y);
            Piece = null;
        }
    }
}
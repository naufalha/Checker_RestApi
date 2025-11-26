using CheckersGameProject.Core;

namespace CheckersGameProject.Contracts
{
    public interface ICell
    {
        Position Position { get; set; }
        IPiece? Piece { get; set; } // Bisa null (kosong)
    }
}
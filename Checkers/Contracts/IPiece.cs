using CheckersGameProject.Core;

namespace CheckersGameProject.Contracts
{
    public interface IPiece
    {
        // Kita definisikan properti dasar saja dulu
        PieceColor Color { get; set; }
        Position Position { get; set; }
        PieceType TypePiece { get; set; }
    }
}
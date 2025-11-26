
using CheckersGameProject.Core;

namespace CheckersGameProject.Contracts
{
    public interface IPlayer
    {
        string Name { get; set; }
        PieceColor Color { get; set; }
    }
}
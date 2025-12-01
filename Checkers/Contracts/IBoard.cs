using CheckersGameProject.Models;

namespace CheckersGameProject.Contracts
{
    public interface IBoard
    {
        // REVISI: Return type array of struct Cell
        Cell[,] Squares { get; }
    }
}
namespace CheckersGameProject.Contracts
{
    public interface IBoard
    {
        ICell[,] Squares { get; }
    }
}
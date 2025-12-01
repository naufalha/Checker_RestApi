using CheckersGameProject.GameLogic;

namespace CheckersGameProject.Contracts
{
    public interface ICheckersGameFactory
    {
        CheckersGame CreateGame(string player1Name, string player2Name);
    }
}
using CheckersGameProject.Contracts;
using CheckersGameProject.GameLogic;

namespace CheckersGameProject.Services
{
    public class CheckersGameFactory : ICheckersGameFactory
    {
        public CheckersGame CreateGame(string player1Name, string player2Name)
        {
            return new CheckersGame(player1Name, player2Name);
        }
    }
}
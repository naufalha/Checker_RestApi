using CheckersGameProject.Contracts;

namespace CheckersGameProject.Models
{
    public class CheckersBoard : IBoard
    {
        private ICell[,] _squares;
        public ICell[,] Squares => _squares;

        public CheckersBoard()
        {
            _squares = new Cell[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    _squares[y, x] = new Cell(x, y);
                    // HAPUS logic penempatan bidak disini.
                    // Biarkan kosong. Logic itu milik CheckersGame.
                }
            }
        }
    }
}
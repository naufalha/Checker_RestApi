using CheckersGameProject.Contracts;
using CheckersGameProject.Core;

namespace CheckersGameProject.Models
{
    public class Player : IPlayer
    {
        public string Name { get; set; }
        public PieceColor Color { get; set; }

        public Player(string name, PieceColor color)
        {
            Name = name;
            Color = color;
        }
    }
}
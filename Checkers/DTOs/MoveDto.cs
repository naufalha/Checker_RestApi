using CheckersGameProject.Core;

namespace CheckersGameProject.Api.DTOs
{
    public class MoveDto
    {
        public Position From { get; set; }
        public Position To { get; set; }
        public bool IsCapture { get; set; }
    }
}
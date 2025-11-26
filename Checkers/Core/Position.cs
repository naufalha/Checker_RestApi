using System;

namespace CheckersGameProject.Core
{
    public struct Position
    {
        // Gunakan Huruf Besar (X, Y) karena ini Property
        public int X { get; set; }
        public int Y { get; set; }

        // INI YANG HILANG TADI: Constructor
        // Tanpa ini, error CS1729 muncul
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Helper Equals (Wajib ada di dalam kurung kurawal)
        public override bool Equals(object obj)
        {
            if (!(obj is Position)) return false;
            var p = (Position)obj;
            return X == p.X && Y == p.Y;
        }

        public override int GetHashCode() => (X, Y).GetHashCode();
        public override string ToString() => $"({X},{Y})";
    }
}
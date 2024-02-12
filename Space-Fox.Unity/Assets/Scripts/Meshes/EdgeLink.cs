using System;

namespace SpaceFox
{
    public struct EdgeLink
    {
        public int Index { get; set; }
        public bool Reversed { get; set; }

        public EdgeLink(int index) : this()
            => Index = index;

        public EdgeLink(int index, bool reversed)
        {
            Index = index;
            Reversed = reversed;
        }

        public override readonly bool Equals(object obj)
            => obj is EdgeLink edge && this == edge;

        public override readonly int GetHashCode()
            => HashCode.Combine(Index, Reversed);

        public static bool operator ==(EdgeLink a, EdgeLink b)
            => a.Index == b.Index && a.Reversed == b.Reversed;

        public static bool operator !=(EdgeLink a, EdgeLink b)
            => !(a == b);
    }
}

using System;
using System.Collections.Generic;

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

        public readonly EdgeLink Reverse()
            => new(Index, !Reversed);

        public readonly int GetFirstVertexIndex(List<Edge> edges)
            => Reversed ? edges[Index].Vertex1 : edges[Index].Vertex0;

        public readonly int GetLastVertexIndex(List<Edge> edges)
            => !Reversed ? edges[Index].Vertex1 : edges[Index].Vertex0;

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

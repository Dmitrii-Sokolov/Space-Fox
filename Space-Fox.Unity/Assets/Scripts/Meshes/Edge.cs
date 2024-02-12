using System;

namespace SpaceFox
{
    public struct Edge
    {
        public int Vertex0 { get; private set; }
        public int Vertex1 { get; private set; }

        public int this[int index]
        {
            readonly get
            {
                return index switch
                {
                    0 => Vertex0,
                    1 => Vertex1,
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Vertex0 = value;
                        break;

                    case 1:
                        Vertex1 = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Edge(int vertex0, int vertex1)
        {
            Vertex0 = vertex0;
            Vertex1 = vertex1;
        }

        public override readonly bool Equals(object obj)
            => obj is Edge edge && this == edge;

        public override readonly int GetHashCode()
            => HashCode.Combine(Vertex0, Vertex1);

        public static bool operator ==(Edge a, Edge b)
            => (a.Vertex0 == b.Vertex0 && a.Vertex1 == b.Vertex1)
            || (a.Vertex0 == b.Vertex1 && a.Vertex1 == b.Vertex0);

        public static bool operator !=(Edge a, Edge b)
            => !(a == b);
    }
}

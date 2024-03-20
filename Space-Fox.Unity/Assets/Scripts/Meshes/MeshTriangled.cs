using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceFox
{
    public class MeshTriangled : IMesh
    {
        public struct Triangle
        {
            public int Vertex0 { get; private set; }
            public int Vertex1 { get; private set; }
            public int Vertex2 { get; private set; }

            public int this[int index]
            {
                readonly get
                {
                    return index switch
                    {
                        0 => Vertex0,
                        1 => Vertex1,
                        2 => Vertex2,
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                }
                set
                {
                    switch (index)
                    {
                        case 0: Vertex0 = value;
                            break;

                        case 1: Vertex1 = value;
                            break;

                        case 2: Vertex2 = value;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public Triangle(int vertex0, int vertex1, int vertex2)
            {
                Vertex0 = vertex0;
                Vertex1 = vertex1;
                Vertex2 = vertex2;
            }

            public override readonly bool Equals(object obj)
                => obj is Triangle triangle && this == triangle;

            public override readonly int GetHashCode()
                => HashCode.Combine(Vertex0, Vertex1, Vertex2);

            public static bool operator ==(Triangle a, Triangle b)
                => (a.Vertex0 == b.Vertex0 && a.Vertex1 == b.Vertex1 && a.Vertex2 == b.Vertex2)
                || (a.Vertex0 == b.Vertex1 && a.Vertex1 == b.Vertex2 && a.Vertex2 == b.Vertex0)
                || (a.Vertex0 == b.Vertex2 && a.Vertex1 == b.Vertex0 && a.Vertex2 == b.Vertex1);

            public static bool operator !=(Triangle a, Triangle b)
                => !(a == b);
        }

        private readonly List<Vector3> Vertices;
        private readonly List<Triangle> Triangles;

        public MeshTriangled() : this(
            new Vector3[0],
            new Triangle[0])
        {
        }

        public MeshTriangled(
            IEnumerable<Vector3> vertices,
            IEnumerable<Triangle> triangles)
        {
            Vertices = vertices.ToList();
            Triangles = triangles.ToList();
        }

        public MeshTriangled(
            IEnumerable<Vector3> vertices,
            IEnumerable<Triangle> triangles,
            Vector3 offset,
            float scale) : this(vertices, triangles)
            => MoveAndScale(offset, scale);

        public (Vector3[], int[]) GetVerticesAndTrianglesAsPlainArray()
        {
            var trianglesClassic = new int[3 * Triangles.Count];

            for (var i = 0; i < Triangles.Count; i++)
            {
                for (var j = 0; j < 3; j++)
                    trianglesClassic[3 * i + j] = Triangles[i][j];
            }

            return (Vertices.ToArray(), trianglesClassic.ToArray());
        }

        public void MakeRibbed()
        {
            var newVertices = new Vector3[3 * Triangles.Count];

            for (var i = 0; i < Triangles.Count; i++)
            {
                for (var j = 0; j < 3; j++)
                    newVertices[3 * i + j] = Vertices[Triangles[i][j]];

                Triangles[i] = new(
                    3 * i + 0,
                    3 * i + 1,
                    3 * i + 2);
            }

            Vertices.Clear();
            Vertices.AddRange(newVertices);
        }

        public MeshTriangled MoveAndScale(Vector3 offset, float scale)
        {
            for (var i = 0; i < Vertices.Count; i++)
                Vertices[i] = scale * Vertices[i] + offset;

            return this;
        }

        public static MeshTriangled GetPrimitive(PrimitiveType primitiveType, float size)
            => GetPrimitive(primitiveType, Vector3.zero, size);

        public static MeshTriangled GetPrimitive(PrimitiveType primitiveType, Vector3 center = default, float size = 1f)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Quad:
                    return GetQuad(center, size);

                case PrimitiveType.Tetrahedron:
                    return GetTetrahedron(center, size);

                case PrimitiveType.Cube:
                    return GetCube(center, size);

                default:
                    throw new NotImplementedException();
            }
        }

        public static MeshTriangled GetQuad(float side)
            => GetQuad(Vector3.zero, side);

        public static MeshTriangled GetQuad(Vector3 center = default, float side = 1f)
        {
            var vertices = new List<Vector3>()
            {
                new(-0.5f, 0f, -0.5f),
                new(-0.5f, 0f,  0.5f),
                new(0.5f, 0f, -0.5f),
                new(0.5f, 0f,  0.5f),
            };

            var triangles = new List<Triangle>()
            {
                new(0, 1, 2),
                new(3, 2, 1),
            };

            return new MeshTriangled(vertices, triangles, center, side);
        }

        public static MeshTriangled GetTetrahedron(float radius)
            => GetTetrahedron(Vector3.zero, radius);

        public static MeshTriangled GetTetrahedron(Vector3 center = default, float radius = 1f)
        {
            var vertices = new List<Vector3>()
            {
                new(Mathf.Sqrt(8f / 9f), 0, -1f / 3f),
                new(-Mathf.Sqrt(2f / 9f), -Mathf.Sqrt(2f / 3f), -1f / 3f),
                new(-Mathf.Sqrt(2f / 9f), Mathf.Sqrt(2f / 3f), -1f / 3f),
                new(0, 0, 1),
            };

            var triangles = new List<Triangle>()
            {
                new(0, 1, 2),
                new(1, 3, 2),
                new(2, 3, 0),
                new(3, 1, 0)
            };

            return new(vertices, triangles, center, radius);
        }

        public static MeshTriangled GetCube(float side)
            => GetCube(Vector3.zero, side);

        public static MeshTriangled GetCube(Vector3 center = default, float side = 1f)
        {
            var vertices = new List<Vector3>()
            {
                new(-0.5f, -0.5f, -0.5f),
                new(-0.5f, -0.5f,  0.5f),
                new(-0.5f,  0.5f, -0.5f),
                new(-0.5f,  0.5f,  0.5f),
                new( 0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f,  0.5f),
                new( 0.5f,  0.5f, -0.5f),
                new( 0.5f,  0.5f,  0.5f),
            };

            var triangles = new List<Triangle>()
            {
                new(1, 2, 0),
                new(2, 1, 3),
                new(4, 1, 0),
                new(1, 4, 5),
                new(2, 4, 0),
                new(4, 2, 6),
                new(6, 5, 4),
                new(5, 6, 7),
                new(3, 6, 2),
                new(6, 3, 7),
                new(5, 3, 1),
                new(3, 5, 7),
            };

            return new(vertices, triangles, center, side);
        }
    }
}

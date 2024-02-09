using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFox
{
    public class MeshTriangledEdged : IMesh
    {
        public struct EdgeLink
        {
            public int Index { get; set; }
            public bool Reversed { get; set; }

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

        public struct Triangle
        {

            public EdgeLink Edge0 { get; private set; }
            public EdgeLink Edge1 { get; private set; }
            public EdgeLink Edge2 { get; private set; }

            public EdgeLink this[int index]
            {
                readonly get
                {
                    return index switch
                    {
                        0 => Edge0,
                        1 => Edge1,
                        2 => Edge2,
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                }
                set
                {
                    switch (index)
                    {
                        case 0:
                            Edge0 = value;
                            break;

                        case 1:
                            Edge1 = value;
                            break;

                        case 2:
                            Edge2 = value;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public Triangle(
                int egde0, bool reversed0,
                int egde1, bool reversed1,
                int egde2, bool reversed2)
                : this(
                      new(egde0, reversed0),
                      new(egde1, reversed1),
                      new(egde2, reversed2))
            {
            }

            public Triangle(EdgeLink egde0, EdgeLink egde1, EdgeLink egde2)
            {
                Edge0 = egde0;
                Edge1 = egde1;
                Edge2 = egde2;
            }

            public override readonly bool Equals(object obj)
                => obj is Triangle triangle && this == triangle;

            public override readonly int GetHashCode()
                => HashCode.Combine(Edge0, Edge1, Edge2);

            public static bool operator ==(Triangle a, Triangle b)
                => (a.Edge0 == b.Edge0 && a.Edge1 == b.Edge1 && a.Edge2 == b.Edge2)
                || (a.Edge0 == b.Edge1 && a.Edge1 == b.Edge2 && a.Edge2 == b.Edge0)
                || (a.Edge0 == b.Edge2 && a.Edge1 == b.Edge0 && a.Edge2 == b.Edge1);

            public static bool operator !=(Triangle a, Triangle b)
                => !(a == b);
        }

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

        public List<Vector3> Vertices { get; }
        public List<Edge> Edges { get; }
        public List<Triangle> Triangles { get; }

        public MeshTriangledEdged() : this(new(), new(), new())
        {
        }

        public MeshTriangledEdged(List<Vector3> vertices, List<Edge> edges, List<Triangle> triangles)
        {
            Vertices = vertices;
            Edges = edges;
            Triangles = triangles;
        }

        public int[] GetTrianglesAsPlainArray()
        {
            var trianglesClassic = new List<int>(3 * Triangles.Count);
            foreach (var triangle in Triangles)
            {
                AddVertex(triangle.Edge0);
                AddVertex(triangle.Edge1);
                AddVertex(triangle.Edge2);

                void AddVertex(EdgeLink edge)
                {
                    var v0 = edge.Reversed ? Edges[edge.Index].Vertex1 : Edges[edge.Index].Vertex0;
                    trianglesClassic.Add(v0);
                }
            }

            return trianglesClassic.ToArray();
        }

        public void MakeRibbed()
        {
            var newVertices = new Vector3[3 * Triangles.Count];
            var newEdges = new Edge[3 * Triangles.Count];

            for (var i = 0; i < Triangles.Count; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    newVertices[3 * i + j] = Vertices[Triangles[i][j].Reversed
                        ? Edges[Triangles[i][j].Index].Vertex1
                        : Edges[Triangles[i][j].Index].Vertex0];
                }

                newEdges[3 * i + 0] = new(3 * i + 0, 3 * i + 1);
                newEdges[3 * i + 1] = new(3 * i + 1, 3 * i + 2);
                newEdges[3 * i + 2] = new(3 * i + 2, 3 * i + 0);

                Triangles[i] = new(
                    3 * i + 0, false,
                    3 * i + 1, false,
                    3 * i + 2, false);
            }

            Vertices.Clear();
            Vertices.AddRange(newVertices);

            Edges.Clear();
            Edges.AddRange(newEdges);
        }

        public MeshTriangledEdged MoveAndScale(Vector3 offset, float scale)
        {
            for (var i = 0; i < Vertices.Count; i++)
                Vertices[i] = scale * Vertices[i] + offset;

            return this;
        }

        public static MeshTriangledEdged GetTetrahedron(Vector3 center)
            => GetTetrahedron().MoveAndScale(center, 1f);

        public static MeshTriangledEdged GetTetrahedron(float radius)
            => GetTetrahedron().MoveAndScale(Vector3.zero, radius);

        public static MeshTriangledEdged GetTetrahedron(Vector3 center, float radius)
            => GetTetrahedron().MoveAndScale(center, radius);

        /// <summary>
        /// Get Tetrahedron with center in (0f, 0f, 0f) and radius 1f
        /// </summary>
        public static MeshTriangledEdged GetTetrahedron()
            => MeshTriangled.GetTetrahedron().ToMeshTriangledEdged();

        public static MeshTriangledEdged GetCube(Vector3 center)
            => GetCube().MoveAndScale(center, 1f);

        public static MeshTriangledEdged GetCube(float side)
            => GetCube().MoveAndScale(Vector3.zero, side);

        public static MeshTriangledEdged GetCube(Vector3 center, float side)
            => GetCube().MoveAndScale(center, side);

        /// <summary>
        /// Get Tetrahedron with center in (0f, 0f, 0f) and side 1f
        /// </summary>
        public static MeshTriangledEdged GetCube()
            => MeshTriangled.GetCube().ToMeshTriangledEdged();
    }
}

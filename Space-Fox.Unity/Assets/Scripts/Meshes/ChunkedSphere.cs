using System;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ChunkedSphere : MonoBehaviour
    {
        private readonly struct TriangleV
        {
            public readonly int Vertex0;
            public readonly int Vertex1;
            public readonly int Vertex2;

            public int this[int index]
                => index == 0
                ? Vertex0
                : index == 1
                    ? Vertex1
                    : index == 2
                        ? Vertex2
                        : throw new ArgumentOutOfRangeException();

            public TriangleV(int vertex0, int vertex1, int vertex2)
            {
                Vertex0 = vertex0;
                Vertex1 = vertex1;
                Vertex2 = vertex2;
            }

            public override bool Equals(object obj)
                => obj is TriangleV triangle && this == triangle;

            public override int GetHashCode()
                => HashCode.Combine(Vertex0, Vertex1, Vertex2);

            public static bool operator ==(TriangleV a, TriangleV b)
                => (a.Vertex0 == b.Vertex0 && a.Vertex1 == b.Vertex1 && a.Vertex2 == b.Vertex2)
                || (a.Vertex0 == b.Vertex1 && a.Vertex1 == b.Vertex2 && a.Vertex2 == b.Vertex0)
                || (a.Vertex0 == b.Vertex2 && a.Vertex1 == b.Vertex0 && a.Vertex2 == b.Vertex1);

            public static bool operator !=(TriangleV a, TriangleV b)
                => !(a == b);
        }

        private readonly struct TriangleE
        {
            public readonly (int Index, bool Reversed) Edge0;
            public readonly (int Index, bool Reversed) Edge1;
            public readonly (int Index, bool Reversed) Edge2;

            public (int, bool) this[int index]
                => index == 0
                ? Edge0
                : index == 1
                    ? Edge1
                    : index == 2
                        ? Edge2
                        : throw new ArgumentOutOfRangeException();

            public TriangleE(
                int egde0, bool reversed0,
                int egde1, bool reversed1,
                int egde2, bool reversed2)
                : this(
                      new(egde0, reversed0),
                      new(egde1, reversed1),
                      new(egde2, reversed2))
            {
            }

            public TriangleE((int, bool) egde0, (int, bool) egde1, (int, bool) egde2)
            {
                Edge0 = egde0;
                Edge1 = egde1;
                Edge2 = egde2;
            }

            public override bool Equals(object obj)
                => obj is TriangleE triangle && this == triangle;

            public override int GetHashCode()
                => HashCode.Combine(Edge0, Edge1, Edge2);

            public static bool operator ==(TriangleE a, TriangleE b)
                => (a.Edge0 == b.Edge0 && a.Edge1 == b.Edge1 && a.Edge2 == b.Edge2)
                || (a.Edge0 == b.Edge1 && a.Edge1 == b.Edge2 && a.Edge2 == b.Edge0)
                || (a.Edge0 == b.Edge2 && a.Edge1 == b.Edge0 && a.Edge2 == b.Edge1);

            public static bool operator !=(TriangleE a, TriangleE b)
                => !(a == b);
        }

        private readonly struct Edge
        {
            public readonly int Vertex0;
            public readonly int Vertex1;

            public Edge(int vertex0, int vertex1)
            {
                Vertex0 = vertex0;
                Vertex1 = vertex1;
            }

            public override bool Equals(object obj)
                => obj is Edge edge && this == edge;

            public override int GetHashCode()
                => HashCode.Combine(Vertex0, Vertex1);

            public static bool operator ==(Edge a, Edge b)
                => (a.Vertex0 == b.Vertex0 && a.Vertex1 == b.Vertex1)
                || (a.Vertex0 == b.Vertex1 && a.Vertex1 == b.Vertex0);

            public static bool operator !=(Edge a, Edge b)
                => !(a == b);
        }

        private const float Radius = 10f;
        private const int RecursiveDepth = 1;
        private static readonly Vector3 Center = Vector3.zero;

        private void Awake()
        {
            //It's possible to store triangles as edges (as edge index)
            //Maybe it's will be faster?

            var (vertices, triangles) = GetSphereFromCubeE(RecursiveDepth);

            var mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private static (Vector3[] Vertices, int[] Triangles) GetSphereFromCubeV(int recursiveDepth)
        {
            var (vertices, triangles) = GetCubeV();
            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = vertices[i] * Radius + Center;

            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = GetLocalVertexPosition(vertices[i]);

            for (var i = 0; i < recursiveDepth; i++)
            {
                var newTriangles = new List<TriangleV>();
                var edgeCenters = new List<(Edge Edge, int CenterVertex)>();

                foreach (var triangle in triangles)
                {
                    var center01 = GetCenter(new(triangle[0], triangle[1]));
                    var center12 = GetCenter(new(triangle[1], triangle[2]));
                    var center20 = GetCenter(new(triangle[2], triangle[0]));

                    newTriangles.Add(new(triangle[0], center01, center20));
                    newTriangles.Add(new(triangle[1], center12, center01));
                    newTriangles.Add(new(triangle[2], center20, center12));
                    newTriangles.Add(new(center01, center12, center20));

                    int GetCenter(Edge edge)
                    {
                        foreach (var edgeCenter in edgeCenters)
                        {
                            if (edgeCenter.Edge == edge)
                                return edgeCenter.CenterVertex;
                        }

                        var v = GetLocalVertexPosition(0.5f * (vertices[edge.Vertex0] + vertices[edge.Vertex1]));
                        vertices.Add(v);

                        var vertexNumber = vertices.Count - 1;
                        edgeCenters.Add((edge, vertexNumber));

                        return vertexNumber;
                    }
                }

                triangles = newTriangles;
            }

            return (vertices.ToArray(), TrianglesToArray(triangles));
        }
        
        private static (Vector3[] Vertices, int[] Triangles) GetSphereFromCubeE(int recursiveDepth)
        {
            var (vertices, edges, triangles) = GetCubeE();
            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = vertices[i] * Radius + Center;

            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = GetLocalVertexPosition(vertices[i]);

            for (var i = 0; i < recursiveDepth; i++)
            {
                var newEdges = new List<Edge>();
                var newTriangles = new List<TriangleE>();

                foreach (var edge in edges)
                {
                    var center = GetEdgeCenter(edge);
                    vertices.Add(center);
                    var centerNumber = vertices.Count - 1;

                    newEdges.Add(new(edge.Vertex0, centerNumber));
                    newEdges.Add(new(centerNumber, edge.Vertex0));
                    //newEdgeNumber = 2 * oldEdgeNumber, 2 * oldEdgeNumber + 1

                    Vector3 GetEdgeCenter(Edge edge)
                        => GetLocalVertexPosition(0.5f * (vertices[edge.Vertex0] + vertices[edge.Vertex1]));
                }

                foreach (var triangle in triangles)
                {
                    //New Edges

                    var center0VertexNumber = newEdges[2 * triangle.Edge0.Index].Vertex1;
                    var center1VertexNumber = newEdges[2 * triangle.Edge1.Index].Vertex1;
                    var center2VertexNumber = newEdges[2 * triangle.Edge2.Index].Vertex1;

                    newEdges.Add(new Edge(center0VertexNumber, center1VertexNumber));
                    var edge01Number = newEdges.Count - 1;

                    newEdges.Add(new Edge(center1VertexNumber, center2VertexNumber));
                    var edge12Number = newEdges.Count - 1;

                    newEdges.Add(new Edge(center2VertexNumber, center0VertexNumber));
                    var edge20Number = newEdges.Count - 1;

                    var edge0 = (triangle.Edge2.Reversed ? 2 * triangle.Edge2.Index : 2 * triangle.Edge2.Index + 1, triangle.Edge2.Reversed); 
                    var edge1 = (triangle.Edge0.Reversed ? 2 * triangle.Edge0.Index + 1 : 2 * triangle.Edge0.Index, triangle.Edge0.Reversed); 
                    var edge2 = (edge20Number, true);
                    newTriangles.Add(new(edge0, edge1, edge2));

                    edge0 = (triangle.Edge0.Reversed ? 2 * triangle.Edge0.Index : 2 * triangle.Edge0.Index + 1, triangle.Edge0.Reversed); 
                    edge1 = (triangle.Edge1.Reversed ? 2 * triangle.Edge1.Index + 1 : 2 * triangle.Edge1.Index, triangle.Edge1.Reversed); 
                    edge2 = (edge01Number, true);
                    newTriangles.Add(new(edge0, edge1, edge2));

                    edge0 = (triangle.Edge1.Reversed ? 2 * triangle.Edge1.Index : 2 * triangle.Edge1.Index + 1, triangle.Edge1.Reversed); 
                    edge1 = (triangle.Edge2.Reversed ? 2 * triangle.Edge2.Index + 1 : 2 * triangle.Edge2.Index, triangle.Edge2.Reversed); 
                    edge2 = (edge12Number, true);
                    newTriangles.Add(new(edge0, edge1, edge2));

                    edge0 = (edge01Number, false);
                    edge1 = (edge12Number, false);
                    edge2 = (edge20Number, false);
                    newTriangles.Add(new(edge0, edge1, edge2));
                }

                edges = newEdges;
                triangles = newTriangles;
            }

            var trianglesClassic = new List<int>(3 * triangles.Count);
            foreach (var triangle in triangles)
            {
                AddVertex(triangle.Edge0);
                AddVertex(triangle.Edge1);
                AddVertex(triangle.Edge2);

                void AddVertex((int Index, bool Reversed) edge)
                {
                    var v0 = edge.Reversed ? edges[edge.Index].Vertex1 : edges[edge.Index].Vertex0;
                    trianglesClassic.Add(v0);
                }
            }

            return (vertices.ToArray(), trianglesClassic.ToArray());
        }

        private static (List<Vector3>, List<TriangleV>) GetTetrahedronV()
        {
            var vertices = new List<Vector3>()
            {
                new(Mathf.Sqrt(8f / 9f), 0, -1f / 3f),
                new(-Mathf.Sqrt(2f / 9f), -Mathf.Sqrt(2f / 3f), -1f / 3f),
                new(-Mathf.Sqrt(2f / 9f), Mathf.Sqrt(2f / 3f), -1f / 3f),
                new(0, 0, 1),
            };

            var triangles = new List<TriangleV>()
            {
                new(0, 1, 2),
                new(1, 3, 2),
                new(2, 3, 0),
                new(3, 1, 0)
            };

            return (vertices, triangles);
        }

        private static (List<Vector3>, List<TriangleV>) GetCubeV()
        {
            var vertices = new List<Vector3>()
            {
                new(-1f, -1f, -1f),
                new(-1f, -1f,  1f),
                new(-1f,  1f, -1f),
                new(-1f,  1f,  1f),
                new( 1f, -1f, -1f),
                new( 1f, -1f,  1f),
                new( 1f,  1f, -1f),
                new( 1f,  1f,  1f),
            };

            var triangles = new List<TriangleV>()
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

            return (vertices, triangles);
        }

        private static (List<Vector3>, List<Edge>, List<TriangleE>) GetCubeE()
        {
            var vertices = new List<Vector3>()
            {
                new(-1f, -1f, -1f),
                new(-1f, -1f,  1f),
                new(-1f,  1f, -1f),
                new(-1f,  1f,  1f),
                new( 1f, -1f, -1f),
                new( 1f, -1f,  1f),
                new( 1f,  1f, -1f),
                new( 1f,  1f,  1f),
            };

            var edges = new List<Edge>()
            {
                new(1, 2),
                new(2, 0),
                new(0, 1),

                new(1, 3),
                new(3, 2),

                new(4, 1),
                new(0, 4),

                new(4, 5),
                new(5, 1),


            };

            var triangles = new List<TriangleE>()
            {
                new(0, false, 1, false, 2, false),
                new(0,  true, 3, false, 4, false),
                new(5, false, 2,  true, 6, false),

                new(5,  true, 7, false, 8, false),



                //new(2, 4, 0),
                //new(4, 2, 6),

                //new(6, 5, 4),
                //new(5, 6, 7),
                //new(3, 6, 2),
                //new(6, 3, 7),
                //new(5, 3, 1),
                //new(3, 5, 7),
            };

            return (vertices, edges, triangles);
        }

        private static int[] TrianglesToArray(List<TriangleV> triangles)
        {
            var array = new int[3 * triangles.Count];

            for (var i = 0; i < triangles.Count; i++)
            {
                array[3 * i] = triangles[i][0];
                array[3 * i + 1] = triangles[i][1];
                array[3 * i + 2] = triangles[i][2];
            }

            return array;
        }

        private static Vector3 GetLocalVertexPosition(Vector3 position)
            => Center + Radius * (position - Center).normalized;
    }
}

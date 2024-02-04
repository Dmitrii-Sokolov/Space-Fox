using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ChunkedSphere : MonoBehaviour
    {
        private readonly struct Triangle
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

            public Triangle(int vertex0, int vertex1, int vertex2)
            {
                Vertex0 = vertex0;
                Vertex1 = vertex1;
                Vertex2 = vertex2;
            }

            public override bool Equals(object obj)
                => obj is Triangle triangle && this == triangle;

            public override int GetHashCode()
                => HashCode.Combine(Vertex0, Vertex1, Vertex2);

            public static bool operator ==(Triangle a, Triangle b)
                => (a.Vertex0 == b.Vertex0 && a.Vertex1 == b.Vertex1 && a.Vertex2 == b.Vertex2)
                || (a.Vertex0 == b.Vertex1 && a.Vertex1 == b.Vertex2 && a.Vertex2 == b.Vertex0)
                || (a.Vertex0 == b.Vertex2 && a.Vertex1 == b.Vertex0 && a.Vertex2 == b.Vertex1);

            public static bool operator !=(Triangle a, Triangle b)
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
        private const int RecursiveDepth = 3;
        private static readonly Vector3 Center = Vector3.zero;

        private void Awake()
        {
            var mesh = new Mesh();

            var (vertices, triangles) = GetCube();
            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = vertices[i] * Radius + Center;

            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = GetLocalVertexPosition(vertices[i]);

            //It's possible to keep just vertices and edges, not triangles
            //But which way is faster?

            //It's possible to store triangles as edges (as edge index)
            //Maybe it's will be faster?

            for (var i = 0; i < RecursiveDepth; i++)
            {
                var newTriangles = new List<Triangle>();
                var edgeCenters = new List<(Edge Edge, int CenterVertex)>();

                foreach (var triangle in triangles)
                {
                    //TODO Triangle to edges tranformation
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

            mesh.vertices = vertices.ToArray();
            mesh.triangles = TrianglesToArray(triangles);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private static (List<Vector3>, List<Triangle>) GetTetrahedron()
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

            return (vertices, triangles);
        }

        private static (List<Vector3>, List<Triangle>) GetCube()
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

            var triangles = new List<Triangle>()
            {
                new(0, 3, 2),
                new(1, 3, 0),
                new(2, 3, 6),
                new(3, 7, 6),
                new(4, 6, 7),
                new(4, 7, 5),
                new(4, 5, 1),
                new(1, 0, 4),
                new(0, 2, 6),
                new(0, 6, 4),
                new(1, 7, 3),
                new(1, 5, 7),
            };
            
            //var triangles = new List<Triangle>()
            //{
            //    new(0, 1, 2),
            //    new(1, 3, 2),
            //    new(2, 3, 6),
            //    new(3, 7, 6),
            //    new(4, 6, 7),
            //    new(4, 7, 5),
            //    new(0, 5, 1),
            //    new(5, 0, 4),
            //    new(0, 2, 6),
            //    new(0, 6, 4),
            //    new(1, 5, 3),
            //    new(3, 5, 7),
            //};

            return (vertices, triangles);
        }

        private static int[] TrianglesToArray(List<Triangle> triangles)
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

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

            var (vertices, triangles) = GetTetrahedron();
            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = vertices[i] * Radius + Center;

            for (var i = 0; i < RecursiveDepth; i++)
            {
                var newTriangles = new List<Triangle>();
                var edgeCenters = new List<(Edge Edge, int CenterVertex)>();

                foreach (var triangle in triangles)
                {
                    //TODO Triangle to edges tranformation
                    var vertex0Number = GetCenter(new(triangle[0], triangle[1]));
                    var vertex1Number = GetCenter(new(triangle[1], triangle[2]));
                    var vertex2Number = GetCenter(new(triangle[2], triangle[0]));

                    newTriangles.Add(new(triangle[0], vertex0Number, vertex2Number));
                    newTriangles.Add(new(triangle[1], vertex1Number, vertex0Number));
                    newTriangles.Add(new(triangle[2], vertex2Number, vertex1Number));
                    newTriangles.Add(new(vertex0Number, vertex1Number, vertex2Number));

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

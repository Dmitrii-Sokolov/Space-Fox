using System.Collections.Generic;
using UnityEngine;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ChunkedSphere : MonoBehaviour
    {
        private class Triangle
        {
            public readonly int[] Vertices = new int[3];

            public Triangle(params int[] vertices) => Vertices = vertices;
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

            //TODO A lot of edges can be reused

            for (var i = 0; i < RecursiveDepth; i++)
            {
                var newTriangles = new List<Triangle>();

                foreach (var triangle in triangles)
                {
                    var vertex0 = GetLocalVertexPosition(0.5f * (vertices[triangle.Vertices[0]] + vertices[triangle.Vertices[1]]));
                    var vertex1 = GetLocalVertexPosition(0.5f * (vertices[triangle.Vertices[1]] + vertices[triangle.Vertices[2]]));
                    var vertex2 = GetLocalVertexPosition(0.5f * (vertices[triangle.Vertices[2]] + vertices[triangle.Vertices[0]]));

                    vertices.Add(vertex0);
                    var vertex0Number = vertices.Count - 1;

                    vertices.Add(vertex1);
                    var vertex1Number = vertices.Count - 1;

                    vertices.Add(vertex2);
                    var vertex2Number = vertices.Count - 1;

                    newTriangles.Add(new(triangle.Vertices[0], vertex0Number, vertex2Number));
                    newTriangles.Add(new(triangle.Vertices[1], vertex1Number, vertex0Number));
                    newTriangles.Add(new(triangle.Vertices[2], vertex2Number, vertex1Number));
                    newTriangles.Add(new(vertex0Number, vertex1Number, vertex2Number));
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
                array[3 * i] = triangles[i].Vertices[0];
                array[3 * i + 1] = triangles[i].Vertices[1];
                array[3 * i + 2] = triangles[i].Vertices[2];
            }

            return array;
        }

        private static Vector3 GetLocalVertexPosition(Vector3 position)
            => Center + Radius * (position - Center).normalized;
    }
}

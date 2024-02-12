using System.Collections.Generic;
using UnityEngine;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ChunkedSphere : MonoBehaviour
    {
        private const float Radius = 10f;
        private const int RecursiveDepth = 1;
        private static readonly Vector3 Center = Vector3.zero;

        private void Awake()
        {
            var sphere = GetSphereFromCube(RecursiveDepth);

            var mesh = new Mesh();

            mesh.ApplyData(sphere);

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private static MeshTriangledEdged GetSphereFromCube(int recursiveDepth)
        {
            var cube = MeshTriangledEdged.GetCube();
            var (vertices, edges, triangles) = (cube.Vertices, cube.Edges, cube.Triangles);
            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = vertices[i] * Radius + Center;

            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = GetLocalVertexPosition(vertices[i]);

            for (var i = 0; i < recursiveDepth; i++)
            {
                var newEdges = new List<Edge>();
                var newTriangles = new List<MeshTriangledEdged.Triangle>();

                foreach (var edge in edges)
                {
                    var center = GetEdgeCenter(edge);
                    var centerNumber = vertices.AddAndReturnIndex(center);

                    newEdges.Add(new(edge.Vertex0, centerNumber));
                    newEdges.Add(new(centerNumber, edge.Vertex1));

                    Vector3 GetEdgeCenter(Edge edge)
                        => GetLocalVertexPosition(0.5f * (vertices[edge.Vertex0] + vertices[edge.Vertex1]));
                }

                foreach (var triangle in triangles)
                {
                    //New Edges
                    var center0VertexNumber = newEdges[2 * triangle.Edge0.Index].Vertex1;
                    var center1VertexNumber = newEdges[2 * triangle.Edge1.Index].Vertex1;
                    var center2VertexNumber = newEdges[2 * triangle.Edge2.Index].Vertex1;

                    var edge01Number = newEdges.AddAndReturnIndex(new(center0VertexNumber, center1VertexNumber));
                    var edge12Number = newEdges.AddAndReturnIndex(new(center1VertexNumber, center2VertexNumber));
                    var edge20Number = newEdges.AddAndReturnIndex(new(center2VertexNumber, center0VertexNumber));

                    newTriangles.Add(new(
                        GetEdgeHalf(triangle.Edge2, true),
                        GetEdgeHalf(triangle.Edge0, false),
                        new(edge20Number, true)));

                    newTriangles.Add(new(
                        GetEdgeHalf(triangle.Edge0, true),
                        GetEdgeHalf(triangle.Edge1, false),
                        new(edge01Number, true)));

                    newTriangles.Add(new(
                        GetEdgeHalf(triangle.Edge1, true),
                        GetEdgeHalf(triangle.Edge2, false),
                        new(edge12Number, true)));

                    newTriangles.Add(new(
                        new(edge01Number, false),
                        new(edge12Number, false),
                        new(edge20Number, false)));

                    EdgeLink GetEdgeHalf(EdgeLink oldLink, bool firstHalf)
                    {
                        //newEdgeNumber = 2 * oldEdgeNumber, 2 * oldEdgeNumber + 1
                        return new (
                            oldLink.Reversed ^ firstHalf
                                ? 2 * oldLink.Index + 1
                                : 2 * oldLink.Index,
                            oldLink.Reversed);
                    }
                }

                edges = newEdges;
                triangles = newTriangles;
            }

            return new(vertices, edges, triangles);
        }

        private static Vector3 GetLocalVertexPosition(Vector3 position)
            => Center + Radius * (position - Center).normalized;
    }
}

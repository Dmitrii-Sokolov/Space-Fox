using System.Collections.Generic;
using UnityEngine;

namespace SpaceFox
{
    public static class MeshExtention
    {
        public static void MakeRibbed(this Mesh mesh)
        {
            var oldVertices = mesh.vertices;
            var triangles = mesh.triangles;
            var newVertices = new Vector3[triangles.Length];

            for (var i = 0; i < triangles.Length; i++)
            {
                newVertices[i] = oldVertices[triangles[i]];
                triangles[i] = i;
            }

            mesh.vertices = newVertices;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }

        public static void ApplyData(this Mesh mesh, MeshTriangled data)
        {
            mesh.vertices = data.Vertices.ToArray();
            mesh.triangles = data.GetTrianglesAsPlainArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }

        public static void ApplyData(this Mesh mesh, MeshTriangledEdged data)
        {
            var triangles = data.Triangles;
            var edges = data.Edges;

            var trianglesClassic = new List<int>(3 * triangles.Count);
            foreach (var triangle in triangles)
            {
                AddVertex(triangle.Edge0);
                AddVertex(triangle.Edge1);
                AddVertex(triangle.Edge2);

                void AddVertex(MeshTriangledEdged.EdgeLink edge)
                {
                    var v0 = edge.Reversed ? edges[edge.Index].Vertex1 : edges[edge.Index].Vertex0;
                    trianglesClassic.Add(v0);
                }
            }

            mesh.vertices = data.Vertices.ToArray();
            mesh.triangles = trianglesClassic.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }
}

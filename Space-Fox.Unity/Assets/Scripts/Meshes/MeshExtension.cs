using UnityEngine;

namespace SpaceFox
{
    public static class MeshExtension
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

        public static void ApplyData(this Mesh mesh, IMesh data)
        {
            (mesh.vertices, mesh.triangles) = data.GetVerticesAndTrianglesAsPlainArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }
}

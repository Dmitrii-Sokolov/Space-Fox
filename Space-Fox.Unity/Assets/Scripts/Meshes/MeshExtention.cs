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
    }
}

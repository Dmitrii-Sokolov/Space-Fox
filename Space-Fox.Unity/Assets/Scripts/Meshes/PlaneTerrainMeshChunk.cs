using UnityEngine;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PlaneTerrainMeshChunk : MonoBehaviour
    {
        //TODO Add scaling, not 1 quad = 1 unit
        private const int ChunkSize = 64;

        //TODO Check x and y axes ordering
        private readonly float[,] HeightMap = new float[ChunkSize + 1, ChunkSize + 1];

        private void Awake()
        {
            var mesh = new Mesh();

            //TODO Separete generator class, Scriptable Object with settings
            for (var x = 0; x < ChunkSize + 1; x++)
            {
                for (var y = 0; y < ChunkSize + 1; y++)
                {
                    HeightMap[x, y] = ChunkSize * Mathf.PerlinNoise(x * 1f / ChunkSize, y * 1f / ChunkSize);
                }
            }

            mesh.vertices = GenerateVertices();
            mesh.triangles = GenerateTriangles();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private Vector3[] GenerateVertices()
        {
            var vertices = new Vector3[(ChunkSize + 1) * (ChunkSize + 1)];

            for (var x = 0; x < ChunkSize + 1; x++)
            {
                for (var y = 0; y < ChunkSize + 1; y++)
                {
                    vertices[GetVertexNumber(x, y)] = new Vector3(x, HeightMap[x, y], y);
                }
            }

            return vertices;
        }

        private int[] GenerateTriangles()
        {
            var triangles = new int[ChunkSize * ChunkSize * 6];

            for (var x = 0; x < ChunkSize; x++)
            {
                for (var y = 0; y < ChunkSize; y++)
                {
                    var quadNumber = GetQuadNumber(x, y);

                    triangles[6 * quadNumber + 0] = GetVertexNumber(x, y);
                    triangles[6 * quadNumber + 1] = GetVertexNumber(x, y + 1);
                    triangles[6 * quadNumber + 2] = GetVertexNumber(x + 1, y);

                    triangles[6 * quadNumber + 3] = GetVertexNumber(x + 1, y);
                    triangles[6 * quadNumber + 4] = GetVertexNumber(x, y + 1);
                    triangles[6 * quadNumber + 5] = GetVertexNumber(x + 1, y + 1);
                }
            }

            return triangles;
        }

        private static int GetQuadNumber(int x, int y)
            => ChunkSize * x + y;

        private static int GetVertexNumber(int x, int y)
            => (ChunkSize + 1) * x + y;
    }
}

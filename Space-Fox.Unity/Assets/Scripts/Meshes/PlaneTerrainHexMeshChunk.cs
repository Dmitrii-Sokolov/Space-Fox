using UnityEngine;
using Zenject;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PlaneTerrainHexMeshChunk : MonoBehaviour
    {
        private const int TrianglesCountX = 8;
        private const int TrianglesCountY = 11;

        private const float TriangleSideLength = ChunkLength / TrianglesCountX;
        private static readonly float TriangleHeight = TriangleSideLength * 0.5f * Mathf.Sqrt(3f);

        public const float ChunkLength = 16f;

        [Inject] private readonly PlaneHeightProvider HeightProvider = default;

        private void Awake()
        {
            var mesh = new Mesh();

            mesh.vertices = GenerateVertices();
            mesh.triangles = GenerateTriangles();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private Vector3[] GenerateVertices()
        {
            var vertices = new Vector3[(TrianglesCountX + 1) * (TrianglesCountY + 1)];

            for (var iy = 0; iy < TrianglesCountY + 1; iy++)
            {
                for (var ix = 0; ix < TrianglesCountX + 1; ix++)
                {
                    //This can be simplified for performance
                    var gridPosition = GetLocalVertexPosition(ix, iy);
                    var position = transform.TransformPoint(gridPosition);
                    var positionY = HeightProvider.GetHeight(position.x, position.z);
                    var localPosition = transform.InverseTransformPoint(position.x, positionY, position.z);
                    vertices[GetVertexNumber(ix, iy)] = localPosition;
                }
            }

            return vertices;
        }

        private int[] GenerateTriangles()
        {
            var triangles = new int[TrianglesCountX * TrianglesCountY * 6];

            for (var iy = 0; iy < TrianglesCountY; iy++)
            {
                for (var ix = 0; ix < TrianglesCountX; ix++)
                {
                    var quadNumber = GetTrianglePairNumber(ix, iy);

                    (triangles[6 * quadNumber + 0],
                        triangles[6 * quadNumber + 1],
                        triangles[6 * quadNumber + 2]) = GetTriangleLeft(ix, iy);

                    (triangles[6 * quadNumber + 3],
                        triangles[6 * quadNumber + 4],
                        triangles[6 * quadNumber + 5]) = GetTriangleRight(ix, iy);
                }
            }

            return triangles;
        }

        private static Vector3 GetLocalVertexPosition(int ix, int iy)
            => new((ix - 0.5f * (TrianglesCountX - iy % 2)) * TriangleSideLength,
                0f,
                (iy - 0.5f * TrianglesCountY) * TriangleHeight);

        private static int GetTrianglePairNumber(int x, int y)
            => x + TrianglesCountX * y;

        private static int GetVertexNumber(int x, int y)
            => x + (TrianglesCountX + 1) * y;

        private static (int, int, int) GetTriangleLeft(int x, int y)
            => (GetVertexNumber(x, y),
            GetVertexNumber(x, y + 1),
            GetVertexNumber(x + 1, y + (y % 2)));

        private static (int, int, int) GetTriangleRight(int x, int y)
            => (GetVertexNumber(x + 1, y),
            GetVertexNumber(x, y + 1 - (y % 2)),
            GetVertexNumber(x + 1, y + 1));
    }
}

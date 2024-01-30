using UnityEngine;
using Zenject;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PlaneTerrainQuadMeshChunk : MonoBehaviour
    {
        private const int QuadCount = 64;
        private const float QuadSize = ChunkSize / QuadCount;

        public const float ChunkSize = 16f;

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
            var vertices = new Vector3[(QuadCount + 1) * (QuadCount + 1)];

            for (var iy = 0; iy < QuadCount + 1; iy++)
            {
                for (var ix = 0; ix < QuadCount + 1; ix++)
                {
                    //This can be simplified for performance
                    var gridPosition = GetLocalVertexPosition(ix, iy);
                    var position = transform.TransformPoint(gridPosition);
                    var positionY = HeightProvider?.GetHeight(position.x, position.z) ?? position.y;
                    var localPosition = transform.InverseTransformPoint(position.x, positionY, position.z);
                    vertices[GetVertexNumber(ix, iy)] = localPosition;
                }
            }

            return vertices;
        }

        private int[] GenerateTriangles()
        {
            var triangles = new int[QuadCount * QuadCount * 6];

            for (var iy = 0; iy < QuadCount; iy++)
            {
                for (var ix = 0; ix < QuadCount; ix++)
                {
                    var quadNumber = GetQuadNumber(ix, iy);
                    var isOdd = (ix + iy) % 2 == 0;

                    (triangles[6 * quadNumber + 0],
                        triangles[6 * quadNumber + 1],
                        triangles[6 * quadNumber + 2]) = isOdd
                        ? GetTriangleLeftBottom(ix, iy)
                        : GetTriangleRightBottom(ix, iy);

                    (triangles[6 * quadNumber + 3],
                        triangles[6 * quadNumber + 4],
                        triangles[6 * quadNumber + 5]) = isOdd
                        ? GetTriangleRightTop(ix, iy)
                        : GetTriangleLeftTop(ix, iy);
                }
            }

            return triangles;
        }

        private static Vector3 GetLocalVertexPosition(int x, int y)
            => new(
                x * QuadSize - 0.5f * ChunkSize,
                0f,
                y * QuadSize - 0.5f * ChunkSize);

        private static int GetQuadNumber(int x, int y)
            => x + QuadCount * y;

        private static int GetVertexNumber(int x, int y)
            => x + (QuadCount + 1) * y;

        private static (int, int, int) GetTriangleLeftBottom(int x, int y)
            => (GetVertexNumber(x, y),
            GetVertexNumber(x, y + 1),
            GetVertexNumber(x + 1, y));

        private static (int, int, int) GetTriangleLeftTop(int x, int y)
            => (GetVertexNumber(x, y),
            GetVertexNumber(x, y + 1),
            GetVertexNumber(x + 1, y + 1));

        private static (int, int, int) GetTriangleRightBottom(int x, int y)
            => (GetVertexNumber(x, y),
            GetVertexNumber(x + 1, y + 1),
            GetVertexNumber(x + 1, y));

        private static (int, int, int) GetTriangleRightTop(int x, int y)
            => (GetVertexNumber(x, y + 1),
            GetVertexNumber(x + 1, y + 1),
            GetVertexNumber(x + 1, y));
    }
}

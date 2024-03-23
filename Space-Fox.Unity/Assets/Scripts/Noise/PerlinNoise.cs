using UnityEngine;

namespace SpaceFox
{
    public class PerlinNoise
    {
        public static float GetRandomValue(Vector2 direction)
        {
            var normailised = direction.normalized;
            var debugCellsCount = 16f;
            var uv = normailised * debugCellsCount;

            var gridId = new Vector2(Mathf.Floor(uv.x), Mathf.Floor(uv.y));
            var gridUV = uv - gridId;
            //gridUV = quintic(gridUV);

            var p00 = gridId + Vector2.zero;
            var p01 = gridId + Vector2.right;
            var p10 = gridId + Vector2.up;
            var p11 = gridId + Vector2.one;

            var gradP00 = GetRandomGradient(p00);
            var gradP01 = GetRandomGradient(p01);
            var gradP10 = GetRandomGradient(p10);
            var gradP11 = GetRandomGradient(p11);

            var distP00 = gridUV - Vector2.zero;
            var distP01 = gridUV - Vector2.right;
            var distP10 = gridUV - Vector2.up;
            var distP11 = gridUV - Vector2.one;

            var dotP00 = Vector3.Dot(gradP00, distP00);
            var dotP01 = Vector3.Dot(gradP01, distP01);
            var dotP10 = Vector3.Dot(gradP10, distP10);
            var dotP11 = Vector3.Dot(gradP11, distP11);

            var dotP0 = Mathf.Lerp(dotP00, dotP01, gridUV.x);
            var dotP1 = Mathf.Lerp(dotP10, dotP11, gridUV.x);
            var dotP = Mathf.Lerp(dotP0, dotP1, gridUV.y);

            return (dotP * 0.1f + 1f);
        }

        private static Vector2 GetRandomGradient(Vector2 vector)
        {
            vector += 0.01f * Vector2.one;
            var x = Vector2.Dot(vector, new Vector2(123.4f, 234.5f));
            var y = Vector2.Dot(vector, new Vector2(234.5f, 345.6f));

            var gradient = new Vector2(x, y);
            gradient = new Vector2(Mathf.Sin(gradient.x), Mathf.Sin(gradient.y));
            gradient *= 43758.5453f;
            gradient = new Vector2(Mathf.Sin(gradient.x), Mathf.Sin(gradient.y));

            return gradient;
        }
    }
}

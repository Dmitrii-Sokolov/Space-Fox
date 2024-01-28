using UnityEngine;

namespace SpaceFox
{
    public class PlaneHeightProvider
    {
        private const float Scale = 0.1f;

        public float GetHeight(Vector2 position)
            => GetHeight(position.x, position.y);

        public float GetHeight(float x, float y)
            => Mathf.PerlinNoise(Scale * x, Scale * y) / Scale;
    }
}

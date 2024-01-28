using UnityEngine;
using Zenject;

namespace SpaceFox
{
    public class PlaneTerrain : MonoBehaviour
    {
        [SerializeField] private PlaneTerrainMeshChunk PlaneTerrainMeshChunk = default;
        [SerializeField] private int XCount = default;
        [SerializeField] private int YCount = default;

        [Inject] private readonly DiContainer DiContainer = default;

        private void Awake()
        {
            //Just for tests

            for (var x = 0; x < XCount; x++)
            {
                for (var y = 0; y < YCount; y++)
                {
                    var position = new Vector3(
                        transform.position.x + (x - 0.5f * (XCount - 1)) * PlaneTerrainMeshChunk.ChunkSize,
                        0,
                        transform.position.z + (y - 0.5f * (YCount - 1)) * PlaneTerrainMeshChunk.ChunkSize);

                    var go = DiContainer.InstantiatePrefab(
                        PlaneTerrainMeshChunk,
                        position,
                        Quaternion.identity,
                        transform);

#if UNITY_EDITOR
                    go.name = $"Chunk_{x}_{y}";
#endif
                }
            }
        }
    }
}

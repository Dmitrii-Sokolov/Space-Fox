using UnityEngine;

namespace SpaceFox
{
    public interface IMesh
    {
        (Vector3[], int[]) GetVerticesAndTrianglesAsPlainArray();
    }
}

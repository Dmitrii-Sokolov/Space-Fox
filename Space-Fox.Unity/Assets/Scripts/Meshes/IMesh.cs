using System.Collections.Generic;
using UnityEngine;

namespace SpaceFox
{
    public interface IMesh
    {
        List<Vector3> Vertices { get; }
        int[] GetTrianglesAsPlainArray();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFox
{
    public class QuadVector3 : List<Vector3>
    {
        public Vector3 RightTop => this[0];
        public Vector3 RightBottom => this[1];
        public Vector3 LeftBottom => this[2];
        public Vector3 LeftTop => this[3];

        public Vector3 LeftNormal => Vector3.Cross(LeftBottom, LeftTop);
        public Vector3 RightNormal => Vector3.Cross(RightTop, RightBottom);

        public Vector3 BottomNormal => Vector3.Cross(RightBottom, LeftBottom);
        public Vector3 TopNormal => Vector3.Cross(LeftTop, RightTop);

		//TODO Prevent creation with non-4 elements
        public QuadVector3(IEnumerable<Vector3> collection) : base(collection)
        {
        }

        public void CutByX(int x, float divider, Func<Vector3, Vector3, float, Vector3> lerp)
        {
            var v0 = lerp(LeftTop,    RightTop,    (x + 1) / divider);
            var v1 = lerp(LeftBottom, RightBottom, (x + 1) / divider);
            var v2 = lerp(LeftBottom, RightBottom,  x      / divider);
            var v3 = lerp(LeftTop,    RightTop,     x      / divider);

            this[0] = v0;
            this[1] = v1;
            this[2] = v2;
            this[3] = v3;
        }

        public void CutByY(int y, float divider, Func<Vector3, Vector3, float, Vector3> lerp)
        {
            var v0 = lerp(RightBottom, RightTop, (y + 1) / divider);
            var v1 = lerp(RightBottom, RightTop,  y      / divider);
            var v2 = lerp(LeftBottom,  LeftTop,   y      / divider);
            var v3 = lerp(LeftBottom,  LeftTop,  (y + 1) / divider);

            this[0] = v0;
            this[1] = v1;
            this[2] = v2;
            this[3] = v3;
        }
    }
}

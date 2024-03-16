using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceFox
{
    public class QuadVector3 : List<Vector3>
    {
        public static readonly Vector2Int[] Neighbours = new Vector2Int[]
        {
            new( 1,  0),
            new( 0, -1),
            new(-1,  0),
            new( 0,  1),
        };

        public Vector3 RightTop
        {
            get => this[0];
            set => this[0] = value;
        }

        public Vector3 RightBottom
        {
            get => this[1];
            set => this[1] = value;
        }

        public Vector3 LeftBottom
        {
            get => this[2];
            set => this[2] = value;
        }

        public Vector3 LeftTop
        {
            get => this[3];
            set => this[3] = value;
        }

        public Vector3 LeftNormal => Vector3.Cross(LeftBottom, LeftTop);
        public Vector3 RightNormal => Vector3.Cross(RightBottom, RightTop);

        public Vector3 BottomNormal => Vector3.Cross(RightBottom, LeftBottom);
        public Vector3 TopNormal => Vector3.Cross(RightTop, LeftTop);

        public QuadVector3(IEnumerable<Vector3> collection) : base(collection)
        {
            if (collection.Count() != 4)
                throw new ArgumentException();
        }

        public void CutByX(int x, int divider, Func<Vector3, Vector3, float, Vector3> lerp)
        {
            var min = x / (float)divider;
            var max = (x + 1) / (float)divider;

            var v0 = lerp(LeftTop,    RightTop,    max);
            var v1 = lerp(LeftBottom, RightBottom, max);
            var v2 = lerp(LeftBottom, RightBottom, min);
            var v3 = lerp(LeftTop,    RightTop,    min);

            this[0] = v0;
            this[1] = v1;
            this[2] = v2;
            this[3] = v3;
        }

        public void CutByY(int y, int divider, Func<Vector3, Vector3, float, Vector3> lerp)
        {
            var min = y / (float)divider;
            var max = (y + 1) / (float)divider;

            var v0 = lerp(RightBottom, RightTop, max);
            var v1 = lerp(RightBottom, RightTop, min);
            var v2 = lerp(LeftBottom,  LeftTop,  min);
            var v3 = lerp(LeftBottom,  LeftTop,  max);

            this[0] = v0;
            this[1] = v1;
            this[2] = v2;
            this[3] = v3;
        }
    }
}

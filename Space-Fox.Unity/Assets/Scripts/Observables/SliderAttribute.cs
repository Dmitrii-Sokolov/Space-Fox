using System;
using UnityEngine;

namespace SpaceFox
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class SliderAttribute : PropertyAttribute
    {
        public float Min { get; }
        public float Max { get; }

        public int MinInt => Convert.ToInt32(Min);
        public int MaxInt => Convert.ToInt32(Max);

        public SliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}

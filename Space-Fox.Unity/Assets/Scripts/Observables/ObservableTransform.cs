using System;
using UnityEngine;

namespace SpaceFox
{
    [Serializable]
    public class ObservableTransform : ObservableValue<Transform>
    {
        private readonly ObservableValue<Vector3> PositionInternal = new();
        private readonly ObservableValue<Quaternion> RotationInternal = new();

        public IReadOnlyObservableValue<Vector3> Position => PositionInternal;
        public IReadOnlyObservableValue<Quaternion> Rotation => RotationInternal;

        public ObservableTransform() : this(default)
        {
        }

        public ObservableTransform(Transform value) : base(value)
        {
            PositionInternal.While(this);
            RotationInternal.While(this);
        }

        public IDisposable SetUpdateProvider(ISubscriptionProvider provider, bool initializeValues = true)
        {
            if (initializeValues)
                UpdateValue();

            var unsubscribe = provider.Subscribe(UpdateValue);

            unsubscribe.While(this);

            return unsubscribe;
        }

        private void UpdateValue()
        {
            if (Value != null)
            {
                PositionInternal.Value = Value.position;
                RotationInternal.Value = Value.rotation;
            }
        }
    }
}

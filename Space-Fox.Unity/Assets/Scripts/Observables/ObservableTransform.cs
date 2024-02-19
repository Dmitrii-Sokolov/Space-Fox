using System;
using UnityEngine;
using Zenject;

namespace SpaceFox
{
    public class ObservableTransform : DisposableComposer, IDisposable
    {
        public class Factory : PlaceholderFactory<Transform, UpdateType, ObservableTransform> { }

        private readonly ObservableValue<Vector3> PositionInternal = new();
        private readonly ObservableValue<Quaternion> RotationInternal = new();

        public Transform Transform { get; private set; }

        public IReadOnlyObservableValue<Vector3> Position => PositionInternal;
        public IReadOnlyObservableValue<Quaternion> Rotation => RotationInternal;

        public ObservableTransform(Transform transform, UpdateType updateType, UpdateProxy updateProxy)
        {
            Transform = transform;
            UpdateValue();

            updateProxy.GetUpdate(updateType).Subscribe(UpdateValue).While(this);
            PositionInternal.While(this);
            RotationInternal.While(this);
        }

        private void UpdateValue()
        {
            if (Transform == null)
            {
                Transform = null;
                Dispose();
            }
            else
            {
                PositionInternal.Value = Transform.position;
                RotationInternal.Value = Transform.rotation;
            }
        }
    }
}

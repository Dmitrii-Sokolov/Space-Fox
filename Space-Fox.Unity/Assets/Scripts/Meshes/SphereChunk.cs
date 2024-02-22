using System;
using UnityEngine;
using Zenject;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SphereChunk : DisposableMonoBehaviour
    {
        private static readonly Vector3 Center = Vector3.zero;

        private static readonly Vector3[] CubeSideDirections = new[] {
                Vector3.left,
                Vector3.right,
                Vector3.down,
                Vector3.up,
                Vector3.back,
                Vector3.forward};

        [Inject] private readonly UpdateProxy UpdateProxy = default;
        [Inject] private readonly ObservableTransform.Factory ObservableTransformFactory = default;

        private readonly ObservableValue<float> Radius = new();

        private ObservableTransform Observer;
        private ObservableTransform Self;

        private bool IsDirty = false;

        [Range(0.1f, 100f)]
        [SerializeField] private float CurrentRadius = default;

        [SerializeField] private Transform TrackedTransform = default;

        protected override void AwakeBeforeDestroy()
        {
            //TODO Check rotation when calculation quadrant
            //TODO Generate whole sphere when far

            Observer = ObservableTransformFactory.Create(TrackedTransform, UpdateType.Update);
            Self = ObservableTransformFactory.Create(transform, UpdateType.Update);

            Observer.Position.Subscribe(_ => IsDirty = true).While(this);
            Self.Position.Subscribe(_ => IsDirty = true).While(this);
            Radius.Subscribe(_ => IsDirty = true).While(this);

            UpdateProxy.LateUpdate.Subscribe(OnLateUpdate).While(this);
        }

        private void OnLateUpdate()
        {
            Radius.Value = CurrentRadius;

            if (IsDirty)
            {
                IsDirty = false;
                RegenerageMesh();
            }
        }

        private void RegenerageMesh()
        {
            var sphere = GetMesh();

            var mesh = new Mesh();

            mesh.ApplyData(sphere);

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private MeshPolygoned GetMesh()
        {
            var vectorToSurface = Observer.Position.Value - Self.Position.Value;
            var direction = CubeSideDirections.GetMax(d => Vector3.Dot(d, vectorToSurface));
            var rotation = Quaternion.FromToRotation(Vector3.right, direction);
            var localUp = rotation * Vector3.up;
            var localForward = rotation * Vector3.forward;

            var orthant = new Vector3[4]
            {
                direction + localUp + localForward,
                direction - localUp + localForward,
                direction - localUp - localForward,
                direction + localUp - localForward,
            };

            for (var i = 0; i < orthant.Length; i++)
                orthant[i] = GetLocalVertexPosition(orthant[i]);

            var mesh = MeshPolygoned.GetPolygon(orthant);

            var distance = vectorToSurface.magnitude;

            //for (var i = 0; i < RecursiveDepth.Value; i++)
            //    mesh.Subdivide(GetLocalVertexPosition);

            return mesh;
        }

        private Vector3 GetLocalVertexPosition(Vector3 position)
            => Center + Radius.Value * (position - Center).normalized;
    }
}

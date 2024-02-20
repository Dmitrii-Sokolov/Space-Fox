using System;
using UnityEngine;
using Zenject;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SphereFromPrimitive : DisposableMonoBehaviour
    {
        private static readonly Vector3 Center = Vector3.zero;

        [Inject] private readonly UpdateProxy UpdateProxy = default;

        private readonly ObservableValue<int> RecursiveDepth = new();
        private readonly ObservableValue<float> Radius = new();
        private readonly ObservableValue<PrimitiveType> PrimitiveType = new();

        private bool IsDirty = false;

        [Range(0, 7)]
        [SerializeField] private int CurrentRecursiveDepth = default;

        [Range(0.1f, 100f)]
        [SerializeField] private float CurrentRadius = default;

        [SerializeField] private PrimitiveType CurrentPrimitiveType = default;

        protected override void AwakeBeforeDestroy()
        {
            RecursiveDepth.Subscribe(_ => IsDirty = true).While(this);
            Radius.Subscribe(_ => IsDirty = true).While(this);
            PrimitiveType.Subscribe(_ => IsDirty = true).While(this);

            UpdateProxy.Update.Subscribe(OnUpdate).While(this);
        }

        private void OnUpdate()
        {
            RecursiveDepth.Value = CurrentRecursiveDepth;
            Radius.Value = CurrentRadius;
            PrimitiveType.Value = CurrentPrimitiveType;

            if (IsDirty)
            {
                IsDirty = false;
                RegenerageMesh();
            }
        }

        private void RegenerageMesh()
        {
            var sphere = GetSphere();

            var mesh = new Mesh();

            mesh.ApplyData(sphere);

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private MeshPolygoned GetSphere()
        {
            var primitiveType = PrimitiveType.Value;
            var center = Center + (primitiveType.IsPlanar() ? 0.01f * Vector3.back : Vector3.zero);

            var mesh = MeshPolygoned.GetPrimitive(primitiveType, center)
                .TransformVertices(GetLocalVertexPosition);

            for (var i = 0; i < RecursiveDepth.Value; i++)
                mesh.Subdivide(GetLocalVertexPosition);

            return mesh;
        }

        private Vector3 GetLocalVertexPosition(Vector3 position)
            => Center + Radius.Value * (position - Center).normalized;
    }
}

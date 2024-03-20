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

        [Slider(0, 7)]
        [SerializeField] private ObservableValue<int> RecursiveDepth = new();

        [Slider(0.1f, 10f)]
        [SerializeField] private ObservableValue<float> Radius = new(1f);

        [SerializeField] private ObservableValue<PrimitiveType> PrimitiveType = new();

        private bool IsDirty = false;

        protected override void AwakeBeforeDestroy()
        {
            RecursiveDepth.Subscribe(SetDirty).While(this);
            Radius.Subscribe(SetDirty).While(this);
            PrimitiveType.Subscribe(SetDirty).While(this);

            UpdateProxy.Update.Subscribe(OnUpdate).While(this);
        }

        private void SetDirty()
            => IsDirty = true;

        private void OnUpdate()
        {
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

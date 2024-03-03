using UnityEngine;
using Zenject;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SphereChunk : DisposableMonoBehaviour
    {
        [Inject] private readonly UpdateProxy UpdateProxy = default;

        [SerializeField] private ObservableValue<Vector3> Center = new();

        [Slider(0.1f, 10f)]
        [SerializeField] private ObservableValue<float> Radius = new();

        [Slider(0.1f, 10f)]
        [SerializeField] private ObservableValue<float> AreaSize = new();

        [Slider(0.1f, 10f)]
        [SerializeField] private ObservableValue<float> TriangleSize = new();

        [SerializeField] private ObservableTransform Observer = new();
        [SerializeField] private ObservableTransform Self = new();

        private bool IsDirty = false;

        private MeshPolygoned ReferenceMesh = default;

        protected override void AwakeBeforeDestroy()
        {
            Center.Subscribe(SetDirty).While(this);
            Radius.Subscribe(SetDirty).While(this);
            AreaSize.Subscribe(SetDirty).While(this);
            TriangleSize.Subscribe(SetDirty).While(this);
            Observer.Position.Subscribe(SetDirty).While(this);
            Self.Position.Subscribe(SetDirty).While(this);

            Observer.SetUpdateProvider(UpdateProxy.Update).While(this);
            Self.SetUpdateProvider(UpdateProxy.Update).While(this);

            UpdateProxy.LateUpdate.Subscribe(OnLateUpdate).While(this);
        }

        private void SetDirty()
            => IsDirty = true;

        private void OnLateUpdate()
        {
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
            var vectorToCenter = Observer.Position.Value - (Self.Position.Value + Center.Value);
            var polygonIndex = GetNearestPolygonIndex(vectorToCenter);
            var polygon = ReferenceMesh.Polygons[polygonIndex];

            //TODO Check rotation when calculation quadrant
            //TODO Generate whole sphere when far

            //TODO This can be caching / used with 'IsDirty' check by: x, y, polygonIndex, Center, Radius...
            //TODO Check with other meshes, not cubes
            //TODO Simplify sector dividing
            //TODO Add height noise
            //TODO Add neighbours quad
            //TODo Check if x y is numbers or not

            //TODO BUG Center changing doesn't work correctly
            //TODO BUG Area size affects triangle size

            var minSize = polygon.GetMinSideSize(ReferenceMesh);
            var divider = 1 << (Mathf.RoundToInt(Mathf.Log(minSize / AreaSize.Value, 2)));

            var sector = polygon.Count == 4 ? polygon.GetVertices(ReferenceMesh) : throw new System.ArgumentException();

            //Will try to find barycentric coordinates of vectorToSurface in this orthant
            var backNormal = Vector3.Cross(sector[2], sector[3]);
            var forwardNormal = Vector3.Cross(sector[1], sector[0]);
            var x = DecomppositeByPlanes(vectorToCenter, backNormal, forwardNormal);

            sector = new Vector3[4]
            {
                Vector3.Slerp(sector[3], sector[0], (Mathf.Floor(x * divider) + 1) / divider),
                Vector3.Slerp(sector[2], sector[1], (Mathf.Floor(x * divider) + 1) / divider),
                Vector3.Slerp(sector[2], sector[1], Mathf.Floor(x * divider) / divider),
                Vector3.Slerp(sector[3], sector[0], Mathf.Floor(x * divider) / divider),
            };

            var bottomNormal = Vector3.Cross(sector[2], sector[1]);
            var topNormal = Vector3.Cross(sector[3], sector[0]);
            var y = DecomppositeByPlanes(vectorToCenter, bottomNormal, topNormal);

            sector = new Vector3[]
            {
                Vector3.Slerp(sector[1], sector[0], (Mathf.Floor(y * divider) + 1) / divider),
                Vector3.Slerp(sector[1], sector[0], Mathf.Floor(y * divider) / divider),
                Vector3.Slerp(sector[2], sector[3], Mathf.Floor(y * divider) / divider),
                Vector3.Slerp(sector[2], sector[3], (Mathf.Floor(y * divider) + 1) / divider),
            };

            for (var i = 0; i < sector.Length; i++)
                sector[i] = GetLocalVertexPosition(sector[i]);

            var mesh = MeshPolygoned.GetPolygon(sector);

            var maxSize = polygon.GetMaxSideSize(ReferenceMesh);
            var currentTrianlgeSide = maxSize / divider;
            var subdivider = Mathf.RoundToInt(Mathf.Log(currentTrianlgeSide / TriangleSize.Value, 2));

            for (var i = 0; i < subdivider; i++)
                mesh.Subdivide(GetLocalVertexPosition);

            return mesh;
        }

        private int GetNearestPolygonIndex(Vector3 vectorToCenter)
        {
            ReferenceMesh = MeshPolygoned.GetCube(Center.Value);

            var polygonIndex = 0;
            var maxProjection = GetDistanceToPolygonPlane(ReferenceMesh, polygonIndex, vectorToCenter);

            for (var i = 1; i < ReferenceMesh.Polygons.Count; i++)
            {
                var newProjection = GetDistanceToPolygonPlane(ReferenceMesh, i, vectorToCenter);
                if (newProjection > maxProjection)
                {
                    polygonIndex = i;
                    maxProjection = newProjection;
                }
            }

            return polygonIndex;
        }

        private float GetDistanceToPolygonPlane(MeshPolygoned mesh, int polygonIdex, Vector3 point)
            => Vector3.Dot(mesh.Polygons[polygonIdex].GetCenter(mesh) - Center.Value, point);

        private float DecomppositeByPlanes(Vector3 vector, Vector3 normal0, Vector3 normal1)
        {
            //Slice is the plane, that is perpendicular to two basis planes
            var sliceNormal = Vector3.Cross(normal0, normal1).normalized;

            //Directions of the intersections of planes with slice
            var direction0 = Vector3.Cross(normal0, sliceNormal);
            var direction1 = Vector3.Cross(normal1, sliceNormal);
            var vectorProjectionToSlice = vector - sliceNormal * Vector3.Dot(sliceNormal, vector);
            var (a, b) = DecompositeByBasis(vectorProjectionToSlice, direction0, direction1);
            return Mathf.Abs(b) / (Mathf.Abs(a) + Mathf.Abs(b));
        }

        private (float X, float Y) DecompositeByBasis(Vector3 vector, Vector3 baseA, Vector3 baseB)
        {
            //Using Gaussian elimination method
            var ab = Vector3.Dot(baseA, baseB);
            var a2 = Vector3.Dot(baseA, baseA);
            var b2 = Vector3.Dot(baseB, baseB);
            var va = Vector3.Dot(vector, baseA);
            var vb = Vector3.Dot(vector, baseB);
            var y = (vb - va * ab / a2) / (b2 - ab * ab / a2);
            var x = (va - y * ab) / a2;
            return (x, y);
        }

        private Vector3 GetLocalVertexPosition(Vector3 position)
            => Center.Value + Radius.Value * (position - Center.Value).normalized;
    }
}

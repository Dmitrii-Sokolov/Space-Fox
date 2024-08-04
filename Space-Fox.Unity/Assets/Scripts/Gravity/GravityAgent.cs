using UnityEngine;
using Zenject;

namespace SpaceFox
{
    [RequireComponent(typeof(Rigidbody))]
    public class GravityAgent : DisposableMonoBehaviour
    {
        [SerializeField] private Vector3 InitialVelocity = default;

        [Inject] private readonly GravitySystem GravitySystem;

        protected override void AwakeBeforeDestroy()
        {
            base.AwakeBeforeDestroy();

            var body = GetComponent<Rigidbody>();
            body.linearVelocity = InitialVelocity;

            GravitySystem.AddAgent(body).While(this);
        }
    }
}

using UnityEngine;
using Zenject;

namespace SpaceFox
{
    public class GravityAgent : DisposableMonoBehaviour
    {
        [SerializeField] private Rigidbody Rigidbody;

        [Inject] private readonly GravitySystem GravitySystem;

        public Rigidbody Body => Rigidbody;

        protected override void AwakeBeforeDestroy()
        {
            base.AwakeBeforeDestroy();

            GravitySystem.AddAgent(this).While(this);
        }
    }
}

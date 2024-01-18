using System;
using UnityEngine;

namespace SpaceFox
{
    public abstract class UpdateProxyBase : MonoBehaviour
    {
        protected class Updater : ISubscriptionProvider
        {
            private event Action UpdateEvent;

            public IDisposable Subscribe(Action action)
            {
                UpdateEvent += action;

                return new Subscription(() => UpdateEvent -= action);
            }

            public void Invoke()
                => UpdateEvent?.Invoke();
        }

        protected Updater UpdateEvent = new();
        protected Updater LateUpdateEvent = new();
        protected Updater FixedUpdateEvent = new();

        private void Update()
            => UpdateEvent?.Invoke();

        private void LateUpdate()
            => LateUpdateEvent?.Invoke();

        private void FixedUpdate()
            => FixedUpdateEvent?.Invoke();
    }
}

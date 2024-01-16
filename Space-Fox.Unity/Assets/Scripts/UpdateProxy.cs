using System;
using UnityEngine;

namespace SpaceFox
{
    public class UpdateProxy : MonoBehaviour
    {
        private class Updater : ISubscriptionProvider
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

        private Updater UpdateEvent = new();
        private Updater LateUpdateEvent = new();
        private Updater FixedUpdateEvent = new();

        public ISubscriptionProvider OnUpdate => UpdateEvent;
        public ISubscriptionProvider OnLateUpdate => LateUpdateEvent;
        public ISubscriptionProvider OnFixedUpdate => FixedUpdateEvent;

        private void Update()
            => UpdateEvent?.Invoke();

        private void LateUpdate()
            => LateUpdateEvent?.Invoke();

        private void FixedUpdate()
            => FixedUpdateEvent?.Invoke();
    }
}

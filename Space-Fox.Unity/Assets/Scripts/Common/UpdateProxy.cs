using System;

namespace SpaceFox
{
    public class UpdateProxy : UpdateProxyBase
    {
        public ISubscriptionProvider Update => UpdateEvent;
        public ISubscriptionProvider LateUpdate => LateUpdateEvent;
        public ISubscriptionProvider FixedUpdate => FixedUpdateEvent;

        public ISubscriptionProvider GetUpdate(UpdateType type)
            => type switch
            {
                UpdateType.Update => Update,
                UpdateType.LateUpdate => LateUpdate,
                UpdateType.FixedUpdate => FixedUpdate,
                _ => throw new ArgumentException($"Invalid update type: '{type}'"),
            };
    }
}

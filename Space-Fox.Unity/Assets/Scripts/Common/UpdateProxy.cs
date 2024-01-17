namespace SpaceFox
{
    public class UpdateProxy : UpdateProxyBase
    {
        public ISubscriptionProvider Update => UpdateEvent;
        public ISubscriptionProvider LateUpdate => LateUpdateEvent;
        public ISubscriptionProvider FixedUpdate => FixedUpdateEvent;
    }
}

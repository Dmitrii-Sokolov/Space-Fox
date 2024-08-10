namespace SpaceFox
{
    public interface IUpdateProxy
    {
        ISubscriptionProvider FixedUpdate { get; }
        ISubscriptionProvider LateUpdate { get; }
        ISubscriptionProvider Update { get; }

        ISubscriptionProvider GetUpdate(UpdateType type);
    }
}
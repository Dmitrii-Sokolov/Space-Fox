using System;

namespace SpaceFox
{
    public interface ISubscriptionProvider
    {
        IDisposable Subscribe(Action action);
    }
}

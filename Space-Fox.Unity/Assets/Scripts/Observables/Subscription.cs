using System;

namespace SpaceFox
{
    public class Subscription : IDisposable
    {
        private bool IsDisposed = false;
        private Action CancelSubscription;

        public Subscription(Action cancelSubscrition)
            => CancelSubscription = cancelSubscrition;

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                CancelSubscription?.Invoke();
                CancelSubscription = null;
            }
        }
    }
}

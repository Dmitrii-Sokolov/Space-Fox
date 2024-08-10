using System;
using VContainer.Unity;

namespace SpaceFox
{
    public class EntryPointUpdater : IUpdateProxy,
        IFixedTickable,
        IPostFixedTickable,
        ITickable,
        IPostTickable,
        ILateTickable,
        IPostLateTickable
    {
        public ISubscriptionProvider Update => TickEvent;
        public ISubscriptionProvider LateUpdate => LateTickEvent;
        public ISubscriptionProvider FixedUpdate => FixedTickEvent;

        public ISubscriptionProvider GetUpdate(UpdateType type)
            => type switch
            {
                UpdateType.Update => Update,
                UpdateType.LateUpdate => LateUpdate,
                UpdateType.FixedUpdate => FixedUpdate,
                _ => throw new ArgumentException($"Invalid update type: '{type}'"),
            };

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

        protected Updater FixedTickEvent = new();
        protected Updater PostFixedTickEvent = new();
        protected Updater TickEvent = new();
        protected Updater PostTickEvent = new();
        protected Updater LateTickEvent = new();
        protected Updater PostLateTickEvent = new();

        public void FixedTick() => FixedTickEvent.Invoke();
        public void PostFixedTick() => PostFixedTickEvent.Invoke();
        public void Tick() => TickEvent.Invoke();
        public void PostTick() => PostTickEvent.Invoke();
        public void LateTick() => LateTickEvent.Invoke();
        public void PostLateTick() => PostLateTickEvent.Invoke();
    }
}

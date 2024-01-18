using System;
using System.Collections.Generic;

namespace SpaceFox
{
    public class ObserversComposer<T> : IObserver<T>, IObservable<T>, IDisposable
    {
        private readonly List<IObserver<T>> Observers = new();
        private readonly List<IObserver<T>> Unsubscribers = new();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Observers.Add(observer);

            return new Subscription(() => Unsubscribers.Add(observer));
        }

        public void Dispose()
            => OnCompleted();
        
        public void OnCompleted()
        {
            CheckUnsubscribers();

            foreach (var observer in Observers)
                observer.OnCompleted();

            Observers.Clear();
            Unsubscribers.Clear();
        }

        public void OnError(Exception error)
        {
            CheckUnsubscribers();

            foreach (var observer in Observers)
                observer.OnError(error);
        }

        public void OnNext(T value)
        {
            CheckUnsubscribers();

            foreach (var observer in Observers)
                observer.OnNext(value);
        }

        private void CheckUnsubscribers()
        {
            Observers.RemoveAll(Unsubscribers.Contains);
            Unsubscribers.Clear();
        }
    }
}

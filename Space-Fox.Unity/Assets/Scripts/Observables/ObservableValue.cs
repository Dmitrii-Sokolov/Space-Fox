using System;
using UnityEngine;

namespace SpaceFox
{
    [Serializable]
    public class ObservableValue<T> :
        IObservableValue<T>,
        IReadOnlyObservableValue<T>,
        IObservable<T>,
        IObservable<(T, T)>,
        IDisposable
    {
        private readonly ObserversComposer<T> ObserversComposer = new();
        private readonly ObserversComposer<(T, T)> MemorableObserversComposer = new();

        [SerializeField] private T TValue = default;

        public T Value
        {
            get
            {
                return TValue;
            }
            set
            {
                if (!Equals(value, TValue))
                {
                    var oldValue = TValue;
                    TValue = value;

                    MemorableObserversComposer.OnNext((oldValue, TValue));
                    ObserversComposer.OnNext(value);
                }
            }
        }

        public ObservableValue() : this(default)
        {
        }

        public ObservableValue(T value)
            => TValue = value;

        public IDisposable Subscribe(IObserver<(T, T)> observer)
            => MemorableObserversComposer.Subscribe(observer);

        public IDisposable Subscribe(IObserver<T> observer)
            => Subscribe(observer, true);

        public IDisposable Subscribe(IObserver<T> observer, bool invokeOnSubscribtion = true)
        {
            if (invokeOnSubscribtion)
                observer.OnNext(Value);

            return ObserversComposer.Subscribe(observer);
        }

        public void Dispose()
        {
            ObserversComposer.Dispose();
            MemorableObserversComposer.Dispose();
        }
    }
}

using System;
using UnityEngine;

namespace SpaceFox
{
    public abstract class ObservableValue : DisposableComposer
    {
    }

    [Serializable]
    public class ObservableValue<T> :
        ObservableValue,
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
                    ObserversComposer.OnNext(TValue);
                }
            }
        }

        public IReadOnlyObservableValue<T> ReadOnly => this;

        public ObservableValue() : this(default)
        {
            ObserversComposer.While(this);
            MemorableObserversComposer.While(this);
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
    }
}

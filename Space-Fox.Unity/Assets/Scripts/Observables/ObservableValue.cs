using System;
using UnityEngine;

namespace SpaceFox
{
    public abstract class ObservableValue
    {
#if UNITY_EDITOR
        public abstract string[] ValueFieldNames { get; }
        public abstract void InvokeCallbacks();
#endif
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

#if UNITY_EDITOR
        private T TOldValue = default;
#endif

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
#if UNITY_EDITOR
                    TOldValue = TValue;
#endif

                    var oldValue = TValue;
                    TValue = value;

                    MemorableObserversComposer.OnNext((oldValue, TValue));
                    ObserversComposer.OnNext(TValue);
                }
            }
        }

#if UNITY_EDITOR
        public override string[] ValueFieldNames => new string[] {nameof(TValue) };

        public override void InvokeCallbacks()
        {
            MemorableObserversComposer.OnNext((TOldValue, TValue));
            ObserversComposer.OnNext(TValue);
        }
#endif

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

using System;

namespace SpaceFox
{
    public interface IReadOnlyObservableValue<T> : ILazyObservable<T>, IObservable<(T, T)>
    {
        public T Value { get; }
    }
}

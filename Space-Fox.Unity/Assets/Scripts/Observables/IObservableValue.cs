using System;

namespace SpaceFox
{
    public interface IObservableValue<T> : ILazyObservable<T>, IObservable<(T, T)>
    {
        public T Value { get; set; }
    }
}

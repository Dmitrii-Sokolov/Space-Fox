using System;

namespace SpaceFox
{
    public interface ILazyObservable<T> : IObservable<T>
    {
        IDisposable Subscribe(IObserver<T> observer, bool invokeOnSubscribtion = true);
    }
}

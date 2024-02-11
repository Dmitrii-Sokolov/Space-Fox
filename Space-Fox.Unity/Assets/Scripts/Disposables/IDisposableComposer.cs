using System;

namespace SpaceFox
{
    public interface IDisposableComposer : IDisposable
    {
        bool IsDisposed { get; }

        void AddDisposable(IDisposable disposable);
    }
}

using System;

namespace SpaceFox
{
    public interface IDisposableComposer : IDisposable
    {
        void AddDisposable(IDisposable disposable);
    }
}

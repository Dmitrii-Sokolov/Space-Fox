using System;
using System.Collections.Generic;

namespace SpaceFox
{
    public class DisposableComposer : IDisposableComposer
    {
        private readonly List<IDisposable> Disposables = new();
        private bool IsDisposed = false;

        public void AddDisposable(IDisposable disposable)
            => Disposables.Add(disposable);

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            Disposables.ForEach(x => x.Dispose());
            Disposables.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFox
{
    public class DisposableComposer : IDisposableComposer
    {
        private readonly List<IDisposable> Disposables = new();

        public bool IsDisposed { get; private set; } = false;

        public DisposableComposer(params IDisposable[] disposables)
            => Disposables.AddRange(disposables);

        public void AddDisposable(IDisposable disposable)
        {
            if (IsDisposed)
            {
                disposable.Dispose();

                Debug.LogWarning($"Subscription after disposing");
            }
            else
            {
                Disposables.Add(disposable);
            }
        }

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

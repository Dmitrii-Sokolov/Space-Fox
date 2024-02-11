using System;
using UnityEngine;

namespace SpaceFox
{
    public abstract class DisposableMonoBehaviour : MonoBehaviour, IDisposableComposer
    {
        private readonly DisposableComposer DisposableComposer = new();

        public bool IsDisposed => DisposableComposer.IsDisposed;

        public void AddDisposable(IDisposable disposable)
            => DisposableComposer.AddDisposable(disposable);

        private void Awake()
        {
            if (IsDisposed)
                return;

            AwakeBeforeDestroy();
        }

        protected virtual void AwakeBeforeDestroy()
        {
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            DisposableComposer.Dispose();

            Destroy(this);
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }
    }
}

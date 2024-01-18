using System;
using UnityEngine;

namespace SpaceFox
{
    public class DisposableMonoBehaviour : MonoBehaviour, IDisposableComposer
    {
        private readonly DisposableComposer DisposableComposer = new();
        private bool IsDisposed = false;

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

            IsDisposed = true;

            DisposableComposer.Dispose();

            Destroy(this);
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }
    }
}

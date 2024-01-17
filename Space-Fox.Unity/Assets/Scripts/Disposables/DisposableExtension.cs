using System;

namespace SpaceFox
{
    public static class DisposableExtension
    {
        public static void While(this IDisposable disposable, IDisposableComposer composer)
            => composer.AddDisposable(disposable);
    }
}

using System;
using UnityEngine;

namespace SpaceFox
{
    public static class ObservableExtension
    {
        public static IDisposable Subscribe<T>(
            this IObservable<T> observable,
            Action<T> next,
            Action completed = null)
            => Subscribe(observable, next, DefaultErrorCallback, completed);

        public static IDisposable Subscribe<T>(
            this IObservable<T> observable, 
            Action<T> next, 
            Action<Exception> error, 
            Action completed = null)
            => observable.Subscribe(new SimpleObserver<T>(next, error, completed));

        public static IDisposable Subscribe<T>(
            this ILazyObservable<T> observable,
            Action<T> next,
            bool invokeOnSubscribtion = true,
            Action completed = null)
            => Subscribe(observable, next, DefaultErrorCallback, invokeOnSubscribtion, completed);

        public static IDisposable Subscribe<T>(
            this ILazyObservable<T> observable, 
            Action<T> next,
            Action<Exception> error,
            bool invokeOnSubscribtion = true,
            Action completed = null)
            => observable.Subscribe(new SimpleObserver<T>(next, error, completed), invokeOnSubscribtion);

        private static void DefaultErrorCallback(Exception error)
            => Debug.LogException(error);
    }
}

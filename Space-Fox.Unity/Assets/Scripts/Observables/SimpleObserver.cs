using System;

namespace SpaceFox
{
    public class SimpleObserver<T> : IObserver<T>
    {
        private readonly Action Completed;
        private readonly Action<Exception> Error;
        private readonly Action<T> Next;

        public SimpleObserver(Action<T> next, Action<Exception> error, Action completed)
        {
            Completed = completed;
            Error = error;
            Next = next;
        }

        public void OnCompleted()
            => Completed?.Invoke();

        public void OnError(Exception error)
            => Error?.Invoke(error);

        public void OnNext(T value)
            => Next?.Invoke(value);
    }
}

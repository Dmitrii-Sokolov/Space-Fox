using System;
using System.Collections.Generic;
using ModestTree;

namespace SpaceFox
{
    public class SimplePool<TView, TData>
    {
        private Func<TView> Factory;
        private Action<TView, TData> OnGet;
        private Action<TView> OnReturn;

        private Queue<TView> Pool = new();

        public SimplePool(Func<TView> factory, Action<TView, TData> onGet, Action<TView> onReturn)
        {
            Factory = factory;
            OnGet = onGet;
            OnReturn = onReturn;
        }

        public TView Get(TData data)
        {
            var result = Pool.IsEmpty() ? Factory() : Pool.Dequeue();
            OnGet(result, data);
            return result;
        }

        public void Return(TView element)
        {
            OnReturn(element);
            Pool.Enqueue(element);
        }
    }
}

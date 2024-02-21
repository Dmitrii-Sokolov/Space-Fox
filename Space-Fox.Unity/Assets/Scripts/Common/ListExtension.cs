using System.Collections.Generic;

namespace SpaceFox
{
    public static class ListExtension
    {
        public static List<T> MakeCopy<T>(this List<T> list)
        {
            var newList = new List<T>(list.Count);
            newList.AddRange(list);
            return newList;
        }

        public static int AddAndReturnIndex<T>(this List<T> list, T value)
        {
            list.Add(value);
            return list.Count - 1;
        }
    }
}

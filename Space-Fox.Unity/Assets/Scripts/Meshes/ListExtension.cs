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
    }
}

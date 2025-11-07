using System.Collections.Generic;
using UnityEngine;

public static class CollectionExtensions
{
    public static T RandomElement<T>(this HashSet<T> set)
    {
        if (set.Count == 0)
            return default;

        int index = Random.Range(0, set.Count);
        int i = 0;
        foreach (var element in set)
        {
            if (i++ == index)
                return element;
        }

        return default;
    }
}


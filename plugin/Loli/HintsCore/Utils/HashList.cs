using System.Collections.Generic;

namespace Loli.HintsCore.Utils;

public class HashList<T> : List<T>
{
    public new bool Add(T item)
    {
        if (Contains(item))
            return false;

        base.Add(item);
        return true;
    }

    public new bool Insert(int index, T item)
    {
        if (Contains(item))
            return false;

        base.Insert(index, item);
        return true;
    }
}
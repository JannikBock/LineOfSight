using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]

// Stores Objects that are layed out in a grid. Can also handle negative indezes.
public class GridMap<T>
{
    private SortedDictionary<Vector2Int, T> cells = new SortedDictionary<Vector2Int, T>(new Vector2IntEqualityComparer());

    public T this[int x, int y]
    {
        get
        {
            return Get(new Vector2Int(x, y));
        }
        set
        {
            Set(new Vector2Int(x, y), value);
        }
    }

    public T Get(Vector2Int pos)
    {
        T result;
        cells.TryGetValue(pos, out result);

        return result;
    }

    public void Set(Vector2Int pos, T obj)
    {
        cells.Remove(pos);

        if(obj != null && !obj.Equals(default(T))) //dont store defaults but remove instead
        {
            cells[pos] = obj;
        }

    }

    public IEnumerable<KeyValuePair<Vector2Int, T>> GetAll()
    {
        return cells;
    }

    public KeyValuePair<Vector2Int, T> GetByIndex(int i)
    {
        return cells.ElementAtOrDefault(i);
    }



}

class Vector2IntEqualityComparer : IComparer<Vector2Int>
{
    public int Compare(Vector2Int v1, Vector2Int v2)
    {
        if (ReferenceEquals(v1, v2))
            return 0;

        if (v1 == null)
            return -1;
        if (v1 == null)
            return 1;

        return v1.x*1000 - v2.x * 1000 + v1.y - v2.y;
    }

    public bool Equals(Vector2Int v1, Vector2Int v2)
    {
        if (ReferenceEquals(v1, v2))
            return true;

        if (v2 == null || v1 == null)
            return false;

        return v1.x == v2.x
            && v1.y == v2.y;
    }

    //public int GetHashCode(Vector2Int v) => v.x;
}

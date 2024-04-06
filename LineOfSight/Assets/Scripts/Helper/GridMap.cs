using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]

// Stores Objects that are layed out in a grid. Can also handle negative indezes.
public class GridMap<T>
{
    private Dictionary<Vector2Int, T> cells = new Dictionary<Vector2Int, T>();

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



}

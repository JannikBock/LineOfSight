using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension
{
    public static T RequireComponent<T>(this GameObject obj)
    {
        T result = obj.GetComponent<T>();
        if(result == null)
        {
            throw new Exception($"{typeof(T)} Component not fount on {obj.name}");
        }
        return result;
    }
}

public static class TransformExtension
{
    public static T RequireComponent<T>(this Transform obj)
    {
        T result = obj.GetComponent<T>();
        if (result == null)
        {
            throw new Exception($"{typeof(T)} Component not fount on {obj.name}");
        }
        return result;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExtension
{
    public static Vector3 ToVector3(this Vector2Int vector2)
    {
        return new Vector3(vector2.x, vector2.y, 0);
    }
}

public static class Vector3Extension
{
    public static Vector2Int ToVector2Int(this Vector3 vector3)
    {
        return new Vector2Int(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.y));
    }

    public static Vector2 ToVector2(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }
}

public static class Vector2Extension
{

    public static Vector2 ToVector3(this Vector2 vector2)
    {
        return new Vector3(vector2.x, vector2.y,0f);
    }
}

using System;
using UnityEngine;

[Serializable]
public class TilePersistance
{
    public TilePersistance()
    {

    }
    public TilePersistance(Tile tile)
    {
        pos = tile.Pos;
    }
    public Vector2Int pos;

}


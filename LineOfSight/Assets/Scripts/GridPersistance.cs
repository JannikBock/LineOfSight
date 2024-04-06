
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public class GridPersistance
{
    public GridPersistance()
    {

    }
    public GridPersistance(GridMap<Tile> grid)
    {
        tiles = grid.GetAll().Select(x => new TilePersistance(x.Value)).ToArray();
    }
    public TilePersistance[] tiles;

}


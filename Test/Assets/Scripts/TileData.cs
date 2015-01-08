using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;

[System.Serializable]
public struct TileData
{
    public TileData(int ID, TileType type, GridCoord coord, int health)
    {
        this.ID = ID;
        this.tileType = type;
        this.coord = coord;
        this.health = health;
    }

    public int ID;
    public TileType tileType;
    public GridCoord coord;
    public int health;
}
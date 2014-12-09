using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;

[System.Serializable]
public struct TileData
{
    public TileData(TileType type, float x, float y, int health)
    {
        this.tileType = type;
        this.x = x;
        this.y = y;
        this.health = health;
    }

    public TileType tileType;
    public float x;
    public float y;
    public int health;
}
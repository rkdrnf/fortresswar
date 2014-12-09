using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;

[System.Serializable]
public struct TileData
{
    public TileData(int ID, TileType type, float x, float y, int health)
    {
        this.ID = ID;
        this.tileType = type;
        this.x = x;
        this.y = y;
        this.health = health;
    }

    public int ID;
    public TileType tileType;
    public float x;
    public float y;
    public int health;
}
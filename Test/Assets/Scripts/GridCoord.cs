using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ProtoBuf;


[System.Serializable]
[ProtoContract]
public struct GridCoord
{
    [ProtoMember(1)]
    public short x;
    [ProtoMember(2)]
    public short y;

    public GridCoord(short x, short y)
    {
        this.x = x;
        this.y = y;
    }

    public static GridCoord operator - (GridCoord a, GridCoord b)
    {
        return new GridCoord((short)(a.x - b.x), (short)(a.y - b.y));
    }

    public int ToIndex(ushort mapWidth)
    {
        return x + y * mapWidth;
    }

    public static GridCoord ToCoord(Vector2 position)
    {
        return new GridCoord((short)Mathf.FloorToInt(position.x + 0.5f), (short)Mathf.FloorToInt(position.y + 0.5f));
    }

    public static GridCoord ToCoordDown(Vector2 position)
    {
        return new GridCoord((short)Mathf.FloorToInt(position.x - 0.5f), (short)Mathf.FloorToInt(position.y - 0.5f));
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    public GridCoord Up()
    {
        return new GridCoord(x, (short)(y + 1));
    }

    public GridCoord Down()
    {
        return new GridCoord(x, (short)(y - 1));
    }

    public GridCoord Left()
    {
        return new GridCoord((short)(x - 1), y);
    }

    public GridCoord Right()
    {
        return new GridCoord((short)(x + 1), y);
    }

    public override string ToString()
    {
        return string.Format("X: {0} Y: {1}", x, y);
    }
}

[System.Serializable]
public struct GridCoordDist
{
    public GridCoord coord;
    public float distance;

    public GridCoordDist(GridCoord coord, float dist)
    {
        this.coord = coord;
        this.distance = dist;
    }

    public override string ToString()
    {
        return string.Format("Coord: {0} Dist: {1}", coord, distance);
    }
}
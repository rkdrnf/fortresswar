using UnityEngine;
using System.Collections;
using Const;
using System.Collections.Generic;

public class MapData : ScriptableObject
{
    public string mapName;
    public TileSet tileSet;
    public TileData[] tiles = new TileData[0];

    public void init(string mapName, TileSet tileSet, List<Tile> tileList)
    {
        this.mapName = mapName;
        this.tileSet = tileSet;
        this.tiles = ImportTiles(tileList);
    }

    private TileData[] ImportTiles(List<Tile> tileList)
    {
        List<TileData> tileDataList = new List<TileData>();
        foreach(Tile tile in tileList)
        {
            tileDataList.Add(new TileData(tile.tileType, tile.transform.position.x, tile.transform.position.y, tile.health));
        }

        return tileDataList.ToArray();
    }
}



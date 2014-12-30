using UnityEngine;
using System.Collections;
using Const;
using System.Collections.Generic;

public class MapData : ScriptableObject
{
    public string mapName;
    public int mapWidth;
    public int mapHeight;
    public float tileSize = 1;
    public TileSet tileSet;
    public TileData[] tiles = new TileData[0];
    public Sprite backgroundImage;

    public void init(string mapName, int mapWidth, int mapHeight, float tileSize, TileSet tileSet, Dictionary<int, Tile> tileList, Sprite background)
    {
        this.mapName = mapName;
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.tileSize = tileSize;
        this.tileSet = tileSet;
        this.tiles = ImportTiles(tileList);
        this.backgroundImage = background;
    }

    private TileData[] ImportTiles(Dictionary<int, Tile> tileList)
    {
        List<TileData> tileDataList = new List<TileData>();
        foreach(var tile in tileList)
        {
            tileDataList.Add(new TileData(tile.Value.ID, tile.Value.tileType, tile.Value.transform.localPosition.x, tile.Value.transform.localPosition.y, tile.Value.health));
        }

        return tileDataList.ToArray();
    }
}



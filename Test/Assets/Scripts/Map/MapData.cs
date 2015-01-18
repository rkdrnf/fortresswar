﻿using UnityEngine;
using System.Collections;
using Const;
using System.Collections.Generic;
using System.Linq;
using Data;

public class MapData : ScriptableObject
{
    public string mapName;
    public int mapWidth;
    public int mapHeight;
    public float tileSize = 1;
    public TileSet tileSet;
    public Tile[] tiles = new Tile[0];
    public Sprite backgroundImage;

    public void init(string mapName, int mapWidth, int mapHeight, float tileSize, TileSet tileSet, IEnumerable<Tile> tileList, Sprite background)
    {
        this.mapName = mapName;
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.tileSize = tileSize;
        this.tileSet = tileSet;
        this.tiles = tileList.ToArray();
        this.backgroundImage = background;
    }

    /*
    private Tile[] ImportTiles(IEnumerable<Tile> tileList)
    {
        List<TileData> tileDataList = new List<TileData>();
        foreach(var tile in tileList)
        {
            tileDataList.Add(new TileData(tile.m_ID, tile.m_tileType, tile.m_coord, tile.m_health));
        }

        return tileDataList.ToArray();
    }
     * */
}



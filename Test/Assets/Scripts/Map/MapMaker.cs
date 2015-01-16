using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapMaker : MonoBehaviour {

    public int m_tileSize = 1;

    public string m_name;
    public int m_width = 320;
    public int m_height = 160;

    public TileSet m_tileSet;
    public Dictionary<GridCoord, Tile> m_tiles;

    public Tile m_brushTile;
    public float m_brushSize;

    public Sprite m_backgroundImage;

    bool m_showGrid = false;

    
    public void Clear()
    {
        Tile[] tiles = GetComponentsInChildren<Tile>();

        foreach (var tile in tiles)
        {
            if (tile != null)
                DestroyImmediate(tile.gameObject);
        }

        m_tiles.Clear();
    }

    public void ReloadTiles()
    {
        Tile[] tiles = GetComponentsInChildren<Tile>();

        m_tiles = new Dictionary<GridCoord, Tile>();
        for (int i = 0; i < tiles.Length; ++i)
        {
            m_tiles.Add(tiles[i].m_coord, tiles[i]);
        }
    }

    public void Add(Tile tile)
    {
        m_tiles.Add(tile.m_coord, tile);
        tile.transform.parent = this.transform;
    }

    public void Remove(Tile tile)
    {
        m_tiles.Remove(tile.m_coord);
        DestroyImmediate(tile.gameObject);
    }

    public void Apply()
    {
        LoadBackground(m_backgroundImage);
    }

    public void Load(MapData mapData)
    {
        m_width = mapData.mapWidth;
        m_height = mapData.mapHeight;
        m_name = mapData.mapName;

        LoadBackground(mapData.backgroundImage);
        LoadTiles(mapData);
        LoadBuildings(mapData);
    }

    public void LoadTiles(MapData mData)
    {
        foreach (TileData tileData in mData.tiles)
        {
            Tile tile = (Tile)Instantiate(mData.tileSet.tiles[(int)tileData.tileType], tileData.coord.ToVector2(), Quaternion.identity);
            tile.InitForMaker(tileData);

            Add(tile);
        }
    }

    public void LoadBuildings(MapData mData)
    {

    }

    public void LoadBackground(Sprite image)
    {
        m_backgroundImage = image;
        GetComponent<SpriteRenderer>().sprite = image;
    }


    public void InitMapData(ref MapData mapAsset)
    {
        mapAsset.init(m_name, m_width, m_height, m_tileSize, m_tileSet, m_tiles.Values, m_backgroundImage);
    }

    public void ToggleGrid()
    {
        m_showGrid = !m_showGrid;
    }

	void OnDrawGizmos()
    {
        DrawGrid();

        DrawMapBoundary();
	}

    void DrawMapBoundary()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector2(-m_width / 2f, -m_height / 2f), new Vector2(-m_width / 2f, m_height / 2f));
        Gizmos.DrawLine(new Vector2(-m_width / 2f, m_height / 2f), new Vector2(m_width / 2f, m_height / 2f));
        Gizmos.DrawLine(new Vector2(m_width / 2f, m_height / 2f), new Vector2(m_width / 2f, -m_height / 2f));
        Gizmos.DrawLine(new Vector2(m_width / 2f, -m_height / 2f), new Vector2(-m_width / 2f, -m_height / 2f));
    }

    void DrawGrid()
    {
        if (m_showGrid == false) return;

        Vector3 pos = Camera.current.transform.position;

        Gizmos.color = new Color(180f, 180f, 180f, 90f);
        for (float y = pos.y - 50.0f; y < pos.y + 50.0f; y += m_tileSize)
        {
            Gizmos.DrawLine(new Vector3(-100.0f, Mathf.Floor(y / m_tileSize) * m_tileSize + 0.5f, 0.0f),
                            new Vector3(100.0f, Mathf.Floor(y / m_tileSize) * m_tileSize + 0.5f, 0.0f));
        }

        for (float x = pos.x - 50.0f; x < pos.x + 50.0f; x += m_tileSize)
        {
            Gizmos.DrawLine(new Vector3(Mathf.Floor(x / m_tileSize) * m_tileSize + 0.5f, -100.0f, 0.0f),
                            new Vector3(Mathf.Floor(x / m_tileSize) * m_tileSize + 0.5f, 100.0f, 0.0f));
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Structure;
using Data;
using Const.Structure;

[ExecuteInEditMode]
public class MapMaker : MonoBehaviour {

    public int m_tileSize = 1;
    public int m_chunkSize = 8;

    public string m_name;
    public int m_width = 320;
    public int m_height = 160;

    public TileSet m_tileSet;
    public Dictionary<TileType, TileData> m_tileTypeDic = new Dictionary<TileType, TileData>();
    
    public Dictionary<GridCoord, Tile> m_tiles = new Dictionary<GridCoord,Tile>();

    public Dictionary<GridCoord, TileChunk> m_chunks = new Dictionary<GridCoord, TileChunk>();

    public TileChunk m_chunkPrefab;

    public TileData m_brushTile;
    public float m_brushSize;

    public Sprite m_backgroundImage;

    bool m_showGrid = false;

    void OnEnable()
    {
        Clear();

        
    }

    void Start()
    {
        
    }
    
    public void Clear()
    {
        TileChunk[] tileChunks = GetComponentsInChildren<TileChunk>();

        foreach (var chunk in tileChunks)
        {
            if (chunk != null)
                DestroyImmediate(chunk.gameObject);
        }

        m_tileTypeDic.Clear();
        m_tiles.Clear();
        m_chunks.Clear();
        GetComponent<SpriteRenderer>().sprite = null;

        foreach (TileData tData in m_tileSet.tiles)
        {
            m_tileTypeDic.Add(tData.type, tData);
        }
    }

    public void ReloadChunks()
    {
        TileChunk[] tileChunks = GetComponentsInChildren<TileChunk>();

        foreach (var chunk in tileChunks)
        {
            m_chunks.Add(chunk.m_coord, chunk);
            Debug.Log("coord: " + chunk.m_coord);
        }
    }

    public void Add(Tile tile)
    {
        m_tiles.Add(tile.m_coord, tile);

        TileChunk chunk = FindChunk(tile.m_coord);


        if (chunk == null)
        {
            chunk = AddChunk(ToChunkCoord(tile.m_coord));
        }

        chunk.AddBlock(tile.m_coord, tile);
    }

    public TileChunk AddChunk(GridCoord coord)
    {
        TileChunk chunk = (TileChunk)Instantiate(m_chunkPrefab, coord.ToVector2(), Quaternion.identity);
        chunk.Init(coord, m_chunkSize);

        m_chunks[coord] = chunk;
        chunk.transform.parent = transform;
        
        return chunk;
    }

    public TileChunk FindChunk(GridCoord coord)
    {
        GridCoord chunkCoord = ToChunkCoord(coord);
        if (m_chunks.ContainsKey(chunkCoord))
            return m_chunks[chunkCoord];

        return null;
    }

    public GridCoord ToChunkCoord(GridCoord coord)
    {
        return new GridCoord(coord.x - mod(coord.x, m_chunkSize), coord.y - mod(coord.y, m_chunkSize));
    }

    int mod(int a, int n)
    {
        int result = a % n;
        if ((a < 0 && n > 0) || (a > 0 && n < 0))
            result += n;
        return result % n;
    }

    public void Remove(GridCoord coord)
    {
        if (m_tiles.ContainsKey(coord))
        {
            Tile tile = m_tiles[coord];
            m_tiles.Remove(coord);
            tile.RemoveForMaker();
        }
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
        m_chunkSize = mapData.chunkSize;
        m_tileSize = mapData.tileSize;
        m_tileSet = mapData.tileSet;


        LoadBackground(mapData.backgroundImage);
        LoadTiles(mapData);
        LoadBuildings(mapData);
    }

    public void LoadTiles(MapData mData)
    {
        foreach (Tile tile in mData.tiles)
        {
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
        mapAsset.init(m_name, m_width, m_height, m_chunkSize, m_tileSize, m_tileSet, m_tiles.Values, m_backgroundImage);
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

    public void GenTerrain()
    {
        for (int px = 0; px < m_width; px++)
        {
            int stone = Noise(px, 0, 80, 15, 1);
            stone += Noise(px, 0, 50, 30, 1);
            stone += Noise(px, 0, 10, 10, 1);
            stone += 75;

            int dirt = Noise(px, 0, 100, 35, 1);
            dirt += Noise(px, 0, 50, 30, 1);
            dirt += 75;

            for (int py = 0; py < m_height; py++)
            {
                int x = px - (m_width / 2);
                int y = py - (m_height / 2);

                do
                {
                    if (py < stone)
                    {
                        //Cave
                        if (Noise(px, py * 2, 16, 14, 1) > 10)
                            break;

                        //Dirt
                        if (Noise(px, py, 12, 16, 1) > 10)
                        {
                            Add(GenTile(TileType.DIRT, new GridCoord(x, y)));
                            break;
                        }

                        Add(GenTile(TileType.STONE, new GridCoord(x, y)));
                        break;
                    }

                    if (py < dirt)
                    {
                        Add(GenTile(TileType.DIRT, new GridCoord(x, y)));
                        break;
                    }
                } while (false);
            }
        }
    }
    
    public Tile GenTile(TileType type, GridCoord coord)
    {
        Tile tile = new Tile();
        tile.InitForMaker(m_tileTypeDic[type], coord);
        return tile;
    }

    int Noise(int x, int y, float scale, float mag, float exp)
    {
        return (int)(Mathf.Pow((Mathf.PerlinNoise(x / scale, y / scale) * mag), (exp)));
    }
}

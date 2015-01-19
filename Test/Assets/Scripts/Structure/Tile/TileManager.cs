using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Data;
using Const.Structure;
namespace Architecture
{
    public class TileManager : MonoBehaviour
    {
        private static TileManager instance;

        public static TileManager Inst
        {
            get
            {
                if (instance == null)
                {
                    instance = new TileManager();
                }
                return instance;
            }
        }

        Dictionary<GridCoord, Tile> m_tileMap;
        Dictionary<int, Tile> m_tileIDMap;

        int m_tileIndex;

        public TileSet m_tileSet; // scene init
        private Dictionary<TileType, TileData> m_tileDataDic;
        public LayerMask m_tileLayer;

        public TileChunkManager m_tileChunkManager = null; // scene init
        
        void Awake()
        {
            instance = this;
            m_tileIndex = 0;
            m_tileMap = new Dictionary<GridCoord, Tile>();
            m_tileIDMap = new Dictionary<int, Tile>();
            m_tileDataDic = new Dictionary<TileType, TileData>();

            foreach(TileData tData in m_tileSet.tiles)
            {
                m_tileDataDic.Add(tData.type, tData);
            }
            
            m_tileLayer = LayerMask.GetMask("Tile");
        }

        public void Clear()
        {
            m_tileIndex = 0;
            m_tileMap.Clear();
            m_tileIDMap.Clear();
            m_tileChunkManager.Clear();
        }

        public void Add(Tile tile)
        {
            m_tileMap.Add(tile.m_coord, tile);
            m_tileIDMap.Add(tile.m_ID, tile);

            m_tileChunkManager.AddBlock(tile);

            m_tileIndex++;
        }

        public void Remove(Tile tile)
        {
            if (m_tileMap.ContainsKey(tile.m_coord) && m_tileMap[tile.m_coord] == tile)
                m_tileMap.Remove(tile.m_coord);

            m_tileIDMap.Remove(tile.m_ID);

            m_tileChunkManager.RemoveBlock(tile);
        }

        public Tile Get(GridCoord coord)
        {
            if (m_tileMap.ContainsKey(coord))
                return m_tileMap[coord];
            else
                return null;
        }

        public Tile Get(int ID)
        {
            if (m_tileIDMap.ContainsKey(ID))
                return m_tileIDMap[ID];
            else
                return null;
        }

        public TileData GetTileData(TileType type)
        {
            return m_tileDataDic[type];
        }

        public List<Tile> GetTiles()
        {
            return m_tileMap.Values.ToList();
        }
    }
}

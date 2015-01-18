using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

        public LayerMask m_tileLayer;

        void Awake()
        {
            instance = this;
            m_tileMap = new Dictionary<GridCoord, Tile>();
            m_tileLayer = LayerMask.GetMask("Tile");
        }

        public void Clear()
        {
            m_tileMap.Clear();
        }

        public void Add(Tile tile)
        {
            m_tileMap.Add(tile.m_coord, tile);

            //tile.transform.parent = this.transform;
        }

        public void Remove(Tile tile)
        {
            if (m_tileMap.ContainsKey(tile.m_coord) && m_tileMap[tile.m_coord] == tile)
                m_tileMap.Remove(tile.m_coord);
        }

        public Tile Get(GridCoord coord)
        {
            if (m_tileMap.ContainsKey(coord))
                return m_tileMap[coord];
            else
                return null;
        }
    }
}

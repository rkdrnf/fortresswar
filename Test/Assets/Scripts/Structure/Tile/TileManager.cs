using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Data;
using Const.Structure;
using Lights;
namespace Architecture
{
    public class TileManager : StructureManager<Tile, TileType, TileData>
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

        

        ushort m_tileIndex;

        public TileSet m_tileSet = null; // scene init
        public LayerMask m_tileLayer;

        public TileChunkManager m_tileChunkManager = null; // scene init

        void Awake()
        {
            instance = this;
            m_tileIndex = 0;
            m_structureMap = new Dictionary<GridCoord, Tile>();
            m_structureIDMap = new Dictionary<ushort, Tile>();
            m_structureDataDic = new Dictionary<TileType, TileData>();

            foreach(TileData tData in m_tileSet.tiles)
            {
                m_structureDataDic.Add(tData.type, tData);
            }
            
            m_tileLayer = LayerMask.GetMask("Tile");
        }

        public override void Clear()
        {
            m_tileIndex = 0;
            m_structureMap.Clear();
            m_structureIDMap.Clear();
            m_tileChunkManager.Clear();
        }

        public override void New(Tile tile)
        {
            Tile newTile = new Tile(m_tileIndex, tile);
            Add(newTile);
            m_tileIndex++;
        }

        public override void Add(Tile tile)
        {
            m_structureMap.Add(tile.m_coord, tile);
            m_structureIDMap.Add(tile.GetID(), tile);

            m_tileChunkManager.AddBlock(tile);

            ShadowPane.Inst.UpdateLight(tile.m_coord, 1f);
        }

        public override void Remove(Tile tile)
        {
            if (m_structureMap.ContainsKey(tile.m_coord) && m_structureMap[tile.m_coord] == tile)
                m_structureMap.Remove(tile.m_coord);

            m_structureIDMap.Remove(tile.GetID());

            m_tileChunkManager.RemoveBlock(tile);
        }
    }
}

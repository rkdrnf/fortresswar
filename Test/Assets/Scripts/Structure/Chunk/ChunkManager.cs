using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util;
using Data;
using UnityEngine;

namespace Architecture
{
    public class ChunkManager<CT, T, DT> : MonoBehaviour
        where CT : PolygonGenerator<T, DT>
        where T : Structure<T, DT>
        where DT : StructureData
    {
        public Dictionary<GridCoord, CT> m_chunks;
        public int m_chunkSize;
        public CT m_chunkPrefab;

        void Awake()
        {
            m_chunks = new Dictionary<GridCoord, CT>();
        }

        public CT FindChunk(GridCoord coord)
        {
            GridCoord chunkCoord = ToChunkCoord(coord);
            if (m_chunks.ContainsKey(chunkCoord))
                return m_chunks[chunkCoord];

            return null;
        }

        public GridCoord ToChunkCoord(GridCoord coord)
        {
            return new GridCoord((short)(coord.x - Calc.mod(coord.x, m_chunkSize)), (short)(coord.y - Calc.mod(coord.y, m_chunkSize)));
        }

        public void AddBlock(T block)
        {
            CT chunk = FindChunk(block.m_coord);


            if (chunk == null)
            {
                chunk = AddChunk(ToChunkCoord(block.m_coord));
            }

            chunk.AddBlock(block.m_coord, block);
        }

        public void RemoveBlock(T block)
        {
            CT chunk = FindChunk(block.m_coord);

            chunk.RemoveBlock(block);
        }

        public CT AddChunk(GridCoord coord)
        {
            CT chunk = (CT)MonoBehaviour.Instantiate(m_chunkPrefab, new Vector3(coord.x, coord.y, 3), Quaternion.identity);
            chunk.Init(coord, m_chunkSize);

            m_chunks[coord] = chunk;
            chunk.transform.parent = transform;

            return chunk;
        }

        public void Clear()
        {
            m_chunks.Clear();
        }
    }
}

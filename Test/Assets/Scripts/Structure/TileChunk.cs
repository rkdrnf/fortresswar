using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Data;
using Structure;

namespace Structure
{
    [ExecuteInEditMode]
    public class TileChunk : PolygonGenerator<Tile, TileData>
    {
        protected override UnityEngine.Vector2 GetTexture(Tile structure)
        {
            return new Vector2(structure.m_spriteIndex, structure.m_data.spriteRowIndex);
        }

        public override void AddBlock(GridCoord coord, Tile block)
        {
            blocks[coord.x - m_coord.x, coord.y - m_coord.y] = block;
            block.SetChunk(this);
            update = true;
        }

        public override void RemoveBlock(Tile block)
        {
            blocks[block.m_coord.x - m_coord.x, block.m_coord.y - m_coord.y] = null;
            update = true;
        }
    }
}

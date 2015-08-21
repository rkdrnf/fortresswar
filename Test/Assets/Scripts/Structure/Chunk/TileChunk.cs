using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Data;

namespace Architecture
{
    [ExecuteInEditMode]
    public class TileChunk : PolygonGenerator<Tile>
    {
        protected override UnityEngine.Vector2 GetTexture(Tile structure)
        {
            return new Vector2(structure.m_spriteIndex, structure.GetData().spriteRowIndex);
        }

        public override void AddBlock(GridCoord coord, Tile block)
        {
            blocks[coord.x - m_coord.x, coord.y - m_coord.y] = block;
            block.SetChunk(this);
            SendUpdate();
        }

        public override void RemoveBlock(Tile block)
        {
            blocks[block.m_coord.x - m_coord.x, block.m_coord.y - m_coord.y] = null;
            SendUpdate();
        }

        [ContextMenu("Refresh")]
        void Refresh()
        {
            base.Refresh();
        }
    }
}

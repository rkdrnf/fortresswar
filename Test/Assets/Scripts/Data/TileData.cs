using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const.Structure;
using UnityEngine;
using Const.Effect;

namespace Data
{ 
    public class TileData : StructureData<TileType>
    {
        public TileType type;
        public Sprite tileBack;

        protected override TileType GetDataKey()
        {
            return type;
        }
    }
}
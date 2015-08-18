using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Data;

namespace Architecture
{
    public abstract class StructureManager<T, ST, DT> : MonoBehaviour
        where T : StructureBase
        where DT : StructureData
    {

        protected Dictionary<GridCoord, T> m_structureMap;
        protected Dictionary<ushort, T> m_structureIDMap;
        protected Dictionary<ushort, bool> m_dirtyBitMap;
        protected Dictionary<ST, DT> m_structureDataDic;
        
        
        public abstract void Clear();

        public abstract void New(T tile);

        public abstract void Add(T tile);

        public abstract void Remove(T tile);

        public virtual T Get(GridCoord coord)
        {
            if (m_structureMap.ContainsKey(coord))
                return m_structureMap[coord];
            else
                return null;
        }

        public virtual T Get(ushort ID)
        {
            if (m_structureIDMap.ContainsKey(ID))
                return m_structureIDMap[ID];
            else
                return null;
        }

        public virtual DT GetData(ST type)
        {
            return m_structureDataDic[type];
        }

        public virtual List<T> GetStructures()
        {
            return m_structureMap.Values.ToList();
        }

        public virtual void SetDirtyBit(ushort id, bool set)
        {
            if (m_dirtyBitMap.ContainsKey(id))
            {
                m_dirtyBitMap.Add(id, set);
            }
        }

        public virtual List<T> GetDirtyStructures()
        {
            List<T> dirties = new List<T>();
            foreach (var id in m_dirtyBitMap.Keys.Where(key => m_dirtyBitMap[key] == true))
            {
                if (m_structureIDMap.ContainsKey(id))
                {
                    T structure = m_structureIDMap[id];
                    dirties.Add(structure);
                }
            }

            return dirties;
        }
    }
}

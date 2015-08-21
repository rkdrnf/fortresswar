using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Data
{
    public abstract class KeyValueDataSet<T> : ScriptableObject
        where T : KeyValueData
    {
        public T[] m_datas;

        public T[] GetDatas()
        {
            return m_datas;
        }
    }
}

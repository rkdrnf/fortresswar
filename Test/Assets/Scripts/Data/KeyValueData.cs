using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Data
{
    public abstract class KeyValueData<K> : ScriptableObject, KeyValueData
    {
        public object GetKey()
        {
            return GetDataKey();
        }

        protected abstract K GetDataKey();
    }

    public interface KeyValueData
    {
        object GetKey();
    }
}

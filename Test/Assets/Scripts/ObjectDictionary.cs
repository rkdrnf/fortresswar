using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class MonoObjManager<K, V> where V : UnityEngine.MonoBehaviour
{
    private Dictionary<K, V> objectDic;

    public MonoObjManager()
    {
        objectDic = new Dictionary<K, V>();
    }

    public V Get(K key)
    {
        if (objectDic.ContainsKey(key))
            return objectDic[key];

        return null;
    }

    public void Set(K key, V val)
    {
        objectDic.Add(key, val);
    }

    public void Remove(K key)
    {
        objectDic.Remove(key);
    }

    public bool Exists(K key)
    {
        return objectDic.ContainsKey(key);
    }

    public void Clear()
    {
        foreach(V val in objectDic.Values)
        {
            if (val != null && val.gameObject != null)
                UnityEngine.Object.Destroy(val.gameObject);
        }

        objectDic.Clear();
    }

    public Dictionary<K, V>.ValueCollection Values
    {
        get
        {
            return objectDic.Values;
        }
    }

    public Dictionary<K, V>.KeyCollection Keys
    {
        get
        {
            return objectDic.Keys;
        }
    }
}


public class ProjectileObjManager : MonoObjManager<long, Projectile>
{ }

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading;
using Client;
using Effect;
using Architecture;

public abstract class MemoryPool<T> : MonoBehaviour 
    where T : MonoBehaviour
{
    public T prefab = null; // scene init
    private int currentIndex;
    public int poolSize = 0; // scene init
    object poolLock = new object();

    void Awake()
    {
        Init(prefab, poolSize);
    }

    protected void Init(T prefab, int size)
    {
        prefabs = new List<T>(size);
        poolSize = size;

        for (int i = 0; i < size; i++)
        {
            T obj = (T)MonoBehaviour.Instantiate(prefab);
            obj.transform.parent = this.transform;
            prefabs.Add(obj);
            obj.gameObject.SetActive(false);
            
        }
    }

    public T Borrow()
    {
        for (int i = currentIndex; i < poolSize + currentIndex; i++)
        {
            int index = i % poolSize;

            T obj = prefabs[index];
            if (obj.gameObject.activeInHierarchy)
                continue;
            else
            {
                currentIndex = i;
                lock (poolLock)
                {
                    obj.gameObject.SetActive(true);
                    return GetObject(obj);
                }
            }
        }

        //Debug.Log(string.Format("Pool Exceeded {0}", typeof(T)));

        return default(T);
    }

    public T GetObject(T obj)
    {
        return obj;
    }

    List<T> prefabs;
}




using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading;

public abstract class MemoryPool<T>
{
    protected GameObject prefab;
    private int currentIndex;
    private int poolSize;
    object poolLock = new object();

    protected void Init(GameObject prefab, int size)
    {
        prefabs = new List<GameObject>(size);
        poolSize = size;

        for (int i = 0; i < size; i++)
        {
            prefabs.Add((GameObject)MonoBehaviour.Instantiate(prefab));
        }
    }

    public T Borrow()
    {
        for (int i = currentIndex; i < poolSize + currentIndex; i++)
        {
            int index = i % poolSize;

            GameObject obj = prefabs[index];
            if (obj.activeInHierarchy)
                continue;
            else
            {
                lock (poolLock)
                {
                    obj.SetActive(true);
                    return GetObject(obj);
                }
            }
        }

        Debug.Log("pool exceeded!!");

        throw new System.Exception("Pool Exceeded");
    }

    public abstract T GetObject(GameObject obj);

    List<GameObject> prefabs;
}

public class ParticlePool : MemoryPool<Particle2D>
{
    public ParticlePool(GameObject prefab, int size)
    {
        Init(prefab, size);
    }

    public override Particle2D GetObject(GameObject obj)
    {
        return obj.GetComponent<Particle2D>();
    }
}

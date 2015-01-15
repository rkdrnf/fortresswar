using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading;
using Client;
using Effect;

public abstract class MemoryPool<T> where T : MonoBehaviour
{
    protected T prefab;
    private int currentIndex;
    private int poolSize;
    object poolLock = new object();

    protected void Init(T prefab, int size)
    {
        prefabs = new List<T>(size);
        poolSize = size;

        for (int i = 0; i < size; i++)
        {
            T obj = (T)MonoBehaviour.Instantiate(prefab);
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

        Debug.Log(string.Format("Pool Exceeded {0}", typeof(T)));

        return default(T);
    }

    public T GetObject(T obj)
    {
        return obj;
    }

    List<T> prefabs;
}

public class ParticlePool : MemoryPool<Particle2D>
{
    public ParticlePool(Particle2D prefab, int size)
    {
        Init(prefab, size);
    }
}

public class ParticleSystem2DPool : MemoryPool<ParticleSystem2D>
{
    public ParticleSystem2DPool(ParticleSystem2D prefab, int size)
    {
        Init (prefab, size);
    }
}

public class AnimationEffectPool : MemoryPool<AnimationEffect>
{
    public AnimationEffectPool(AnimationEffect prefab, int size)
    {
        Init(prefab, size);
    }
}

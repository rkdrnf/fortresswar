using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using S2C = Communication.S2C;
using C2S = Communication.C2S;

public class ProjectileManager : MonoBehaviour {

    private ProjectileObjManager projectileObjManager;
    private long totalProjectileCount;

    private static ProjectileManager instance;

    public static ProjectileManager Inst
    {
        get
        {
            if (instance == null)
            {
                instance = new ProjectileManager();
            }
            return instance;
        }
    }

    public void Awake()
    {
        instance = this;
        totalProjectileCount = 0;
        projectileObjManager = new ProjectileObjManager();
    }

    public long GetUniqueKeyForNewProjectile()
    {
        return Interlocked.Increment(ref totalProjectileCount);
    }

    public Projectile Get(long projectileID)
    {
        return projectileObjManager.Get(projectileID);
    }

    public void Set(long projectileID, Projectile projectile)
    {
        projectileObjManager.Set(projectileID, projectile);
    }

    public void Clear()
    {
        projectileObjManager.Clear();
    }

    public void Remove(long projID)
    {
        projectileObjManager.Remove(projID);
    }
}

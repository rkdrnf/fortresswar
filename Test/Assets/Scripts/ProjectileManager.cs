using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using S2C = Packet.S2C;
using C2S = Packet.C2S;

public class ProjectileManager : MonoBehaviour {

    private ProjectileObjManager projectileObjManager;
    private long totalProjectileCount;

    private static ProjectileManager instance;

    public static ProjectileManager Instance
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

    [RPC]
    public void DestroyProjectile(string destroyProjectileJson)
    {
        S2C.DestroyProjectile pck = S2C.DestroyProjectile.Deserialize(destroyProjectileJson);

        if (Network.isServer)
        {
            Debug.Log(string.Format("[SERVER] projectile {0} Destroyed", pck.projectileID));
            Destroy((UnityEngine.Object)projectileObjManager.Get(pck.projectileID).gameObject);
            networkView.RPC("DestroyProjectile", RPCMode.Others, destroyProjectileJson);
            networkView.RPC("OnProjectileRemoved", RPCMode.All, destroyProjectileJson);
        }
        else if (Network.isClient)
        {
            Debug.Log(string.Format("[CLIENT] projectile {0} Destroyed", pck.projectileID));
            Destroy((UnityEngine.Object)projectileObjManager.Get(pck.projectileID).gameObject);
        }
    }

    [RPC]
    public void OnProjectileRemoved(string destroyProjectileJson)
    {
        S2C.DestroyProjectile pck = S2C.DestroyProjectile.Deserialize(destroyProjectileJson);

        projectileObjManager.Remove(pck.projectileID);
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
}

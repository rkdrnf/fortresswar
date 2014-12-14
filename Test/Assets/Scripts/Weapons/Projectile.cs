using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public long ID;
    
    public NetworkPlayer owner;

    void Awake()
    {
    }



    public void Destroy()
    {
        if (Network.isServer)
        {
            Packet.S2C.DestroyProjectile pck = new Packet.S2C.DestroyProjectile(ID);
            Game.Instance.DestroyProjectile(pck.Serialize());
        }

    }
}

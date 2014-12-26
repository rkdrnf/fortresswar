using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Server;

public abstract class Projectile : Weapon
{
    public long ID;
    public int owner;

    public int damage;
    public int range;

    public bool friendlyFire;
    public int power;

    protected Vector3 startPosition;
    public GameObject explosionAnimation;

    void Awake()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Draw in Client, Collide in Server
        //Client

        Quaternion rot = Quaternion.FromToRotation(Vector3.right, new Vector3(rigidbody2D.velocity.x, rigidbody2D.velocity.y));
        transform.rotation = rot;

        if (!Network.isServer) return;

        if ((transform.position - startPosition).sqrMagnitude > range * range)
        {
            DestroyFromNetwork();
        }
    }

    void OnTriggerEnter2D(Collider2D targetCollider)
    {
        if (!Network.isServer) return;

        if (targetCollider.gameObject.CompareTag("Tile"))
        {
            OnCollideToTile(targetCollider);
        }
        else if (targetCollider.gameObject.CompareTag("Player"))
        {
            if (friendlyFire == false)
            {
                ServerPlayer character = targetCollider.gameObject.GetComponent<ServerPlayer>();
                PlayerSetting myPlayerSetting = PlayerManager.Inst.GetSetting(owner);
                PlayerSetting targetPlayerSetting = PlayerManager.Inst.GetSetting(character.GetOwner());

                if (myPlayerSetting.team == targetPlayerSetting.team)

                    return;
            }

            OnCollideToPlayer(targetCollider);
        }
    }

    protected abstract void OnCollideToTile(Collider2D targetCollider);

    protected abstract void OnCollideToPlayer(Collider2D targetCollider);

    void OnApplicationQuit()
    {
        DestroyImmediate(gameObject);
    }

    public void DestroyFromNetwork()
    {
        if (Network.isServer)
        {
            ProjectileManager.Inst.DestroyProjectile(ID);
        }
    }
    void OnDestroy()
    {
        if (explosionAnimation != null)
        { 
            GameObject explosion = (GameObject)Instantiate(explosionAnimation, transform.position, transform.rotation);
            Destroy(explosion, 0.4f);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Server;
using C2S = Packet.C2S;

public abstract class Projectile : Weapon
{
    public long ID;
    public int owner;

    public int damage;
    public int range;

    public bool friendlyFire;
    public int power;

    protected Vector3 startPosition;
    private Vector2 direction;
    public GameObject explosionAnimation;

    bool InitFinished = false;

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (Network.isServer)
            BroadcastStatus();

        Projectile proj = GetComponent<Projectile>();

        networkView.RPC("RequestStatus", RPCMode.Server);

        projObj.rigidbody2D.AddForce(new Vector2(fire.direction.x * proj.power, fire.direction.y * proj.power), ForceMode2D.Impulse);
        proj.ID = fire.projectileID;
        ProjectileManager.Inst.Set(fire.projectileID, proj);

        Debug.Log(string.Format("Fire ID :{0} registered", fire.projectileID));
        proj.owner = fire.playerID;
    }


    virtual void BroadcastStatus();

    [RPC]
    virtual void SetStatus(byte[] pckData, NetworkMessageInfo info);


    void Awake()
    {
        startPosition = transform.position;
    }

    public void Init(C2S.Fire fire)
    {
        ID = fire.projectileID;
        owner = fire.playerID;
        direction = fire.direction;
        rigidbody2D.AddForce(direction * power, ForceMode2D.Impulse);

        InitFinished = true;
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

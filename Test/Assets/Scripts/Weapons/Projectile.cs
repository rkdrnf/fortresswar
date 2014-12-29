using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Server;
using C2S = Packet.C2S;
using S2C = Packet.S2C;
using Const;

[RequireComponent(typeof(NetworkView), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public abstract class Projectile : Weapon
{
    public long ID;
    public int owner;

    public int damage;
    public int range;

    public bool friendlyFire;

    protected Vector3 startPosition;
    private Vector2 direction;
    public GameObject explosionAnimation;

    void Awake()
    {
        networkView.group = NetworkViewGroup.PROJECTILE;
        startPosition = transform.position;

        if(Network.isClient && Client.ClientGame.Inst.IsPlayerMapLoaded())
        {
            OnPlayerMapLoaded();
        }

        OnAwake();
    }

    virtual protected void OnAwake() { }

    void OnPlayerMapLoaded()
    {
        if (Network.isClient)
            networkView.RPC("RequestCurrentStatus", RPCMode.Server);
    }

    public virtual void Init(WeaponInfo weapon, FireInfo info)
    {
        long projID = ProjectileManager.Inst.GetUniqueKeyForNewProjectile();
        ProjectileManager.Inst.Set(projID, this);
        owner = info.owner;
        direction = info.direction;
        rigidbody2D.AddForce(direction * GetPower(weapon), ForceMode2D.Impulse);

        OnInit();
    }

    protected virtual float GetPower(WeaponInfo weapon)
    {
        return weapon.power;
    }

    [RPC]
    protected virtual void RequestCurrentStatus(NetworkMessageInfo info)
    {
        S2C.ProjectileStatus pck = new S2C.ProjectileStatus(owner, transform.position, rigidbody2D.velocity);

        networkView.RPC("SetStatus", info.sender, pck.SerializeToBytes());
    }

    [RPC]
    protected virtual void SetStatus(byte[] pckData, NetworkMessageInfo info)
    {
        S2C.ProjectileStatus pck = S2C.ProjectileStatus.DeserializeFromBytes(pckData);

        owner = pck.owner;
        transform.position = pck.position;
        rigidbody2D.velocity = pck.velocity;
    }


    protected virtual void OnInit() { }

    // Update is called once per frame
    void Update()
    {
        // Draw in Client, Collide in Server
        //Client

        Rotate();

        RangeCheck();
        
    }

    protected void Rotate()
    {
        Quaternion rot = Quaternion.FromToRotation(Vector3.right, new Vector3(rigidbody2D.velocity.x, rigidbody2D.velocity.y));
        transform.rotation = rot;
    }

    protected void RangeCheck()
    {
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
            Network.RemoveRPCs(networkView.viewID);
            Network.Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        ProjectileManager.Inst.Remove(ID);
        if (explosionAnimation != null)
        { 
            GameObject explosion = (GameObject)Instantiate(explosionAnimation, transform.position, transform.rotation);
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Server;
using C2S = Packet.C2S;
using S2C = Packet.S2C;
using Const;
using Const.Effect;
using Effect;
using Data;
using Architecture;

[RequireComponent(typeof(NetworkView), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public abstract class Projectile : Weapon
{
    public long ID;
    public int owner;

    public int damage;
    public int range;
    public int impact;

    public bool friendlyFire;

    protected Vector3 startPosition;
    private Vector2 direction;
    public AnimationEffectType explosionAnimation;

    public LayerMask collisionLayer;

    void Awake()
    {
        collisionLayer = LayerMask.GetMask("Tile", "Building", "Player");
        networkView.group = NetworkViewGroup.PROJECTILE;
        startPosition = transform.position;

        if(Network.isClient && ServerGame.Inst.IsPlayerMapLoaded())
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

        RaycastHit2D hit = Physics2D.Raycast(transform.position, rigidbody2D.velocity, 0.5f, collisionLayer);

        if (targetCollider.gameObject.CompareTag("Tile"))
        {
            Tile tile = TileManager.Inst.Get(GridCoord.ToCoord(hit.point - (hit.normal * 0.5f)));
            OnCollideToTile(tile, hit.point);
        }
        else if (targetCollider.gameObject.CompareTag("Building"))
        {
            Building building = BuildingManager.Inst.Get(GridCoord.ToCoord(hit.point - (hit.normal * 0.5f)));
            OnCollideToBuilding(building, hit.point);
        }
        else if (targetCollider.gameObject.CompareTag("Player"))
        {
            ServerPlayer character = targetCollider.gameObject.GetComponent<ServerPlayer>();

            if (friendlyFire == false)
            {
                PlayerSetting myPlayerSetting = PlayerManager.Inst.GetSetting(owner);
                PlayerSetting targetPlayerSetting = PlayerManager.Inst.GetSetting(character.GetOwner());

                if (myPlayerSetting.team == targetPlayerSetting.team)

                    return;
            }
            
            OnCollideToPlayer(character, hit.point);
        }
    }

    protected abstract void OnCollideToTile(Tile tile, Vector2 point);

    protected abstract void OnCollideToBuilding(Building tile, Vector2 point);

    protected abstract void OnCollideToPlayer(ServerPlayer character, Vector2 point);

    protected void ImpactTarget(Rigidbody2D targetBody, int impact)
    {
        const int multiplier = 100;
        targetBody.AddForce(rigidbody2D.velocity.normalized * impact * multiplier, ForceMode2D.Impulse);
    }

    protected void ImpactTargetAway(Rigidbody2D targetBody, int impact)
    {
        const int multiplier = 100;
        targetBody.AddForce((targetBody.position - (Vector2)transform.position).normalized * impact * multiplier, ForceMode2D.Impulse);
    }

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

        PlayDestructionAnimation();

        OnDestroyInternal();
    }

    protected virtual void OnDestroyInternal() { }

    void PlayDestructionAnimation()
    {
        AnimationEffectManager.Inst.PlayAnimationEffect(explosionAnimation, transform.position);
        
    }
}

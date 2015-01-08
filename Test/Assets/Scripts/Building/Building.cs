using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using Server;
using Const;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class Building : MonoBehaviour
{
    public int ID;
    public GridCoord coord;
    public Const.ParticleType particleType;
    public bool destroyable;
    public int maxHealth;
    public Vector2 size;
    public spriteInfo[] sprites;

    [HideInInspector]
    public int health;
    [HideInInspector]
    public Map map;

    
    private SpriteRenderer spriteRenderer;


    public enum DestroyReason
    {
        DAMAGE,
        COLLIDE,
        MANUAL,
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSprite(health);

        rigidbody2D.isKinematic = true;
    }

    public void Init(GridCoord coord)
    {
        this.coord = coord;
 
        if (Network.isServer)
        {
            BuildingManager.Inst.Add(this);
            FillNeighbors();
            FillSuspension();
        }
    }

    public void Fall()
    {
        if (!Network.isServer) return;

        gameObject.layer = BuildingDataLoader.Inst.fallingBuildingLayer;
        rigidbody2D.isKinematic = false;

        collider2D.isTrigger = true;
    }

    void BroadcastFall()
    {
        //서버는 이미 처리 다 해서 또 보낼 필요 없음
        networkView.RPC("ClientFall", RPCMode.Others);
    }

    [RPC]
    void ClientFall()
    {
        gameObject.layer = BuildingDataLoader.Inst.fallingBuildingLayer;
        rigidbody.isKinematic = false;
        collider2D.isTrigger = true;
    }


    void OnTriggerEnter2D(Collider2D targetCollider)
    {
        if (rigidbody2D.isKinematic) return;

        if (targetCollider.tag == "Building" || targetCollider.tag == "Tile")
            DestroyBuilding(DestroyReason.COLLIDE);
        
        //damage target collider
    }



    Sprite GetSprite(int health)
    {
        int index = 0;

        while (index + 1 < sprites.Length && health < sprites[index].HealthValue)
        {
            index++;
        }

        return sprites[index].sprite;
    }

    public void SetHealth(int health)
    {
        this.health = health;

        if (health < 1)
        {
            DestroyBuilding(DestroyReason.MANUAL);
            return;
        }

        spriteRenderer.sprite = GetSprite(health);
    }

    public void Damage(int damage, Vector2 point)
    {
        if (!Network.isServer) return;

        if (destroyable)
        {
            health -= damage;
        }

        S2C.DamageTile pck = new S2C.DamageTile(this.coord, damage, point);
        BroadcastDamage(pck);

        if (health < 1)
        {
            DestroyBuilding(DestroyReason.DAMAGE);
            return;
        }
    }

    public void BroadcastDamage(S2C.DamageTile pck)
    {
        if (!Server.ServerGame.Inst.isDedicatedServer)
        {
            networkView.RPC("ClientDamage", RPCMode.All, pck.SerializeToBytes());
        }
        else
        {
            networkView.RPC("ClientDamage", RPCMode.Others, pck.SerializeToBytes());
        }
    }

    [RPC]
    void ClientDamage(byte[] pckData, NetworkMessageInfo info)
    {
        S2C.DamageTile pck = S2C.DamageTile.DeserializeFromBytes(pckData);

        DamageInternal(pck.damage, pck.point);
    }

    public void DamageInternal(int damage, Vector2 point)
    {
        if (destroyable)
        {
            PlaySplash();

            health -= damage;

            spriteRenderer.sprite = GetSprite(health);
        }
    }

    public void PlaySplash()
    {
        Client.ParticleSystem2D pSystem = Client.ParticleManager.Inst.particleSystemPool.Borrow();
        pSystem.Init(Client.ParticleManager.Inst.particleSet.particles[(int)particleType]);
        pSystem.transform.position = transform.position;
        pSystem.Play();
    }

    public void DestroyBuilding(DestroyReason reason)
    {
        switch(reason)
        {
            case DestroyReason.DAMAGE:
                PropagateDestruction();
                break;

            case DestroyReason.MANUAL:
                break;

            case DestroyReason.COLLIDE:
                break;
        }

        Network.RemoveRPCs(networkView.viewID);
        Network.Destroy(gameObject);
    }

    public void PropagateDestruction()
    {
        neighbors.DoForAll((GridDirection direction, Building building) => { building.DestroySuspension(direction); }); 
    }

    public void DestroySuspension(GridDirection direction)
    {
        suspension.DestroySuspension(direction);

        if (suspension.Count == 0)
        { 
            PropagateDestruction();
            Fall();
        }
    }

    public void AddSuspension(GridDirection direction, Building building)
    {
        suspension.Add(direction, building);
    }

    public void PropagateSuspension()
    {
        neighbors.DoForAll((GridDirection direction, Building building) =>
        {
            building.AddSuspension(direction, this);
        });
    }

    public void AddNeighbor(GridDirection direction, Building building)
    {
        neighbors.Add(direction, building);
    }

    public void PropagateNeighbor()
    {
        suspension.DoForAll((GridDirection direction, MonoBehaviour behaviour) =>
        {
            if (behaviour is Building)
                (behaviour as Building).AddNeighbor(direction, this);
        });
    }

    private Neighbors neighbors;
    private Suspension suspension;

    public void FillNeighbors()
    {
        neighbors = BuildingManager.Inst.FindNeighbors(coord);
        PropagateSuspension();
    }

    public void FillSuspension()
    {
        suspension = BuildingManager.Inst.FindSuspension(coord);
        PropagateNeighbor();
    }
}



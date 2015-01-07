using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using Server;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class Building : MonoBehaviour
{
    public int ID;
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

    

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSprite(health);

        rigidbody2D.isKinematic = true;

        FillNeighbors();
    }

    public void Fall()
    {
        if (!Network.isServer) return;

        gameObject.layer = BuildingDataLoader.Inst.fallingBuildingLayer;
        rigidbody.isKinematic = false;

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
        Network.RemoveRPCs(networkView.viewID);
        Network.Destroy(gameObject);

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
            DestroyTile();
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

        S2C.DamageTile pck = new S2C.DamageTile(this.ID, damage, point);
        BroadcastDamage(pck);

        if (health < 1)
        {
            DestroyTile();
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

    public void DestroyTile()
    {
        Network.RemoveRPCs(networkView.viewID);
        Network.Destroy(gameObject);
    }


    [HideInInspector]
    public List<GameObject> neighbors;

    private bool suspended;
    private bool isSuspensor;

    public void FillNeighbors()
    {
        neighbors.Add(Map.GetLayerObjectAt(new Vector2(transform.position.x - size.x, transform.position.y), BuildingDataLoader.Inst.buildingCollidingLayers));
        neighbors.Add(Map.GetLayerObjectAt(new Vector2(transform.position.x + size.x, transform.position.y), BuildingDataLoader.Inst.buildingCollidingLayers));
        neighbors.Add(Map.GetLayerObjectAt(new Vector2(transform.position.x, transform.position.y - transform.position.y), BuildingDataLoader.Inst.buildingCollidingLayers));
        neighbors.Add(Map.GetLayerObjectAt(new Vector2(transform.position.x, transform.position.y + transform.position.y), BuildingDataLoader.Inst.buildingCollidingLayers));
    }
}

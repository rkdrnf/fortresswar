using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using S2C = Packet.S2C;
using C2S = Packet.C2S;
using System;

[System.Serializable]
public struct spriteInfo
{
    public Sprite sprite;
    public int HealthValue;
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour {

    public int ID;
    public Const.TileType tileType;
    public Const.ParticleType particleType;
	public bool destroyable;
	public int health;
    public int maxHealth;

    public Map map;

    public spriteInfo[] sprites;
    private SpriteRenderer spriteRenderer;

    public Sprite tileBack;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSprite(health);
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
            map.networkView.RPC("ClientDamageTile", RPCMode.All, pck.SerializeToBytes());
        }
        else
        {
            map.networkView.RPC("ClientDamageTile", RPCMode.Others, pck.SerializeToBytes());
        }
    }

    public void DamageInternal(int damage, Vector2 point)
    {
        if (destroyable)
        {
            PlaySplash();

            health -= damage;

            if (health < 1)
            {
                DestroyTile();
                return;
            }

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
        spriteRenderer.sprite = tileBack;
        if (GetComponent<Collider2D>() != null)
            Destroy(GetComponent<Collider2D>());
        if (GetComponent<Rigidbody2D>())
            Destroy(GetComponent<Rigidbody2D>());
    }

    public override string ToString()
    {
        return transform.position.x.ToString() + "\t" + transform.position.y.ToString() + "\t" + ((int)tileType).ToString() + "\t" + health.ToString();
    }
}


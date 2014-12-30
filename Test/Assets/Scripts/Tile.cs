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
	public bool destroyable;
	public int health;
    public int maxHealth;

    public Map map;

    public spriteInfo[] sprites;
    private SpriteRenderer spriteRenderer;

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

    public void Damage(int damage)
    {
        if (!Network.isServer) return;

        DamageInternal(damage);
        S2C.DamageTile pck = new S2C.DamageTile(this.ID, damage);

        map.networkView.RPC("ClientDamageTile", RPCMode.Others, pck.SerializeToBytes());
    }

    public void DamageInternal(int damage)
    {
        if (destroyable)
        {
            health -= damage;
            if (health < 1)
            {
                gameObject.SetActive(false);
                //Destroy(gameObject);
            }


            try
            { 

            spriteRenderer.sprite = GetSprite(health);
            }
            catch(Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public override string ToString()
    {
        return transform.position.x.ToString() + "\t" + transform.position.y.ToString() + "\t" + ((int)tileType).ToString() + "\t" + health.ToString();
    }
}

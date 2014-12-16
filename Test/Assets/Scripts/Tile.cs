using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using S2C = Packet.S2C;
using C2S = Packet.C2S;

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

    public Map map;

    public spriteInfo[] sprites;
    private int curSpriteIdx;

    private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
        curSpriteIdx = 0;
        while (curSpriteIdx + 1 < sprites.Length && health - sprites[curSpriteIdx + 1].HealthValue < 0)
        {
            curSpriteIdx++;
        }
        spriteRenderer.sprite = sprites[curSpriteIdx].sprite;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Damage(int damage)
    {
        if (!Network.isServer) return;

        DamageInternal(damage);
        S2C.DamageTile pck = new S2C.DamageTile(this.ID, damage);

        map.networkView.RPC("ClientDamageTile", RPCMode.OthersBuffered, pck.SerializeToBytes());
    }

    public void DamageInternal(int damage)
    {
        if (destroyable)
        {
            health -= damage;
            if (health < 1)
                Destroy(gameObject);
            else if (health < sprites[curSpriteIdx].HealthValue)
            {
                curSpriteIdx++;
                spriteRenderer.sprite = sprites[curSpriteIdx].sprite;
            }
        }
    }

    public override string ToString()
    {
        return transform.position.x.ToString() + "\t" + transform.position.y.ToString() + "\t" + ((int)tileType).ToString() + "\t" + health.ToString();
    }
}

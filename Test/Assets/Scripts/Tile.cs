using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct spriteInfo
{
    public Sprite sprite;
    public int HealthValue;
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour {

    public Const.eTileType tileType;
	public bool destroyable;
	public int health;

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
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Particle2D : MonoBehaviour
{

    float lifeTime;

    Rigidbody2D pBody;
    BoxCollider2D pColl;
    SpriteRenderer pRenderer;

    enum OutDirection
    {
        LEFT, 
        RIGHT, 
        UP, 
        DOWN
    }

    public void Awake()
    {
        pBody = GetComponent<Rigidbody2D>();
        pColl = GetComponent<BoxCollider2D>();
        pRenderer = GetComponent<SpriteRenderer>();

        gameObject.layer = LayerMask.NameToLayer("Particle");
    }

    public void Init(Particle2DData setting)
    {
        this.lifeTime = setting.lifeTime;

        transform.position = setting.position;

        pBody.velocity = setting.velocity;
        pBody.gravityScale = setting.gravityScale;
        pBody.fixedAngle = true;

        if (setting.collide)
        {
            pColl.enabled = true;
            pColl.size = setting.sprite.bounds.size;
            pColl.sharedMaterial = setting.collidingMaterial;
        }
        else
        {
            pColl.enabled = false;
        }

        pRenderer.sprite = setting.sprite;
        pRenderer.material = setting.material;
        pRenderer.color = setting.color;

        transform.localScale = setting.scale;

        Invoke("Disable", lifeTime);
    }

    void Disable()
    {
        gameObject.SetActive(false);
    }

    
}

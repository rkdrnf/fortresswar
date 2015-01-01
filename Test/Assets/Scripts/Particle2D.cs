using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Particle2D : MonoBehaviour
{
    float lifeTime;
    float inclination;
    Bounds bounds;

    Rigidbody2D pBody;
    BoxCollider2D pColl;
    SpriteRenderer pRenderer;
    
    public void Awake()
    {
        pBody = GetComponent<Rigidbody2D>();
        pColl = GetComponent<BoxCollider2D>();
        pRenderer = GetComponent<SpriteRenderer>();

        gameObject.layer = LayerMask.NameToLayer("Particle");
    }

    public void Init(float minVelocity, float maxVelocity, Vector2 gravityModifier, float gravityScale,
        bool collide, Sprite sprite, PhysicsMaterial2D collidingMaterial, Material material, Color[] colors,
        float size, Bounds bounds, float lifeTime, Vector3 position)
    {
        this.lifeTime = lifeTime;
        this.bounds = bounds;
        inclination = bounds.size.y / bounds.size.x;

        float randomX = Random.value - 0.5f;
        float randomY = Random.value - 0.5f;
        Vector3 randomDirection = new Vector3(randomX, randomY, 0);

        pBody.velocity = (randomDirection * Random.Range(minVelocity, maxVelocity)) + (Vector3)gravityModifier;
        pBody.gravityScale = gravityScale;
        pBody.fixedAngle = true;

        if (collide)
        {
            pColl.enabled = true;
            pColl.size = sprite.bounds.size;
            pColl.sharedMaterial = collidingMaterial;
        }
        else
        {
            pColl.enabled = false;
        }

        pRenderer.sprite = sprite;
        pRenderer.material = material;
        pRenderer.color = colors[(int)(Random.value * colors.Length)];

        transform.localScale = Vector3.one * (size / sprite.bounds.size.x);

        //particleObj.transform.parent = this.transform;

        transform.position = position + (Vector3)FindBorder(randomX, randomY);

        Invoke("Disable", lifeTime);
    }

    void Disable()
    {
        gameObject.SetActive(false);
    }

    Vector2 FindBorder(float x, float y)
    {
        float particleInc = y / x;

        if (x >= 0 && y >= 0) //1사분면
        {
            if (inclination > particleInc)
            {
                return new Vector2(bounds.size.x / 2, y * bounds.size.y);
            }
            else
            {
                return new Vector2(x * bounds.size.x, bounds.size.y / 2);
            }
        }

        if (x < 0 && y >= 0) //2사분면
        {
            if (-inclination < particleInc)
            {
                return new Vector2(-bounds.size.x / 2, y * bounds.size.y);
            }
            else
            {
                return new Vector2(x * bounds.size.x, bounds.size.y / 2);
            }
        }

        if (x < 0 && y < 0) //3사분면
        {
            if (inclination > particleInc)
            {
                return new Vector2(-bounds.size.x / 2, y * bounds.size.y);
            }
            else
            {
                return new Vector2(x * bounds.size.x, -bounds.size.y / 2);
            }
        }

        if (x >= 0 && y < 0)
        {
            if (-inclination < particleInc)
            {
                return new Vector2(bounds.size.x / 2, y * bounds.size.y);
            }
            else
            {
                return new Vector2(x * bounds.size.x, bounds.size.y / 2);
            }
        }

        return Vector2.zero;
    }
}

using UnityEngine;
using System.Collections;

public class Missile : Projectile {

    public int splashRange;

    public int sqrSplashRange;

    public int distDamping;

    void Awake()
    {
        sqrSplashRange = splashRange * splashRange;
    }

    protected override void CollisionFunc(Collider2D targetCollider)
    {
        if (Network.isServer)
        {
            if (targetCollider.gameObject.CompareTag("Tile"))
            {
                OnCollideToTile(targetCollider);
            }
            else if (targetCollider.gameObject.CompareTag("Player"))
            {
                OnCollideToPlayer(targetCollider);
            }
            
        }
    }

    void OnCollideToTile(Collider2D targetCollider)
    {
        Tile tile = targetCollider.gameObject.GetComponent<Tile>();
        if (tile)
        {
            DamageAround(new Vector2(transform.position.x, transform.position.y));
            DestroyFromNetwork();
        }
    }

    void OnCollideToPlayer(Collider2D targetCollider)
    {
        //When Hit My Player
        PlayerBehaviour character = targetCollider.gameObject.GetComponent<PlayerBehaviour>();
        if (character)
        {
            if (owner == character.GetOwner())
                return;

            if (character.IsDead())
                return;

            DamageAround(new Vector2(transform.position.x, transform.position.y));
            DestroyFromNetwork();
        }
    }
    void DamageAround(Vector2 origin)
    {
        Collider2D[] colliders =
            Physics2D.OverlapCircleAll(origin, (float)splashRange);

        for (int i = 0; i < colliders.Length; ++i)
        {
            GameObject collidingObject = colliders[i].gameObject;
            if (collidingObject.CompareTag("Tile"))
            {
                collidingObject.GetComponent<Tile>().Damage(DamageByDistance(collidingObject.transform.position));
            }
            else if (collidingObject.CompareTag("Player"))
            {
                collidingObject.GetComponent<PlayerBehaviour>().Damage(DamageByDistance(collidingObject.transform.position), new NetworkMessageInfo());
            }
        }
    }

    int DamageByDistance(Vector3 targetPoint)
    {
        if (splashRange <= 0)
        {
            return damage;
        }

        Vector2 dist2D = transform.position - targetPoint;
        int finalDamage = damage - (int)((damage * (dist2D.sqrMagnitude / sqrSplashRange)) * distDamping);

        if (finalDamage < 0)
            return 0;

        return finalDamage;
    }

    
}

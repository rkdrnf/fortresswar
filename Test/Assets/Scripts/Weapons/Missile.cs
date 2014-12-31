using UnityEngine;
using System.Collections;
using Server;

public class Missile : Projectile {

    public int splashRange;
    public int sqrSplashRange;
    public int distDamping;
    

    protected override void OnAwake()
    {
        sqrSplashRange = splashRange * splashRange;
    }

    protected override void OnCollideToTile(Tile tile, Vector2 point)
    {
        if (tile)
        {
            DamageAround(new Vector2(transform.position.x, transform.position.y));
            DestroyFromNetwork();
        }
    }

    protected override void OnCollideToPlayer(ServerPlayer character, Vector2 point)
    {
        //When Hit My Player
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

            LayerMask tileLayer = LayerMask.GetMask("Tile");
            if (collidingObject.CompareTag("Tile"))
            {
                RaycastHit2D hit = Physics2D.Linecast(collidingObject.transform.position, origin, tileLayer);
                collidingObject.GetComponent<Tile>().Damage(DamageByDistance(collidingObject.transform.position), hit.point);
            }
            else if (collidingObject.CompareTag("Player"))
            {
                collidingObject.GetComponent<ServerPlayer>().Damage(DamageByDistance(collidingObject.transform.position), new NetworkMessageInfo());
                ImpactTargetAway(collidingObject.rigidbody2D, ImpactByDistance(collidingObject.transform.position));
            }
        }
    }

    int ImpactByDistance(Vector3 targetPoint)
    {
        if (splashRange <= 0)
        {
            return impact;
        }

        Vector2 dist2D = transform.position - targetPoint;
        int finalImpact = impact - (int)((impact * (dist2D.sqrMagnitude / sqrSplashRange)) * distDamping);

        if (impact < 0)
            return 0;

        return impact;
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

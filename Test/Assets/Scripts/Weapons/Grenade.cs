using UnityEngine;
using System.Collections;
using Server;

public class Grenade : Projectile {

    protected override float GetPower(WeaponInfo weapon)
    {
        float multiplier = (float)(weapon.chargeTimer / weapon.maxChargeTime);
        if (multiplier > 1 ) multiplier = 1f;
        return weapon.power * multiplier;
    }

    public int splashRange;
    public int sqrSplashRange;
    public int distDamping;

    protected override void OnAwake()
    {
        sqrSplashRange = splashRange * splashRange;
    }

    protected override void OnCollideToTile(Collider2D targetCollider)
    {
        Tile tile = targetCollider.gameObject.GetComponent<Tile>();
        if (tile)
        {
            DamageAround(new Vector2(transform.position.x, transform.position.y));
            DestroyFromNetwork();
        }
    }

    protected override void OnCollideToPlayer(Collider2D targetCollider)
    {
        //When Hit My Player
        ServerPlayer character = targetCollider.gameObject.GetComponent<ServerPlayer>();
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

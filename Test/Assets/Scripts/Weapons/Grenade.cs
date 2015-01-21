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

    protected override void OnAwake()
    {
        sqrSplashRange = splashRange * splashRange;
    }

    protected override void OnCollideToTile(Tile tile, Vector2 point)
    {
        if (tile != null)
        {
            DamageAround(new Vector2(transform.position.x, transform.position.y));
            DestroyFromNetwork();
        }
    }

    protected override void OnCollideToBuilding(Building building, Vector2 point)
    {
        if (building != null)
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
}

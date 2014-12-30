using UnityEngine;
using System.Collections;

using Server;
using ProtoBuf;

public class GunBullet : Projectile {

    protected override void OnCollideToTile(Collider2D targetCollider)
    {
        Tile tile = targetCollider.gameObject.GetComponent<Tile>();
        if (tile)
        {
            tile.Damage(damage);
            ImpactTarget(targetCollider.rigidbody2D, impact);
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
            
            character.Damage(damage, new NetworkMessageInfo());
            ImpactTarget(targetCollider.rigidbody2D, impact);

            DestroyFromNetwork();
        }
    }
}


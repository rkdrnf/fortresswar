using UnityEngine;
using System.Collections;

using Server;
using ProtoBuf;

public class GunBullet : Projectile {

    protected override void OnCollideToTile(Tile tile, Vector2 point)
    {
        if (tile != null)
        {
            tile.Damage(damage, point);
            DestroyFromNetwork();
        }
    }

    protected override void OnCollideToBuilding(Building building, Vector2 point)
    {
        if (building != null)
        {
            building.Damage(damage, point);
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
            
            character.Damage(damage, new NetworkMessageInfo());
            ImpactTarget(character.GetComponent<Rigidbody2D>(), impact);

            DestroyFromNetwork();
        }
    }
}


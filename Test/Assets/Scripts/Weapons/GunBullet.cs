using UnityEngine;
using System.Collections;

using Server;
using ProtoBuf;

public class GunBullet : Projectile {

    protected override void OnCollideToTile(Tile tile, Vector2 point)
    {
        if (tile)
        {
            tile.Damage(damage, point);
            //ImpactTarget(tile.rigidbody2D, impact);
            DestroyFromNetwork();
        }
    }

    protected override void OnCollideToBuilding(Building building, Vector2 point)
    {
        if (building)
        {
            building.Damage(damage, point);
            //ImpactTarget(building.rigidbody2D, impact);
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
            ImpactTarget(character.rigidbody2D, impact);

            DestroyFromNetwork();
        }
    }
}


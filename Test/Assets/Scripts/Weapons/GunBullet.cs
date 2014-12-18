﻿using UnityEngine;
using System.Collections;

public class GunBullet : Projectile {

	
    protected override void OnCollideToTile(Collider2D targetCollider)
    {
        Tile tile = targetCollider.gameObject.GetComponent<Tile>();
        if (tile)
        {
            tile.Damage(damage);
            DestroyFromNetwork();
        }
    }

    protected override void OnCollideToPlayer(Collider2D targetCollider)
    {
        //When Hit My Player
        PlayerBehaviour character = targetCollider.gameObject.GetComponent<PlayerBehaviour>();
        if (character)
        {
            if (owner == character.GetOwner())
                return;

            if (character.IsDead())
                return;
            
            character.Damage(damage, new NetworkMessageInfo());

            DestroyFromNetwork();
        }
    }
}

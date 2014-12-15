using UnityEngine;
using System.Collections;

public class GunBullet : Projectile {

	protected override void CollisionFunc(Collider2D targetCollider)

	{
        Debug.Log("collided");
		if (Network.isServer) {
            if (targetCollider.gameObject.CompareTag("Tile"))
            {
                OnCollideToTile(targetCollider);
            }
            else if (targetCollider.gameObject.CompareTag("Player"))
            {
                OnCollideToPlayer(targetCollider);
			}
		}
        return;
	}

    void OnCollideToTile(Collider2D targetCollider)
    {
        Tile tile = targetCollider.gameObject.GetComponent<Tile>();
        if (tile)
        {
            tile.Damage(damage);
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
            
            character.Damage(damage, new NetworkMessageInfo());

            DestroyFromNetwork();
        }
    }
}

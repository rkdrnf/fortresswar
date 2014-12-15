using UnityEngine;
using System.Collections;

public class Missile : Projectile {

    public int splashRange;

    protected override void CollisionFunc(Collider2D targetCollider)
    {
        if (Network.isServer)
        {
            GameObject obj = targetCollider.gameObject;
            if (obj.GetComponent<Tile>() || obj.GetComponent<PlayerBehaviour>())
            {
                Collider2D[] colliders =
                    Physics2D.OverlapCircleAll(new Vector2(obj.transform.position.x, obj.transform.position.y), (float)splashRange);

                for (int i = 0; i < colliders.Length; ++i)
                {
                    if (colliders[i].gameObject.CompareTag("Tile"))
                    {
                        OnCollideToTile(colliders[i]);
                    }
                    else if (colliders[i].gameObject.CompareTag("Player"))
                    {
                        OnCollideToPlayer(colliders[i]);
                    }
                }
            }
        }
    }

    void OnCollideToTile(Collider2D targetCollider)
    {
        Tile tile = targetCollider.gameObject.GetComponent<Tile>();
        if (tile)
        {
            tile.GetComponent<Tile>().Damage(damage);
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
            character.Damage(damage, new NetworkMessageInfo());
        }
    }
}

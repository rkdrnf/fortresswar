using UnityEngine;
using System.Collections;

public class GunBullet : Projectile {



	const int DAMAGE = 20;

	const int RANGE = 30;

	Vector3 startPosition;
	Vector3 currentPosition;

    public GameObject explosionAnimation;

    bool isQuitting = false;

	// Use this for initialization
	void Awake () {
        
		startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		// Draw in Client, Collide in Server
		//Client

		currentPosition = transform.position;

		Quaternion rot = Quaternion.FromToRotation (Vector3.right, new Vector3 (rigidbody2D.velocity.x, rigidbody2D.velocity.y));
		transform.rotation = rot;

        if ((currentPosition - startPosition).sqrMagnitude > RANGE * RANGE)
        {
            Destroy();
        }
	}

	void OnTriggerEnter2D(Collider2D targetCollider)
	{
        Debug.Log("collided");
		if (Network.isServer) {
			if (targetCollider.gameObject.CompareTag ("Tile")) {
				OnCollideToTile(targetCollider);
			} else if (targetCollider.gameObject.CompareTag("Player")){
                OnCollideToPlayer(targetCollider);
			}
		}
        return;

	}

    void OnCollideToTile(Collider2D targetCollider)
    {
        GameObject tile = targetCollider.gameObject;
        tile.GetComponent<Tile>().Damage(DAMAGE);

        Destroy();

        
    }

    void OnCollideToPlayer(Collider2D targetCollider)
    {
        //When Hit My Player
        PlayerBehaviour character = targetCollider.gameObject.GetComponent<PlayerBehaviour>();
        if (owner == character.GetOwner()) 
            return;



        character.Damage(DAMAGE, new NetworkMessageInfo());

        Destroy();
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        if (isQuitting)
            return;

        GameObject explosion = (GameObject)Instantiate(explosionAnimation, transform.position, transform.rotation);
        Destroy(explosion, 0.4f);
    }
}

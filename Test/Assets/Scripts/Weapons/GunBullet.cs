using UnityEngine;
using System.Collections;

public class GunBullet : Projectile {



	const int DAMAGE = 20;

	const int RANGE = 10;

	Vector3 startPosition;
	Vector3 currentPosition;

    public bool isOwner;

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
		if (Network.isServer) {
			if (targetCollider.gameObject.CompareTag ("Tile")) {
				GameObject tile = targetCollider.gameObject;
				tile.GetComponent<Tile> ().Damage (DAMAGE);

                Destroy();
			} else {
				return;
			}
		}
	}
}

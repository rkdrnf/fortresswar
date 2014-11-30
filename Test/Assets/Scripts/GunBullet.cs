using UnityEngine;
using System.Collections;

public class GunBullet : MonoBehaviour {

	const int DAMAGE = 20;

	const int RANGE = 10;

	Vector3 startPosition;
	Vector3 currentPosition;

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

		//Server
		if (Network.isServer) 
		{
			if ((currentPosition - startPosition).sqrMagnitude > RANGE * RANGE) {
				Destroy();
			}
		}
	
	}

	public void Fire(Vector3 direction, int power)
	{
		networkView.RPC ("BroadcastFire", RPCMode.AllBuffered, transform.position, direction, power);
	}

	[RPC]
	public void BroadcastFire(Vector3 pos, Vector3 direction, int power)
	{
		transform.position = pos;

		direction.Normalize ();
		
		float powerRateX = direction.x;
		float powerRateY = direction.y;

		rigidbody2D.AddForce(new Vector2(powerRateX * power, powerRateY * power), ForceMode2D.Impulse);
	}

	void OnTriggerEnter2D(Collider2D targetCollider)
	{
		if (Network.isServer) {
			if (targetCollider.gameObject.CompareTag ("Tile")) {
				GameObject tile = targetCollider.gameObject;
				tile.GetComponent<StoneBehaviour> ().Damage (DAMAGE);

				Destroy();
			} else {
				return;
			}
		}
	}

	void Destroy()
	{
		if (Network.isServer) {
			Network.RemoveRPCs(networkView.viewID);
			Network.Destroy (gameObject);
		}
	}


}

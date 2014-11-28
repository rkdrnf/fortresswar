using UnityEngine;
using System.Collections;

public class PlayerBehaviour : MonoBehaviour {

	public GameObject projectile;
	public GameObject netManager;

	const int MOVE_SPEED = 4;

	const float FIRE_RATE = 0.2f;
	const int FIRE_POWER = 20;

	bool facingRight = true;

	bool isOwner = false;

	float timer;

	float moving;

	NetworkView transformView;
	NetworkView controllerView;


	[RPC]
	public void SetOwner()
	{
		NetworkViewID controllerID = Network.AllocateViewID ();

		controllerView.RPC ("SetControllerNetworkView", RPCMode.Server, controllerID);

		controllerView.viewID = controllerID;

		isOwner = true;

		Debug.Log (controllerID);
	}

	[RPC]
	public void SetControllerNetworkView(NetworkViewID viewID)
	{
		controllerView.viewID = viewID;

		Debug.Log (viewID);
	}

	void Awake()
	{
		timer = 0f;
		netManager = GameObject.Find ("NetworkManager");

		Component[] views = gameObject.GetComponents(typeof(NetworkView));
		foreach (Component view in views) {
			NetworkView nView = (NetworkView)view;
			if (nView.observed is PlayerBehaviour)
			{
				controllerView = nView;
			}

			if (nView.observed is Transform)
			{
				transformView = nView;
			}
		}
	}

	// Use this for initialization
	void Start () {

	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo mnInfo)
	{
		if (stream.isWriting) {
			Debug.Log ("writing");
			if (isOwner) {
				float moveVal = moving;
				stream.Serialize (ref moveVal);
			}
		} else {
			Debug.Log ("reading");
			float moveVal = 0;
			stream.Serialize(ref moveVal);
			moving = moveVal;
		}
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (isOwner) {
						float move = Input.GetAxis ("Horizontal") * MOVE_SPEED;


						moving = move;



						if (Input.GetButton ("Fire1")) {
			
								timer += Time.deltaTime;
								if (timer > FIRE_RATE) {
										timer = 0f;
										GameObject bullet = Instantiate (projectile, transform.position, transform.rotation) as GameObject;
				
										Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, transform.position.z));
										Vector3 direction = worldMousePosition - transform.position;
				
										bullet.GetComponent<GunBullet> ().Fire (direction, 20);
								}
						}
				}
		// Server Rendering.
		if (Network.isServer) {
			if (moving != 0) {
				rigidbody2D.velocity = new Vector2 (moving, rigidbody2D.velocity.y);
			}

			if (moving < 0 && facingRight) {
				Flip();
				
			}
			if (moving > 0 && !facingRight) {
				Flip();
			}
		}


	}

	void Flip() {
		facingRight = !facingRight;
		Vector3 scale = transform.localScale;
		scale.x = -scale.x;
		transform.localScale = scale;
	}
}

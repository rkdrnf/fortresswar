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

	float fireTimer;

	float horMov;
	float verMov;
	bool jumpMov;

	bool grounded = false;
	public Transform groundCheck;
	float groundRadius = 0.1f;
	public LayerMask whatIsGround;

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
		fireTimer = 0f;
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
				float horMoveVal = horMov;
				float verMoveVal = verMov;
				bool jumpVal = jumpMov;
				stream.Serialize (ref horMoveVal);
				stream.Serialize (ref verMoveVal);
				stream.Serialize (ref jumpVal);
			}
		} else {
			Debug.Log ("reading");
			float horMoveVal = 0;
			float verMoveVal = 0;
			bool jumpVal = false;
			stream.Serialize(ref horMoveVal);
			stream.Serialize(ref verMoveVal);
			stream.Serialize (ref jumpVal);
			horMov = horMoveVal;
			verMov = verMoveVal;
			jumpMov = jumpVal;
		}
	}

	// Update is called once per frame
	// Reset input data per frame.
	void Update () {
		if (isOwner) {
			fireTimer += Time.deltaTime;
			horMov = Input.GetAxis ("Horizontal");
			verMov = Input.GetAxis("Vertical");
			jumpMov = Input.GetButtonDown("Vertical");
			if (jumpMov)
			{
				Debug.Log("jump");
			}
			if (Input.GetButton ("Fire1")) {
				Debug.Log("fire button pressed");

				//Client manages Bullet Spawn. 
				if (fireTimer > FIRE_RATE) {
					fireTimer = 0;
					Debug.Log ("Fire!");
					Fire();
				}
			}
		}


		// Server Rendering. Rendering from input must be in Update()
		// Fixed Update refreshes in fixed period. 
		// When Update() Period is shorter than fixed period, Update() can be called multiple times between FixedUpdate().
		// Because Input Data is reset in Update(), FixedUpdate() Can't get input data properly.
		if (Network.isServer) {
			
			grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
			
			if (horMov != 0) {
				rigidbody2D.velocity = new Vector2 (horMov * MOVE_SPEED, rigidbody2D.velocity.y);
			}
			
			if (jumpMov) 
			{
				if (grounded)
				{
					Debug.Log("jump!!!");
					rigidbody2D.velocity = new Vector2(0, 12);
				}
				else
				{
					Debug.Log("not in ground");
				}
			}
			
			
			if (horMov < 0 && facingRight) {
				Flip();
				
			}
			if (horMov > 0 && !facingRight) {
				Flip();
			}
		}
	}


	void Fire()
	{
			GameObject bullet = Network.Instantiate (projectile, transform.position, transform.rotation, 1) as GameObject;
			Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, transform.position.z));
			Vector3 direction = worldMousePosition - transform.position;
			
			bullet.GetComponent<GunBullet> ().Fire (direction, 20);
	}

	void Flip() {
		facingRight = !facingRight;
		Vector3 scale = transform.localScale;
		scale.x = -scale.x;
		transform.localScale = scale;
	}
}

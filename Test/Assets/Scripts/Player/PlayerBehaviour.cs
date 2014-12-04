using UnityEngine;
using System.Collections;
using Const;

public class PlayerBehaviour : MonoBehaviour {

	public GameObject projectile;

	const int MOVE_SPEED = 6;
	const float WALL_WALK_SPEED = 5;

	const float WALL_JUMP_SPEED_X = 8;
	const float WALL_JUMP_SPEED_Y = 4;

	const float FIRE_RATE = 0.2f;
	const int FIRE_POWER = 20;

	bool facingRight = true;

	bool isOwner = false;

	float fireTimer;

	float horMov;
	float verMov;
	//bool jumpMov;

	float wallWalkTimer;

	const float WALL_WALK_TIME = 0.7f;


	NetworkView transformView;
	NetworkView controllerView;

	int stateFlag;



	public bool IsInState(CharacterState state, params CharacterState[] stateList)
	{
		bool result = (stateFlag & (1 << (int)state)) != 0;
	

		foreach(CharacterState stateVal in stateList)
		{
			if(result == false)
				break;

			result = result && ((stateFlag & (1 << (int)state)) != 0);
		}

		return result;
	}

	public void SetState(CharacterState state, bool value)
	{
		if (value) {
			stateFlag = stateFlag | (1 << (int)state);
		} 
		else {
			stateFlag = stateFlag & (~(1 << (int)state));
		}
	}

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
		wallWalkTimer = 0f;

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
				//bool jumpVal = jumpMov;
				stream.Serialize (ref horMoveVal);
				stream.Serialize (ref verMoveVal);
				//stream.Serialize (ref jumpVal);
			}
		} else {
			Debug.Log ("reading");
			float horMoveVal = 0;
			float verMoveVal = 0;
			//bool jumpVal = false;
			stream.Serialize(ref horMoveVal);
			stream.Serialize(ref verMoveVal);
			//stream.Serialize (ref jumpVal);
			horMov = horMoveVal;
			verMov = verMoveVal;
			//jumpMov = jumpVal;
		}
	}

	// Update is called once per frame
	// Reset input data per frame.
	void Update () {
		if (isOwner) {
			fireTimer += Time.deltaTime;
			horMov = Input.GetAxis ("Horizontal");
			verMov = Input.GetAxis("Vertical");

			/*
			jumpMov = Input.GetButtonDown("Vertical");

			if (jumpMov)
			{
				Debug.Log("jump");
			}
			*/

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

		//Animation Rendering.
		//Every client must do his own Animation Rendering.
		Animator anim = GetComponent<Animator>();
		anim.SetBool("HorMoving", horMov != 0);

		// Server Rendering. Rendering from input must be in Update()
		// Fixed Update refreshes in fixed period. 
		// When Update() Period is shorter than fixed period, Update() can be called multiple times between FixedUpdate().
		// Because Input Data is reset in Update(), FixedUpdate() Can't get input data properly.
		if (Network.isServer) {

			if (horMov != 0) {
				rigidbody2D.velocity = new Vector2 (horMov * MOVE_SPEED, rigidbody2D.velocity.y);
			}

			do
			{
				if (IsInState(CharacterState.GROUNDED))
				{
					//Set Other States
					EndWallWalk();
					SetState(CharacterState.WALL_WALKED_LEFT, false);
					SetState(CharacterState.WALL_WALKED_RIGHT, false);

					//
					if (verMov > 0)
					{
						Debug.Log("jump!!!");
						rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 12);
					}
					break;
				}

				if (IsInState(CharacterState.WALLED_FRONT) || IsInState(CharacterState.WALLED_BACK))
			    {
					if (verMov > 0)
					{
						if (IsMoving(Direction.RIGHT) && IsWalled(Direction.RIGHT))
						{
							WallWalk(Direction.RIGHT);
							break;
						}

						if (IsMoving(Direction.LEFT) && IsWalled(Direction.LEFT))
						{
							WallWalk(Direction.LEFT);
						    break;
						}

						if (IsMoving(Direction.RIGHT) && IsWalled(Direction.LEFT))
						{
							WallJump(Direction.RIGHT);
							break;
						}

						if (IsMoving(Direction.LEFT) && IsWalled(Direction.RIGHT))
						{
							WallJump(Direction.LEFT);
							break;
						}
					}

					break;
				}
				else
				{
					EndWallWalk();
				}
			}
			while(false);

			
			if (horMov < 0 && facingRight) {
				Flip();
			}
			if (horMov > 0 && !facingRight) {
				Flip();
			}
		}
	}

	bool IsMoving(Direction direction)
	{
		return direction == Direction.RIGHT ? horMov > 0 : horMov < 0;
	}

	bool IsWalled(Direction direction)
	{
		if (direction == Direction.LEFT) {
			return facingRight ? IsInState (CharacterState.WALLED_BACK) : IsInState(CharacterState.WALLED_FRONT);
		} else {
			return facingRight ? IsInState (CharacterState.WALLED_FRONT) : IsInState(CharacterState.WALLED_BACK);
		}
	}

	void WallWalk(Direction direction)
	{
		//Already wall walked same wall
		if ((!IsInState(CharacterState.WALL_WALKING)) && WallWalked (direction))
			return;

		SetState (CharacterState.WALL_WALKING, true);

		SetState (GetWallWalkStateByDirection (direction), true);
		SetState (direction == Direction.RIGHT ? CharacterState.WALL_WALKED_LEFT : CharacterState.WALL_WALKED_RIGHT, false);
		wallWalkTimer += Time.deltaTime;

		if (wallWalkTimer > WALL_WALK_TIME) {
			EndWallWalk();
			return;
		}

		rigidbody2D.velocity = new Vector2 (rigidbody2D.velocity.x, WALL_WALK_SPEED);
	}

	void EndWallWalk()
	{
		wallWalkTimer = 0f;
		SetState(CharacterState.WALL_WALKING, false);
	}

	CharacterState GetWallWalkStateByDirection(Direction direction)
	{
		return direction == Direction.RIGHT ? CharacterState.WALL_WALKED_RIGHT : CharacterState.WALL_WALKED_LEFT;
	}

	bool WallWalked(Direction direction)
	{
		return IsInState (GetWallWalkStateByDirection(direction));
	}

	void WallJump(Direction direction)
	{
		Debug.Log ("Wall Jump!!");
		rigidbody2D.velocity = new Vector2 (direction == Direction.RIGHT ? WALL_JUMP_SPEED_X : -WALL_JUMP_SPEED_X, WALL_JUMP_SPEED_Y);

		EndWallWalk();
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

	void OnFootCollide(Collision2D coll){
	}
}

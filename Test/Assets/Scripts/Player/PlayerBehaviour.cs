﻿using UnityEngine;
using System.Collections;
using Const;

public class PlayerBehaviour : MonoBehaviour {

	public GameObject projectile;

	const int MOVE_SPEED = 8;
	const float WALL_WALK_SPEED = 8;

	const float JUMP_SPEED = 12;
	const float WALL_JUMP_SPEED_X = 15;
	const float WALL_JUMP_SPEED_Y = 10;

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

    public CharacterState state;

    int envFlag;

    public bool IsInState(CharacterState state, params CharacterState[] stateList)
    {
        bool result = this.state == state;

        foreach (CharacterState stateVal in stateList)
        {
            if (result == true)
                break;

            result = this.state == stateVal;
        }

        return result;
    }


    public bool IsInEnv(CharacterEnv env, params CharacterEnv[] envList)
	{
        bool result = (envFlag & (1 << (int)env)) != 0;


        foreach (CharacterEnv envVal in envList)
		{
			if(result == false)
				break;

			result = result && ((envFlag & (1 << (int)env)) != 0);
		}

		return result;
	}

	public void SetEnv(CharacterEnv env, bool value)
	{
		if (value) {
            envFlag = envFlag | (1 << (int)env);
		} 
		else {
            envFlag = envFlag & (~(1 << (int)env));
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

        CameraBehaviour camera = GameObject.Find("Main Camera").GetComponent<CameraBehaviour>();
        camera.target = transform;
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
			horMov = Input.GetAxisRaw ("Horizontal");
			verMov = Input.GetAxisRaw("Vertical");

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
			do
			{
                if (state == CharacterState.GROUNDED)
                {
                    WhenGrounded();
					break;
				}

                if (state == CharacterState.FALLING)
                {
                    WhenFalling();
                    break;
                }

                if (state == CharacterState.JUMPING_UP)
                {
                    WhenJumping();
                    break;
                }

                if (state == CharacterState.WALL_JUMPING)
                {
                    WhenWallJumping();
                    break;
                }

                if (state == CharacterState.WALL_WALKING)
                {
                    WhenWallWalking();
                    break;
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

    void WhenGrounded()
    {
        //Set Other States
        EndWallWalk();
        SetEnv(CharacterEnv.WALL_WALKED_LEFT, false);
        SetEnv(CharacterEnv.WALL_WALKED_RIGHT, false);

        if (horMov != 0)
        {
            rigidbody2D.velocity = new Vector2(horMov * MOVE_SPEED, rigidbody2D.velocity.y);
        }

        //
        if (verMov > 0)
        {
            Debug.Log("jump!!!");
			Jump();
            
        }
    }

    void WhenFalling()
    {
        // Fall to ground;
        if (horMov == 0)
        {
            return;
        }

        if (horMov != 0)
        {
            //WallJump Or Climb more.
            if (verMov > 0)
            {
                //Debug.Log("CheckWallJump");
                CheckWallJump();

                return;
            }

            //Fall to ground.
            if (verMov <= 0f)
            {
                rigidbody2D.velocity = new Vector2(horMov * MOVE_SPEED, rigidbody2D.velocity.y);
                
                return;
            }
        }
    }

    void WhenJumping()
    {
        if (horMov != 0)
        {
            rigidbody2D.velocity = new Vector2(horMov * MOVE_SPEED, rigidbody2D.velocity.y);
        }

        if (rigidbody2D.velocity.y <= 0f)
        {
            state = CharacterState.FALLING;
        }
    }

    void WhenWallJumping()
    {
        if (horMov != 0)
        {
            rigidbody2D.AddForce(new Vector2(horMov * MOVE_SPEED, 2f));

            //WallJump Or Climb more.
            if (verMov > 0)
            {
                //Debug.Log("CheckWallJump");
                CheckWallJump();

                return;
            }

            //Fall to ground.
            if (verMov <= 0f)
            {
                rigidbody2D.velocity = new Vector2(horMov * MOVE_SPEED, rigidbody2D.velocity.y);

                return;
            }
        }
    }

    void WhenWallWalking()
    {
        //Fall to ground.
        if (horMov == 0)
        {
            state = CharacterState.FALLING;
            return;
        }

        if (horMov != 0)
        {
            //WallJump Or Climb more.
            if (verMov > 0)
            {
				//월 워킹 중에 위 방향키를 누르고 있는데도 월 관련 함수가 실패하면 벽이 사라진거. 가던 방향으로 점프함.
                if (CheckWallJump())
				{
                	return;
				}
				else
				{
					Jump();
				}
            }

            //Fall to ground.
            if (verMov <= 0f)
            {
                rigidbody2D.velocity = new Vector2(horMov * MOVE_SPEED, rigidbody2D.velocity.y);

                state = CharacterState.FALLING;
                return;
            }

			rigidbody2D.velocity = new Vector2(horMov * MOVE_SPEED, rigidbody2D.velocity.y);
        }
    }


	bool IsMoving(Direction direction)
	{
		return direction == Direction.RIGHT ? horMov > 0 : horMov < 0;
	}

	bool IsWalled(Direction direction)
	{
		if (direction == Direction.LEFT) {
            return facingRight ? IsInEnv(CharacterEnv.WALLED_BACK) : IsInEnv(CharacterEnv.WALLED_FRONT);
		} else {
            return facingRight ? IsInEnv(CharacterEnv.WALLED_FRONT) : IsInEnv(CharacterEnv.WALLED_BACK);
		}
	}

	void Jump()
	{
		state = CharacterState.JUMPING_UP;
		rigidbody2D.velocity = new Vector2 (rigidbody2D.velocity.x, JUMP_SPEED);
	}

	void WallWalk(Direction direction)
	{
		//Already wall walked same wall
		if ((!(state == CharacterState.WALL_WALKING)) && WallWalked (direction))
			return;

        state = CharacterState.WALL_WALKING;

		SetEnv(GetWallWalkStateByDirection (direction), true);
        SetEnv(direction == Direction.RIGHT ? CharacterEnv.WALL_WALKED_LEFT : CharacterEnv.WALL_WALKED_RIGHT, false);
		wallWalkTimer += Time.deltaTime;

		if (wallWalkTimer > WALL_WALK_TIME) {
			EndWallWalk();
            state = CharacterState.FALLING;
			return;
		}

		rigidbody2D.velocity = new Vector2 (rigidbody2D.velocity.x, WALL_WALK_SPEED);
	}

	void EndWallWalk()
	{
		wallWalkTimer = 0f;
	}

	CharacterEnv GetWallWalkStateByDirection(Direction direction)
	{
        return direction == Direction.RIGHT ? CharacterEnv.WALL_WALKED_RIGHT : CharacterEnv.WALL_WALKED_LEFT;
	}

	bool WallWalked(Direction direction)
	{
		return IsInEnv (GetWallWalkStateByDirection(direction));
	}

    bool CheckWallJump()
    {
        do
        {
			if (rigidbody2D.velocity.y < -6f)
				return false;
            //Check Wall and moving direction

            //Climb more;
            if (IsWalled(Direction.LEFT) && IsMoving(Direction.LEFT))
            {
                WallWalk(Direction.LEFT);
				return true;
            }

            if (IsWalled(Direction.RIGHT) && IsMoving(Direction.RIGHT))
            {
                WallWalk(Direction.RIGHT);
				return true;
            }

            //Wall Jump;
            if (IsWalled(Direction.LEFT) && IsMoving(Direction.RIGHT))
            {
                WallJump(Direction.RIGHT);
				return true;
            }

            if (IsWalled(Direction.RIGHT) && IsMoving(Direction.LEFT))
            {
                WallJump(Direction.LEFT);
				return true;
            }

        } while (false);

		return false;
    }

	void WallJump(Direction direction)
	{
		Debug.Log ("Wall Jump!!");
        EndWallWalk();

        state = CharacterState.WALL_JUMPING;
		rigidbody2D.velocity = new Vector2 (direction == Direction.RIGHT ? WALL_JUMP_SPEED_X : -WALL_JUMP_SPEED_X, WALL_JUMP_SPEED_Y);
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

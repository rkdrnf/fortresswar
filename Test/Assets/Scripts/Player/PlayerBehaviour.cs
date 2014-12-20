using UnityEngine;
using System.Collections;
using Const;
using Newtonsoft.Json;
using Util;
using Packet;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using System;

public class PlayerBehaviour : MonoBehaviour {

    int owner;
    bool isOwner = false;

    Job job;
    public JobStat jobStat;

	public WeaponType weapon;
    public int health;
    Animator animator;

    double healthLastSet = 0f;
    double weaponLastSet = 0f;
    
	float FIRE_RATE = 0.2f;
	int FIRE_POWER = 20;

	public bool facingRight = true;

	float fireTimer;

	float horMov;
	float verMov;
	//bool jumpMov;

	float wallWalkTimer;


	NetworkView transformView;
	NetworkView controllerView;

    
    public CharacterState state;

    int envFlag;

    const float REVIVAL_TIME = 5f;
    double revivalTimer = 0f;

    public bool IsInState(CharacterState state, params CharacterState[] stateList)
    {
        return StateUtil.IsInState<CharacterState>(this.state, state, stateList);
    }

    public bool IsNotInState(CharacterState state, params CharacterState[] stateList)
    {
        return StateUtil.IsNotInState<CharacterState>(this.state, state, stateList);
    }

    public void SetState(CharacterState state)
    {
        StateUtil.SetState(out this.state, state);

        if (!Network.isServer) return;

        S2C.CharacterChangeState pck = new S2C.CharacterChangeState(state);
        networkView.RPC("ClientSetState", RPCMode.Others, pck.SerializeToBytes());
        
    }

    [RPC]
    void ClientSetState(byte[] pckData, NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //ServerCheck

        S2C.CharacterChangeState pck = S2C.CharacterChangeState.DeserializeFromBytes(pckData);

        SetState(pck.state);
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

    void Awake()
    {
        fireTimer = 0f;
        wallWalkTimer = 0f;
        animator = GetComponent<Animator>();

        Component[] views = gameObject.GetComponents(typeof(NetworkView));
        foreach (Component view in views)
        {
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

        //Server has valid value.
        if (Network.isServer)
        {
            StateUtil.SetState(out this.state, CharacterState.DEAD);
        }
    }

    [RPC]
    void RequestCurrentStatus(NetworkMessageInfo info)
    {
        S2C.CharacterStatus pck = new S2C.CharacterStatus(job, weapon, health);
        networkView.RPC("SetPlayerStatus", info.sender, pck.SerializeToBytes());
    }

    [RPC]
    void SetPlayerStatus(byte[] data, NetworkMessageInfo info)
    {
        S2C.CharacterStatus pck = S2C.CharacterStatus.DeserializeFromBytes(data);

        LoadJob(pck.job);
        SetHealth(pck.health, info.timestamp);
        SetWeapon(pck.weapon, info.timestamp);
    }

    void SetHealth(int health, double settingTime)
    {
        //old status
        if (healthLastSet > settingTime)
            return;

        healthLastSet = settingTime;
        this.health = health;
    }

    void SetWeapon(WeaponType weapon, double settingTime)
    {
        //old status
        if (weaponLastSet > settingTime)
            return;

        weaponLastSet = settingTime;
        this.weapon = weapon;
    }


    [RPC]
	public void SetOwner(int playerID, NetworkMessageInfo info)
	{
        if (!Network.isClient) return;
        //ServerCheck

        OnSetOwner(playerID);

        networkView.RPC("RequestCurrentStatus", RPCMode.Server);
	}

    public void OnSetOwner(int playerID)
    {
        PlayerManager.Inst.Set(playerID, this);
        owner = playerID;
        isOwner = owner == Game.Inst.GetID();

        if (isOwner) // Set Camera to own character.
        {
            CameraBehaviour camera = GameObject.Find("Main Camera").GetComponent<CameraBehaviour>();
            camera.target = transform;
        }

        if (isOwner && Network.isClient) // Allocating controller ID. When Network is server, ID is already allocated.
        {
            NetworkViewID controllerID = Network.AllocateViewID();
            controllerView.RPC("SetControllerNetworkView", RPCMode.Server, controllerID);
            controllerView.viewID = controllerID;
        }

        if (Network.isServer)
        {
            //LoadJob After Setting Owner;
            LoadJob(job);
        }

        
    }

    public int GetOwner()
    {
        return owner;
    }

	[RPC]
	public void SetControllerNetworkView(NetworkViewID viewID)
	{
		controllerView.viewID = viewID;
	}

    void LoadJob(Job job)
    {
        JobStat newJobStat = Game.Inst.jobSet.Jobs[(int)job];

        this.job = job;
        this.jobStat = newJobStat;

        this.health = newJobStat.MaxHealth;
        this.weapon = newJobStat.Weapons[0];
        PlayerSetting setting = PlayerManager.Inst.GetSetting(owner);

        if (setting != null)
        {
            LoadAnimation(setting.team, newJobStat);
        }
    }

    public void ChangeJob(Job newJob)
    {
        C2S.ChangeJob pck = new C2S.ChangeJob(newJob);
        if (Network.isServer)
        {
            ChangeJobRequest(pck.SerializeToBytes(), new NetworkMessageInfo());
        }
        else if (Network.isClient)
        { 
            
            networkView.RPC("ChangeJobRequest", RPCMode.Server, pck.SerializeToBytes());
        }
    }

    [RPC]
    void ChangeJobRequest(byte[] pckData, NetworkMessageInfo info)
    {
        if (!Network.isServer) return;
        if (!PlayerManager.Inst.IsValidPlayer(owner, info.sender)) return;

        //TODO::JobChangeValidation

        OnChangeJob(C2S.ChangeJob.DeserializeFromBytes(pckData));
        networkView.RPC("ClientChangeJob", RPCMode.Others, pckData);
    }

    [RPC]
    void ClientChangeJob(byte[] pckData, NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //ServerCheck

        OnChangeJob(C2S.ChangeJob.DeserializeFromBytes(pckData));
    }

    void OnChangeJob(C2S.ChangeJob changeJob)
    {
        LoadJob(changeJob.job);
    }

    public void ChangeTeam(C2S.UpdatePlayerTeam pck)
    {
        PlayerSetting setting = PlayerManager.Inst.GetSetting(owner);
        Team team = pck.team;

        LoadAnimation(team, jobStat);
    }

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo mnInfo)
	{
		if (stream.isWriting) {
			Debug.Log ("writing");
			if (isOwner) {
				float horMoveVal = horMov;
				float verMoveVal = verMov;

				stream.Serialize (ref horMoveVal);
				stream.Serialize (ref verMoveVal);
			}
		} else {
			Debug.Log ("reading");
			float horMoveVal = 0;
			float verMoveVal = 0;
            int stateVal = (int)CharacterState.DEAD;

			stream.Serialize(ref horMoveVal);
            stream.Serialize(ref verMoveVal);

			horMov = horMoveVal;
			verMov = verMoveVal;
		}
	}

    void FixedUpdate()
    {
        if (!Network.isServer) return;

        if (IsDead())
        {
            UpdateRevivalTimer(Time.deltaTime);

            if (CanRevive())
            {
                BroadcastRevive();
                
            }
        }

        if (IsDead() == false && Game.Inst.map.CheckInBorder(this) == false)
        {
            BroadcastDie();
        }
    }

	// Update is called once per frame
	// Reset input data per frame.
	void Update () {
        fireTimer += Time.deltaTime;

        //Animation Rendering.
        //Every client must do his own Animation Rendering.

        animator.SetBool("HorMoving", horMov != 0);
        animator.SetBool("Dead", IsInState(CharacterState.DEAD));


        //Client Input Processing
		
        do
        {
            if (!isOwner) break;
            if (IsDead()) break;

            if (Game.Inst.keyFocusManager.IsFocused(InputKeyFocus.PLAYER))
            {
                horMov = Input.GetAxisRaw("Horizontal");
                verMov = Input.GetAxisRaw("Vertical");


                //TeamSelector
                if (Input.GetKey(KeyCode.M))
                {
                    Game.Inst.OpenTeamSelector();
                }

                //JobSelector
                if (Input.GetKey(KeyCode.N))
                {
                    Game.Inst.OpenJobSelector();
                }
            }

            if (Game.Inst.mouseFocusManager.IsFocused(InputMouseFocus.PLAYER))
            {
                if (Input.GetButton("Fire1"))
                {
                    Debug.Log("fire button pressed");
                    Fire();

                }
            }

        } while (false);

		

		// Server Rendering. Rendering from input must be in Update()
		// Fixed Update refreshes in fixed period. 
		// When Update() Period is shorter than fixed period, Update() can be called multiple times between FixedUpdate().
		// Because Input Data is reset in Update(), FixedUpdate() Can't get input data properly.
		if (Network.isServer) {
            if (IsDead())
            {
                horMov = 0f;
                verMov = 0f;
            }

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
            rigidbody2D.velocity = new Vector2(horMov * jobStat.MovingSpeed, rigidbody2D.velocity.y);
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
                rigidbody2D.velocity = new Vector2(horMov * jobStat.MovingSpeed, rigidbody2D.velocity.y);
                
                return;
            }
        }
    }

    void WhenJumping()
    {
        if (horMov != 0)
        {
            rigidbody2D.velocity = new Vector2(horMov * jobStat.MovingSpeed, rigidbody2D.velocity.y);
        }

        if (rigidbody2D.velocity.y <= 0f)
        {
            SetState(CharacterState.FALLING);
        }
    }

    void WhenWallJumping()
    {
        if (horMov != 0)
        {
            rigidbody2D.AddForce(new Vector2(horMov * jobStat.MovingSpeed, 2f));

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
                rigidbody2D.velocity = new Vector2(horMov * jobStat.MovingSpeed, rigidbody2D.velocity.y);

                return;
            }
        }
    }

    void WhenWallWalking()
    {
        //Fall to ground.
        if (horMov == 0)
        {
            SetState(CharacterState.FALLING);
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
                rigidbody2D.velocity = new Vector2(horMov * jobStat.MovingSpeed, rigidbody2D.velocity.y);

                
                SetState(CharacterState.FALLING);
                return;
            }

            rigidbody2D.velocity = new Vector2(horMov * jobStat.MovingSpeed, rigidbody2D.velocity.y);
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
		SetState(CharacterState.JUMPING_UP);
		rigidbody2D.velocity = new Vector2 (rigidbody2D.velocity.x, jobStat.JumpingSpeed);
	}

	void WallWalk(Direction direction)
	{
		//Already wall walked same wall
		if ((!(state == CharacterState.WALL_WALKING)) && WallWalked (direction))
			return;

        SetState(CharacterState.WALL_WALKING);

		SetEnv(GetWallWalkStateByDirection (direction), true);
        SetEnv(direction == Direction.RIGHT ? CharacterEnv.WALL_WALKED_LEFT : CharacterEnv.WALL_WALKED_RIGHT, false);
		wallWalkTimer += Time.deltaTime;

		if (wallWalkTimer > jobStat.WallWalkingTime) {
			EndWallWalk();
            SetState(CharacterState.FALLING);
			return;
		}

		rigidbody2D.velocity = new Vector2 (rigidbody2D.velocity.x, jobStat.WallWalkingSpeed);
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

        SetState(CharacterState.WALL_JUMPING);
        rigidbody2D.velocity = new Vector2(direction == Direction.RIGHT ? jobStat.WallJumpingSpeed.x : -jobStat.WallJumpingSpeed.x, jobStat.WallJumpingSpeed.y);
	}

    void Flip()
    {
        if (IsDead())
            return;

        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }



	void Fire()
	{
        if (IsDead())
            return;


        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Vector2 direction = (worldMousePosition - transform.position);
        direction.Normalize();

        C2S.Fire fire = new C2S.Fire(owner, -1, weapon, Vector3.zero, direction);

        Debug.Log(string.Format("Player {0} pressed Fire", Network.player));

        if (Network.isServer)
        {
            OnServerFire(fire);
        }
        else
        { 
            networkView.RPC("ServerFire", RPCMode.Server, fire.SerializeToBytes());
        }
        
	}

    [RPC]
    public void ServerFire(byte[] fireData, NetworkMessageInfo info)
    {

        if (!Network.isServer) return;
        
        C2S.Fire fire = C2S.Fire.DeserializeFromBytes(fireData);

        if (!PlayerManager.Inst.IsValidPlayer(fire.playerID, info.sender)) return;

        OnServerFire(fire);

        return;
    }

    public void OnServerFire(C2S.Fire fire)
    {
        if (!Network.isServer) return;

        if (CanFire() == false)
        {
            return;
        }

        PlayerBehaviour character = PlayerManager.Inst.Get(fire.playerID);

        long projID = ProjectileManager.Inst.GetUniqueKeyForNewProjectile();

        Debug.Log(string.Format("Fire of player {0}, fireID:{1}", fire.playerID, projID));

        fire.projectileID = projID;
        fire.origin = character.gameObject.transform.position;

        networkView.RPC("BroadcastFire", RPCMode.Others, fire.SerializeToBytes());
        OnBroadcastFire(fire);
    }

    public bool CanFire()
    {
        if (IsDead())
            return false;

        if (fireTimer > FIRE_RATE)
        {
            fireTimer = 0;
            
            return true;
        }
        return false;
    }

    [RPC]
    public void BroadcastFire(byte[] fireData, NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //ServerCheck

        C2S.Fire fire = C2S.Fire.DeserializeFromBytes(fireData);

        OnBroadcastFire(fire);
    }

    public void OnBroadcastFire(C2S.Fire fire)
    {
        GameObject projObj = (GameObject)Instantiate(Game.Inst.projectileSet.projectiles[(int)fire.weaponType], fire.origin, Quaternion.identity);

        projObj.rigidbody2D.AddForce(new Vector2(fire.direction.x * FIRE_POWER, fire.direction.y * FIRE_POWER), ForceMode2D.Impulse);

        Projectile proj = projObj.GetComponent<Projectile>();
        proj.ID = fire.projectileID;
        ProjectileManager.Inst.Set(fire.projectileID, proj);

        Debug.Log(string.Format("Fire ID :{0} registered", fire.projectileID));
        proj.owner = fire.playerID;
    }

    public void Damage(int damage, NetworkMessageInfo info)
    {
        if (!Network.isServer) return;
        
        if (IsDead())
            return;

        health -= damage;
        if (health < 0)
        {
            health = 0;
        }

        networkView.RPC("ClientDamage", RPCMode.Others, damage);
        
        if(health <= 0)
        {
            BroadcastDie();
        }
    }

    [RPC]
    void ClientDamage(int damage, NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //ServerCheck

        //old damage;
        if (healthLastSet > info.timestamp) return;

        health -= damage;
        if (health < 0)
        {
            health = 0;
        }
    }

    public void BroadcastDie()
    {
        if (!Network.isServer) return;

        networkView.RPC("Die", RPCMode.Others);
        OnDie();
    }

    [RPC]
    void Die(NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //ServerCheck

        OnDie();
    }

    void OnDie()
    {
        SetState(CharacterState.DEAD);
        
        revivalTimer = REVIVAL_TIME;
    }

    public bool IsDead()
    {
        return IsInState(CharacterState.DEAD);
    }

    void UpdateRevivalTimer(double deltaTime)
    {
        revivalTimer -= deltaTime;
    }

    bool CanRevive()
    {
        return revivalTimer <= 0;
    }

    void BroadcastRevive()
    {
        Revive();
        networkView.RPC("ClientRevive", RPCMode.Others);
    }

    void Revive()
    {
        if (!Network.isServer) return;

        transform.position = Game.Inst.RevivalLocation;
        rigidbody2D.velocity = Vector2.zero;
        OnRevive();
    }

    [RPC]
    void ClientRevive(NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //ServerCheck

        OnRevive();
    }

    void OnRevive()
    {
        SetState(CharacterState.FALLING);
        
        fireTimer = 0f;
        health = jobStat.MaxHealth;
    }

    void LoadAnimation(Team team, JobStat jobStat)
    {
        animator.runtimeAnimatorController = team == Team.BLUE ?
            jobStat.BlueTeamAnimations :
            jobStat.RedTeamAnimations;
    }

    public void OnSettingChange()
    {
        PlayerSetting setting = PlayerManager.Inst.GetSetting(owner);
        Team team = setting.team;

        LoadAnimation(team, jobStat);
    }

    public void RemoveCharacterFromNetwork()
    {
        Network.RemoveRPCs(transformView.viewID);
        Network.RemoveRPCs(controllerView.viewID);
        Network.Destroy(gameObject);
    }
}


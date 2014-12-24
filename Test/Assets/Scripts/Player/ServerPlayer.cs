﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Const;
using Util;
using Packet;
using S2C = Packet.S2C;
using C2S = Packet.C2S;

namespace Server
{
    public class ServerPlayer : MonoBehaviour
    {

        int owner;
        bool isOwner = false;

        public int GetOwner()
        {
            return owner;
        }

        public bool IsMine()
        {
            return isOwner;
        }

        Job job;
        public JobStat jobStat;

        public WeaponType weapon;
        public int health;
        Animator animator;

        public bool facingRight = true;

        WeaponManager weaponManager;

        float horMov
        {
            get { return clientPlayer.horMov; }
        }
        float verMov
        {
            get { return clientPlayer.verMov; }
        }

        float wallWalkTimer;


        NetworkView controllerView;
        NetworkView transformView;

        public Vector2 lookingDirection
        {
            get { return clientPlayer.lookingDirection; }
        }



        int envFlag;

        const float REVIVAL_TIME = 5f;
        double revivalTimer = 0f;

        CharacterSM stateManager;

        Client.ClientGame client
        {
            get { return Client.ClientGame.Inst; }
        }
        ServerGame server
        {
            get { return ServerGame.Inst; }
        }

        Client.PlayerBehaviour clientPlayer;

        void Awake()
        {
            weaponManager = new WeaponManager(this);
            wallWalkTimer = 0f;
            animator = GetComponent<Animator>();

            Component[] views = gameObject.GetComponents(typeof(NetworkView));
            foreach (Component view in views)
            {
                NetworkView nView = (NetworkView)view;
                if (nView.observed is Client.PlayerBehaviour)
                {
                    controllerView = nView;
                }

                if (nView.observed is NetworkInterpolatedTransform)
                {
                    transformView = nView;
                }
            }

            clientPlayer = GetComponent<Client.PlayerBehaviour>();

            stateManager = new CharacterSM(CharacterState.DEAD, this);
        }

        public CharacterState GetState()
        {
            return stateManager.GetState();
        }

        public void BroadcastState()
        {
            S2C.CharacterChangeState pck = new S2C.CharacterChangeState(stateManager.GetState());

            if (!ServerGame.Inst.isDedicatedServer)
                networkView.RPC("ClientSetState", RPCMode.All, pck.SerializeToBytes());
            else
                networkView.RPC("ClientSetState", RPCMode.Others, pck.SerializeToBytes());
        }

        public void OnTouchGround()
        {
            if (stateManager.IsInState(CharacterState.GROUNDED, CharacterState.JUMPING_UP))
            {
                //Maintain State;
                return;
            }

            if (stateManager.IsInState(CharacterState.FALLING, CharacterState.WALL_JUMPING, CharacterState.WALL_WALKING))
            {
                Debug.Log("grounded!");
                stateManager.SetState(CharacterState.GROUNDED);
                return;
            }
        }

        public void OnAwayFromGround()
        {
            if (stateManager.IsInState(CharacterState.GROUNDED))
            {
                stateManager.SetState(CharacterState.FALLING);
                return;
            }

            if (stateManager.IsInState(CharacterState.JUMPING_UP, CharacterState.WALL_WALKING, CharacterState.WALL_JUMPING, CharacterState.FALLING))
            {
                //Maintain state;
                return;
            }
        }

        public bool IsInEnv(CharacterEnv env, params CharacterEnv[] envList)
        {
            bool result = (envFlag & (1 << (int)env)) != 0;


            foreach (CharacterEnv envVal in envList)
            {
                if (result == false)
                    break;

                result = result && ((envFlag & (1 << (int)env)) != 0);
            }

            return result;
        }

        public void SetEnv(CharacterEnv env, bool value)
        {
            if (value)
            {
                envFlag = envFlag | (1 << (int)env);
            }
            else
            {
                envFlag = envFlag & (~(1 << (int)env));
            }
        }

        

        [RPC]
        public void RequestCurrentStatus(NetworkMessageInfo info)
        {
            S2C.CharacterStatus pck = new S2C.CharacterStatus(job, weapon, health, stateManager.GetState());

            if (info.sender == Network.player)
            {
                clientPlayer.SetPlayerStatus(pck.SerializeToBytes(), new NetworkMessageInfo());
            }
            else
            {
                networkView.RPC("SetPlayerStatus", info.sender, pck.SerializeToBytes());
            }
        }

        public void Init(int playerID)
        {
            owner = playerID;
            LoadJob(job);
        }

        [RPC]
        public void SetControllerNetworkView(NetworkViewID viewID)
        {
            controllerView.viewID = viewID;
            controllerView.enabled = true;
        }

        void LoadJob(Job job)
        {
            JobStat newJobStat = Game.Inst.jobSet.Jobs[(int)job];

            this.job = job;
            this.jobStat = newJobStat;

            this.health = newJobStat.MaxHealth;
            this.weapon = newJobStat.Weapons[0];

            weaponManager.LoadWeapons(newJobStat.Weapons);
        }

        [RPC]
        public void ChangeJobRequest(byte[] pckData, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;
            if (!PlayerManager.Inst.IsValidPlayer(owner, info.sender)) return;

            //TODO::JobChangeValidation

            LoadJob(C2S.ChangeJob.DeserializeFromBytes(pckData).job);

            if (!ServerGame.Inst.isDedicatedServer)
                networkView.RPC("ClientChangeJob", RPCMode.All, pckData);
            else
                networkView.RPC("ClientChangeJob", RPCMode.Others, pckData);
        }

        void FixedUpdate()
        {
            if (!Network.isServer) return;

            weaponManager.RefreshFireRate(Time.fixedDeltaTime);

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

        void Update()
        {
            // Server Rendering. Rendering from input must be in Update()
            // Fixed Update refreshes in fixed period. 
            // When Update() Period is shorter than fixed period, Update() can be called multiple times between FixedUpdate().
            // Because Input Data is reset in Update(), FixedUpdate() Can't get input data properly.
            if (Network.isServer)
            {
                

                if (IsDead())
                {
                    return;
                }

                do
                {
                    if (stateManager.IsInState(CharacterState.GROUNDED))
                    {
                        WhenGrounded();
                        break;
                    }

                    if (stateManager.IsInState(CharacterState.FALLING))
                    {
                        WhenFalling();
                        break;
                    }

                    if (stateManager.IsInState(CharacterState.JUMPING_UP))
                    {
                        WhenJumping();
                        break;
                    }

                    if (stateManager.IsInState(CharacterState.WALL_JUMPING))
                    {
                        WhenWallJumping();
                        break;
                    }

                    if (stateManager.IsInState(CharacterState.WALL_WALKING))
                    {
                        WhenWallWalking();
                        break;
                    }
                }
                while (false);


                if (lookingDirection.x < 0 && facingRight)
                {
                    Flip();
                }
                if (lookingDirection.x > 0 && !facingRight)
                {
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
                stateManager.SetState(CharacterState.FALLING);
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
                stateManager.SetState(CharacterState.FALLING);
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


                    stateManager.SetState(CharacterState.FALLING);
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
            if (direction == Direction.LEFT)
            {
                return facingRight ? IsInEnv(CharacterEnv.WALLED_BACK) : IsInEnv(CharacterEnv.WALLED_FRONT);
            }
            else
            {
                return facingRight ? IsInEnv(CharacterEnv.WALLED_FRONT) : IsInEnv(CharacterEnv.WALLED_BACK);
            }
        }

        void Jump()
        {
            stateManager.SetState(CharacterState.JUMPING_UP);
            rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, jobStat.JumpingSpeed);
        }

        void WallWalk(Direction direction)
        {
            //Already wall walked same wall
            if ((stateManager.IsNotInState(CharacterState.WALL_WALKING)) && WallWalked(direction))
                return;

            stateManager.SetState(CharacterState.WALL_WALKING);

            SetEnv(GetWallWalkStateByDirection(direction), true);
            SetEnv(direction == Direction.RIGHT ? CharacterEnv.WALL_WALKED_LEFT : CharacterEnv.WALL_WALKED_RIGHT, false);
            wallWalkTimer += Time.deltaTime;

            if (wallWalkTimer > jobStat.WallWalkingTime)
            {
                EndWallWalk();
                stateManager.SetState(CharacterState.FALLING);
                return;
            }

            rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, jobStat.WallWalkingSpeed);
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
            return IsInEnv(GetWallWalkStateByDirection(direction));
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
            Debug.Log("Wall Jump!!");
            EndWallWalk();

            stateManager.SetState(CharacterState.WALL_JUMPING);
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

        [RPC]
        public void ServerFire(byte[] fireData, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;
            if (!PlayerManager.Inst.IsValidPlayer(owner, info.sender)) return;

            C2S.Fire fire = C2S.Fire.DeserializeFromBytes(fireData);

            weaponManager.Fire(fire);
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

            if (!ServerGame.Inst.isDedicatedServer)
                networkView.RPC("ClientDamage", RPCMode.All, damage);
            else
                networkView.RPC("ClientDamage", RPCMode.Others, damage);

            if (health <= 0)
            {
                BroadcastDie();
            }
        }

        public void BroadcastDie()
        {
            if (!Network.isServer) return;

            OnDie();
            networkView.RPC("Die", RPCMode.Others);
        }

        void OnDie()
        {
            stateManager.SetState(CharacterState.DEAD);
            revivalTimer = REVIVAL_TIME;
        }

        public bool IsDead()
        {
            return stateManager.IsInState(CharacterState.DEAD);
        }

        void UpdateRevivalTimer(double deltaTime)
        {
            revivalTimer -= deltaTime;
        }

        bool CanRevive()
        {
            return jobStat != null && revivalTimer <= 0;
        }

        void BroadcastRevive()
        {
            if (!Network.isServer) return;

            Revive();

            if (!ServerGame.Inst.isDedicatedServer)
                networkView.RPC("ClientRevive", RPCMode.All);
            else
                networkView.RPC("ClientRevive", RPCMode.Others);
        }

        void Revive()
        {
            if (!Network.isServer) return;

            transform.position = server.RevivalLocation;
            rigidbody2D.velocity = Vector2.zero;

            stateManager.SetState(CharacterState.FALLING);

            weaponManager.ReloadAll();
            health = jobStat.MaxHealth;
        }

        public void RemoveCharacterFromNetwork()
        {
            Network.RemoveRPCs(transformView.viewID);
            Network.RemoveRPCs(controllerView.viewID);
            Network.Destroy(gameObject);
        }
    }

    class CharacterSM : StateManager<CharacterState>
    {
        ServerPlayer player;

        public CharacterSM(CharacterState initial, ServerPlayer owner)
        {
            state = initial;
            player = owner;
        }

        public override void SetState(CharacterState newState)
        {
            base.SetState(newState);

            player.BroadcastState();
        }
    }
}

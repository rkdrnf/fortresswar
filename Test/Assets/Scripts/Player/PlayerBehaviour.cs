using UnityEngine;
using System.Collections;
using Const;
using Newtonsoft.Json;
using Util;
using Packet;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using System;

namespace Client
{
    public class PlayerBehaviour : MonoBehaviour
    {

        int owner;
        bool isOwner = false;

        Job job;
        public JobStat jobStat;

        public WeaponType weapon;
        public int health;
        Animator animator;

        double healthLastSet = 0f;
        double weaponLastSet = 0f;

        public float horMov;
        public float verMov;

        NetworkView transformView;
        NetworkView controllerView;

        public Vector2 lookingDirection;



        int envFlag;

        const float REVIVAL_TIME = 5f;
        double revivalTimer = 0f;

        CharacterSM stateManager;

        ClientGame client
        {
            get { return ClientGame.Inst; }
        }
        Server.ServerGame server
        {
            get { return Server.ServerGame.Inst; }
        }

        Server.ServerPlayer serverPlayer;

        public CharacterState GetState()
        {
            return stateManager.GetState();
        }

        [RPC]
        void ClientSetState(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck

            S2C.CharacterChangeState pck = S2C.CharacterChangeState.DeserializeFromBytes(pckData);

            stateManager.SetState(pck.state);
        }

        void Awake()
        {
            animator = GetComponent<Animator>();

            serverPlayer = GetComponent<Server.ServerPlayer>();

            stateManager = new CharacterSM(CharacterState.DEAD);

            Component[] views = gameObject.GetComponents(typeof(NetworkView));
            foreach (Component view in views)
            {
                NetworkView nView = (NetworkView)view;
                if (nView.observed is PlayerBehaviour)
                {
                    controllerView = nView;
                }

                if (nView.observed is NetworkInterpolatedTransform)
                {
                    transformView = nView;
                }
            }
        }

        [RPC]
        public void SetPlayerStatus(byte[] data, NetworkMessageInfo info)
        {
            //ServerCheck

            S2C.CharacterStatus pck = S2C.CharacterStatus.DeserializeFromBytes(data);

            LoadJob(pck.job);
            SetHealth(pck.health, info.timestamp);
            SetWeapon(pck.weapon, info.timestamp);

            stateManager.SetState(pck.state);
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
            //ServerCheck

            OnSetOwner(playerID);

            if (Network.isServer)
            {
                serverPlayer.RequestCurrentStatus(new NetworkMessageInfo());
            }
            else
            {
                networkView.RPC("RequestCurrentStatus", RPCMode.Server);
            }
        }

        public void OnSetOwner(int playerID)
        {
            C_PlayerManager.Inst.Set(playerID, this);
            owner = playerID;
            isOwner = owner == client.GetID();

            if (isOwner) // Set Camera to own character.
            {
                CameraBehaviour camera = GameObject.Find("Main Camera").GetComponent<CameraBehaviour>();
                camera.target = transform;
            }

            if (isOwner && !Network.isServer) // Allocating controller ID. When Network is server, ID is already allocated.
            {
                Network.RemoveRPCs(controllerView.viewID);
                NetworkViewID controllerID = Network.AllocateViewID();
                controllerView.RPC("SetControllerNetworkView", RPCMode.Server, controllerID);
                controllerView.viewID = controllerID;
            }
        }

        public int GetOwner()
        {
            return owner;
        }

        public bool IsMine()
        {
            return isOwner;
        }

        void LoadJob(Job job)
        {
            JobStat newJobStat = Game.Inst.jobSet.Jobs[(int)job];

            this.job = job;
            this.jobStat = newJobStat;

            this.health = newJobStat.MaxHealth;
            this.weapon = newJobStat.Weapons[0];
            PlayerSetting setting = C_PlayerManager.Inst.GetSetting(owner);

            if (setting != null)
            {
                LoadAnimation(setting.team, newJobStat);
            }
        }

        public void ChangeJob(Job newJob)
        {
            C2S.ChangeJob pck = new C2S.ChangeJob(newJob);

            if (Network.isServer) //own server player
            {
                serverPlayer.ChangeJobRequest(pck.SerializeToBytes(), new NetworkMessageInfo());
            }
            else if (Network.isClient)
            {

                networkView.RPC("ChangeJobRequest", RPCMode.Server, pck.SerializeToBytes());
            }
        }

        [RPC]
        void ClientChangeJob(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck

            LoadJob(C2S.ChangeJob.DeserializeFromBytes(pckData).job);
        }

        public void OnChangeTeam(Team team)
        {
            LoadAnimation(team, jobStat);
        }

        void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo mnInfo)
        {
            if (stream.isWriting)
            {
                //Debug.Log (string.Format("[WRITING] player: {0} sender: {1} time: {2}", owner, mnInfo.sender, mnInfo.timestamp));
                if (isOwner)
                {
                    float horMoveVal = horMov;
                    float verMoveVal = verMov;
                    Vector3 lookingDirectionVal = lookingDirection;

                    stream.Serialize(ref horMoveVal);
                    stream.Serialize(ref verMoveVal);
                    stream.Serialize(ref lookingDirectionVal);
                }
            }
            else if (stream.isReading)
            {
                //Debug.Log(string.Format("[READING] player: {0}  sender: {1} time: {2}", owner, mnInfo.sender, mnInfo.timestamp));
                float horMoveVal = 0;
                float verMoveVal = 0;
                Vector3 lookingDirectionVal = Vector2.right;

                stream.Serialize(ref horMoveVal);
                stream.Serialize(ref verMoveVal);
                stream.Serialize(ref lookingDirectionVal);

                horMov = horMoveVal;
                verMov = verMoveVal;
                lookingDirection = lookingDirectionVal;
            }
        }

        // Update is called once per frame
        // Reset input data per frame.
        void Update()
        {
            //Animation Rendering.
            //Every client must do his own Animation Rendering.

            animator.SetBool("HorMoving", horMov != 0);
            animator.SetBool("Dead", stateManager.IsInState(CharacterState.DEAD));


            //Client Input Processing

            do
            {
                if (!isOwner) break;
                if (IsDead()) break;

                if (client.keyFocusManager.IsFocused(InputKeyFocus.PLAYER))
                {
                    horMov = Input.GetAxisRaw("Horizontal");
                    verMov = Input.GetAxisRaw("Vertical");


                    //TeamSelector
                    if (Input.GetKey(KeyCode.M))
                    {
                        client.teamSelector.Open();
                    }

                    //JobSelector
                    if (Input.GetKey(KeyCode.N))
                    {
                        client.jobSelector.Open();
                    }
                }

                if (client.mouseFocusManager.IsFocused(InputMouseFocus.PLAYER))
                {
                    if (Input.GetButton("Fire1"))
                    {
                        Debug.Log("fire button pressed");
                        Fire();

                    }
                }

                Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                lookingDirection = (worldMousePosition - transform.position);

            } while (false);
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
                serverPlayer.ServerFire(fire.SerializeToBytes(), new NetworkMessageInfo());
            }
            else
            {
                networkView.RPC("ServerFire", RPCMode.Server, fire.SerializeToBytes());
            }

        }

        [RPC]
        public void BroadcastFire(byte[] fireData, NetworkMessageInfo info)
        {
            //ServerCheck

            C2S.Fire fire = C2S.Fire.DeserializeFromBytes(fireData);

            GameObject projObj = (GameObject)Instantiate(Game.Inst.weaponSet.weapons[(int)fire.weaponType].weaponPrefab, fire.origin, Quaternion.identity);

            Projectile proj = projObj.GetComponent<Projectile>();

            projObj.rigidbody2D.AddForce(new Vector2(fire.direction.x * proj.power, fire.direction.y * proj.power), ForceMode2D.Impulse);
            proj.ID = fire.projectileID;
            ProjectileManager.Inst.Set(fire.projectileID, proj);

            Debug.Log(string.Format("Fire ID :{0} registered", fire.projectileID));
            proj.owner = fire.playerID;
        }



        [RPC]
        void ClientDamage(int damage, NetworkMessageInfo info)
        {
            //ServerCheck

            //old damage;
            if (healthLastSet > info.timestamp) return;

            health -= damage;
            if (health < 0)
            {
                health = 0;
            }
        }

        [RPC]
        void Die(NetworkMessageInfo info)
        {
            //ServerCheck
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


        [RPC]
        void ClientRevive(NetworkMessageInfo info)
        {
            //ServerCheck

            stateManager.SetState(CharacterState.FALLING);

            if (jobStat != null)
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
            PlayerSetting setting = C_PlayerManager.Inst.GetSetting(owner);
            Team team = setting.team;

            LoadAnimation(team, jobStat);
        }

        
    }

    class CharacterSM : StateManager<CharacterState>
    {
        public CharacterSM(CharacterState initial)
        {
            state = initial;
        }

        public override void SetState(CharacterState newState)
        {
            base.SetState(newState);
        }
    }
}


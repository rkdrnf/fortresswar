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

        public int health;
        Animator animator;
        Team team;

        double healthLastSet = 0f;
        
        public float horMov;
        public float verMov;
        public Vector2 lookingDirection;

        NetworkView transformView;
        NetworkView controllerView;

        public Material outlineMaterial;
        public Material normalMaterial;

        public double highlightTime;
        double highlightTimer;

        SpriteRenderer characterRenderer;

        KeyCode[] weaponCodes = new KeyCode[4]{KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4};

        KeyCode[] skillCodes = new KeyCode[] { KeyCode.R };

        int envFlag;

        const float REVIVAL_TIME = 5f;
        double revivalTimer = 0f;

        CharacterSM stateManager;

        C_WeaponManager weaponManager;

        WeaponUI weaponUI;

        ClientGame client
        {
            get { return ClientGame.Inst; }
        }
        Server.ServerGame server
        {
            get { return Server.ServerGame.Inst; }
        }

        public Server.ServerPlayer serverPlayer;

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
            C_PlayerManager.Inst.StartLoadUser(this);

            weaponManager = GetComponent<C_WeaponManager>();
            weaponManager.Init(this);

            animator = GetComponent<Animator>();

            characterRenderer = GetComponent<SpriteRenderer>();

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

        void SetHealth(int health, double settingTime)
        {
            //old status
            if (healthLastSet > settingTime)
                return;

            healthLastSet = settingTime;
            this.health = health;
        }

        [RPC]
        void InitFinished(NetworkMessageInfo info)
        {
            //ServerCheck

            if (Network.isServer)
                serverPlayer.RequestCharacterStatus(new NetworkMessageInfo());
            else
                networkView.RPC("RequestCharacterStatus", RPCMode.Server);
        }

        [RPC]
        public void SetOwner(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck

            S2C.CharacterStatus pck = S2C.CharacterStatus.DeserializeFromBytes(pckData);

            OnSetOwner(pck.playerID);

            C_PlayerManager.Inst.SetSetting(pck.setting);
            OnSettingChange(pck.setting);

            LoadJob(pck.info.job);

            SetHealth(pck.info.health, info.timestamp);
            stateManager.SetState(pck.info.state);

            weaponManager.ChangeWeapon(pck.info.weapon, info.timestamp);

            C_PlayerManager.Inst.CompleteLoad(this);

            if (C_PlayerManager.Inst.IsLoadComplete())
            {
                ClientGame.Inst.OnPlayerLoadCompleted();
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

                GameObject weaponUIObj = new GameObject();
                weaponUIObj.transform.parent = this.transform;
                weaponUI = weaponUIObj.AddComponent<WeaponUI>();
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

            weaponManager.LoadWeapons(newJobStat.Weapons);

            LoadAnimation();
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

        public void OnChangeTeam(Team newTeam)
        {
            this.team = newTeam;
            LoadAnimation();
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

            if (highlightTimer > 0)
                highlightTimer -= Time.deltaTime;
            if (highlightTimer <= 0)
            {
                ChangeMaterial(normalMaterial);
            }

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

                    if (Input.GetKey(KeyCode.Space))
                    {
                        ClientJump();
                    }

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

                    foreach (KeyCode code in weaponCodes)
                    {
                        if (Input.GetKeyDown(code))
                        {
                            weaponManager.ChangeWeapon(code);
                            break;
                        }
                    }

                    foreach (KeyCode code in skillCodes)
                    {
                        if (Input.GetKeyDown(code))
                        {
                            weaponManager.UseSkill(code);
                            break;
                        }
                    }
                }
                else
                {
                    Input.ResetInputAxes();
                    horMov = 0f;
                    verMov = 0f;
                }

                if (client.mouseFocusManager.IsFocused(InputMouseFocus.PLAYER))
                {
                    if (Input.GetButton("Fire1"))
                    {
                        weaponManager.Fire();
                    }

                    if (Input.GetButtonUp("Fire1"))
                    {
                        weaponManager.FireCharged();
                    }
                }

                

                Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                lookingDirection = (worldMousePosition - transform.position);

            } while (false);
        }
        
        void ClientJump()
        {
            if (Network.isServer)
                serverPlayer.Jump(new NetworkMessageInfo());
            else
                networkView.RPC("Jump", RPCMode.Server);
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

            if(damage > 0)
            { 
                ChangeMaterial(outlineMaterial);
                highlightTimer = highlightTime;
            }
        }

        void ChangeMaterial(Material material)
        {
            characterRenderer.material = material;
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

        void LoadAnimation()
        {
            if (jobStat == null) return;

            animator.runtimeAnimatorController = team == Team.BLUE ?
                jobStat.BlueTeamAnimations :
                jobStat.RedTeamAnimations;
        }

        public void OnSettingChange(PlayerSetting setting)
        {
            team = setting.team;

            LoadAnimation();
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Const;
using Util;
using Packet;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using System.Threading;
using Character;
using Maps;
using Const.Character;

    public class ServerPlayer : MonoBehaviour, IRopable
    {

        int m_owner;
        bool isOwner = false;

        public int GetOwner()
        {
            return m_owner;
        }

        public bool IsMine()
        {
            return isOwner;
        }

        Job m_job;
        public Job GetJob()
        {
            return m_job;
        }


        public JobStat m_jobStat;
        public JobStat GetJobStat()
        {
            return m_jobStat;
        }

        int m_health;
        public int GetHealth()
        {
            return m_health;
        }

        
        WeaponManager m_weaponManager;


        NetworkView controllerView;
        NetworkView transformView;

        public PhysicsMaterial2D bodyMaterial;
        public PhysicsMaterial2D onAirMaterial;

        UI.WeaponUI m_weaponUI;

        Collider2D bodyCollider;

        const float REVIVAL_TIME = 5f;
        double revivalTimer = 0f;

        Team m_team;
        public Team GetTeam()
        {
            return m_team;
        }
        
        ServerGame server
        {
            get { return ServerGame.Inst; }
        }

        CharacterMoveController m_controller;
        CharacterInputProcessor m_inputProcessor;
        BuildController m_buildController;
        CharacterRenderer m_characterRenderer;

        public float GetInputX()
        {
            return m_inputProcessor.GetInputX();
        }
        public float GetInputY()
        {
            return m_inputProcessor.GetInputY();
        }

        public Vector2 GetInputLookingDirection()
        {
            return m_inputProcessor.GetLookingDirection();
        }

        public bool IsLookingRight()
        {
            return m_inputProcessor.IsLookingRight();
        }

        public bool IsFacingRight()
        {
            return m_controller.IsFacingRight();
        }
        
        void Awake()
        {
            PlayerManager.Inst.StartLoadUser(this);

            networkView.group = NetworkViewGroup.PLAYER;
            m_weaponManager = GetComponent<WeaponManager>();
            m_weaponManager.Init(this);

            bodyCollider = GetComponent<Collider2D>();

            m_controller = new CharacterMoveController(this);
            m_ropeController = new RopeController(this);
            m_inputProcessor = new CharacterInputProcessor(this);
            m_buildController = new BuildController(this);
            m_characterRenderer = new CharacterRenderer(this);

            Component[] views = gameObject.GetComponents(typeof(NetworkView));
            foreach (Component view in views)
            {
                NetworkView nView = (NetworkView)view;
                if (nView.observed is ServerPlayer)
                {
                    controllerView = nView;
                }

                if (nView.observed is NetworkInterpolatedTransform)
                {
                    transformView = nView;
                }
            }

            m_stateManager = new CharacterSM(CharacterState.DEAD, this);
            m_envManager = new CharacterEM();

            
        }


        // StateManager
        [HideInInspector]
        CharacterSM m_stateManager;

        public CharacterState GetState()
        {
            return m_stateManager.GetState();
        }
        public void SetState(CharacterState state)
        {
            m_stateManager.SetState(state);
        }
        public bool IsInState(CharacterState state, params CharacterState[] stateList)
        {
            return m_stateManager.IsInState(state, stateList);
        }
        public bool IsNotInState(CharacterState state, params CharacterState[] states)
        {
            return m_stateManager.IsNotInState(state, states);
        }

        public void BroadcastState()
        {
            if (!Network.isServer) return;

            S2C.CharacterChangeState pck = new S2C.CharacterChangeState(m_stateManager.GetState());

            networkView.RPC("RecvState", RPCMode.Others, pck.SerializeToBytes());
        }

        [RPC]
        void RecvState(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck
            S2C.CharacterChangeState pck = S2C.CharacterChangeState.DeserializeFromBytes(pckData);

            m_stateManager.SetState(pck.state);
        }

        //EnvManager
        [HideInInspector]
        CharacterEM m_envManager;
        public void SetEnv(CharacterEnv env, bool set)
        {
            m_envManager.SetEnv(env, set);
        }
        public bool IsInEnv(CharacterEnv env, params CharacterEnv[] envList)
        {
            return m_envManager.IsInEnv(env, envList);
        }

        public void TryJump()
        {
            if (Network.isServer)
                Jump(new NetworkMessageInfo());
            else
                networkView.RPC("Jump", RPCMode.Server);
        }

        public void OnTouchGround()
        {
            if (m_stateManager.IsInState(CharacterState.GROUNDED, CharacterState.JUMPING_UP))
            {
                //Maintain State;
                return;
            }

            if (m_stateManager.IsInState(CharacterState.FALLING, CharacterState.WALL_JUMPING, CharacterState.WALL_WALKING))
            {
                Debug.Log("grounded!");
                m_stateManager.SetState(CharacterState.GROUNDED);
                return;
            }
        }

        public void OnAwayFromGround()
        {
            if (m_stateManager.IsInState(CharacterState.GROUNDED))
            {
                m_stateManager.SetState(CharacterState.FALLING);
                return;
            }

            if (m_stateManager.IsInState(CharacterState.JUMPING_UP, CharacterState.WALL_WALKING, CharacterState.WALL_JUMPING, CharacterState.FALLING))
            {
                //Maintain state;
                return;
            }
        }

        [RPC]
        public void RequestCharacterStatus(NetworkMessageInfo info)
        {
            PlayerSetting setting = PlayerManager.Inst.GetSetting(m_owner);
            S2C.CharacterStatus pck = new S2C.CharacterStatus(setting.playerID, setting, GetInfo());

            networkView.RPC("SetOwner", info.sender, pck.SerializeToBytes());
        }

        [RPC]
        public void SetOwner(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck

            S2C.CharacterStatus pck = S2C.CharacterStatus.DeserializeFromBytes(pckData);

            PlayerManager.Inst.Set(pck.playerID, this);

            LoadSetting(pck.setting);
            PlayerManager.Inst.SetSetting(pck.setting);
            LoadJob(pck.info.job);

            OnSetOwner(pck.playerID);

            SetHealth(pck.info.health, DamageReason.MANUAL);
            m_stateManager.SetState(pck.info.state);

            m_weaponManager.TryChangeWeapon(pck.info.weapon, info.timestamp);

            PlayerManager.Inst.CompleteLoad(this);

            if (PlayerManager.Inst.IsLoadComplete())
            {
                ServerGame.Inst.OnPlayerLoadCompleted();
            }
        }


        public void OnSetOwner(int playerID)
        {
            isOwner = m_owner == ServerGame.Inst.GetID();

            if (isOwner) // Set Camera to own character.
            {
                CameraBehaviour camera = GameObject.Find("Main Camera").GetComponent<CameraBehaviour>();
                camera.target = transform;

                GameObject weaponUIObj = new GameObject();
                weaponUIObj.transform.parent = this.transform;
                m_weaponUI = weaponUIObj.AddComponent<UI.WeaponUI>();
            }

            if (isOwner && !Network.isServer) // Allocating controller ID. When Network is server, ID is already allocated.
            {
                Network.RemoveRPCs(controllerView.viewID);
                NetworkViewID controllerID = Network.AllocateViewID();
                controllerView.RPC("SetControllerNetworkView", RPCMode.Server, controllerID);
                controllerView.viewID = controllerID;
            }
        }

        public S2C.CharacterInfo GetInfo()
        {
            return new S2C.CharacterInfo(m_job, m_weaponManager.GetCurrentWeaponType(), m_health, m_stateManager.GetState());
        }
        
        public void Init(PlayerSetting setting)
        {
            if (!Network.isServer) return;

            LoadSetting(setting);

            LoadJob(m_job);

            if (!ServerGame.Inst.isDedicatedServer) // own player
            {
                networkView.RPC("RecvInitFinished", RPCMode.AllBuffered);
                //newPlayer.networkView.RPC("SetOwner", RPCMode.AllBuffered, pck.SerializeToBytes());
            }
            else
            {
                networkView.RPC("RecvInitFinished", RPCMode.OthersBuffered);
                //newPlayer.networkView.RPC("SetOwner", RPCMode.OthersBuffered, pck.SerializeToBytes());
            }
        }

        [RPC]
        void RecvInitFinished(NetworkMessageInfo info)
        {
            //ServerCheck

            if (Network.isServer)
                OnSetOwner(m_owner);
            else
                networkView.RPC("RequestCharacterStatus", RPCMode.Server);
        }

        [RPC]
        public void SetControllerNetworkView(NetworkViewID viewID)
        {
            controllerView.viewID = viewID;
            controllerView.enabled = true;
        }

        

        public void TryChangeJob(Job newJob)
        {
            C2S.ChangeJob pck = new C2S.ChangeJob(newJob);

            if (Network.isServer) //own server player
            {
                ChangeJob(pck.SerializeToBytes(), new NetworkMessageInfo());
            }
            else if (Network.isClient)
            {

                networkView.RPC("ChangeJob", RPCMode.Server, pck.SerializeToBytes());
            }
        }

        [RPC]
        public void ChangeJob(byte[] pckData, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;
            if (!PlayerManager.Inst.IsValidPlayer(m_owner, info.sender)) return;

            //TODO::JobChangeValidation

            LoadJob(C2S.ChangeJob.DeserializeFromBytes(pckData).job);

            networkView.RPC("RecvChangeJob", RPCMode.Others, pckData);
        }

        [RPC]
        void RecvChangeJob(byte[] pckData, NetworkMessageInfo info)
        {
            if (!Network.isClient) return;
            //ServerCheck

            LoadJob(C2S.ChangeJob.DeserializeFromBytes(pckData).job);
        }

        void LoadJob(Job job)
        {
            JobStat newJobStat = Game.Inst.jobSet.Jobs[(int)job];

            
            m_job = job;
            m_jobStat = newJobStat;

            if (Network.isServer)
            {
                SetHealth(Mathf.Min(m_health, newJobStat.MaxHealth), DamageReason.MANUAL);
            }

            m_weaponManager.LoadWeapons(newJobStat.Weapons);
            m_characterRenderer.LoadAnimation(m_team);
        }
        
        public void OnChangeTeam(Team newTeam)
        {
            m_team = newTeam;

            m_characterRenderer.LoadAnimation(m_team);

            Die(DamageReason.MANUAL);
        }

        public void LoadSetting(PlayerSetting setting)
        {
            m_owner = setting.playerID;
            m_ropableController = new RopableController(this, new RopableID(ObjectType.PLAYER, setting.playerID));

            m_team = setting.team;
        }

        void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo mnInfo)
        {
            m_inputProcessor.SyncInput(ref stream);
        }

        void FixedUpdate()
        {
            if (!Network.isServer) return;

            if (IsDead())
            {
                UpdateRevivalTimer(Time.deltaTime);

                if (CanRevive())
                {
                    Revive();
                    
                }
            }

            if (IsDead() == false && Map.Inst.CheckInBorder(this) == false)
            {
                Die(DamageReason.MANUAL);
            }
        }

        [RPC]
        public void Jump(NetworkMessageInfo info)
        {
            if(!PlayerManager.Inst.IsValidPlayer(m_owner, info.sender)) return;
            if (m_stateManager.IsInState(CharacterState.ROPING))
            {
                m_ropeController.CutRope();
            }
        }

        void Update()
        {
            //Animation Rendering.
            //Every client must do his own Animation Rendering.
            m_characterRenderer.Render();
            //Client Input Processing
            m_inputProcessor.ProcessInput();

            // Server Rendering. Rendering from input must be in Update()
            // Fixed Update refreshes in fixed period. 
            // When Update() Period is shorter than fixed period, Update() can be called multiple times between FixedUpdate().
            // Because Input Data is reset in Update(), FixedUpdate() Can't get input data properly.
            m_controller.ProcessCharacter();
        }
        
        public bool ProcessInputForBuild()
        {
            return m_buildController.ProcessInput();
        }

        public void Damage(int damage, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;

            if (damage == 0) return;

            if (IsDead())
                return;

            SetHealth(m_health - damage, DamageReason.DAMAGE);
        }

        void BroadcastHealth(int health, int delta, DamageReason reason)
        {
            S2C.SetCharacterHealth pck = new S2C.SetCharacterHealth(health, delta, reason);

            networkView.RPC("RecvHealth", RPCMode.Others, pck.SerializeToBytes());
        }
        
        [RPC]
        void RecvHealth(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck
            S2C.SetCharacterHealth pck = S2C.SetCharacterHealth.DeserializeFromBytes(pckData);

            SetHealth(pck.m_health, pck.m_reason);
        }

        void SetHealth(int health, DamageReason reason)
        {
            int original = m_health;
            m_health = health;

            if (m_health <= 0)
            {
                m_health = 0;
            }

            if (Network.isServer)
                BroadcastHealth(m_health, m_health - original, reason);

            if (m_health == 0)
                Die(reason);

            if (Network.isClient || !ServerGame.Inst.isDedicatedServer) //rendering
            {
                switch(reason)
                {
                    case DamageReason.DAMAGE:
                        m_characterRenderer.Highlight(Const.Effect.CharacterHighlight.DAMAGE);
                        break;

                    default:
                        break;
                }
            }
        }

        void Die(DamageReason reason)
        {
            if (m_stateManager.IsInState(CharacterState.DEAD)) return;

            m_health = 0;
            revivalTimer = REVIVAL_TIME;

            if (Network.isServer)
                m_stateManager.SetState(CharacterState.DEAD);
        }
        
        public bool IsDead()
        {
            return m_stateManager.IsInState(CharacterState.DEAD);
        }

        void UpdateRevivalTimer(double deltaTime)
        {
            revivalTimer -= deltaTime;
        }

        bool CanRevive()
        {
            return m_jobStat != null && revivalTimer <= 0;
        }

        void BroadcastRevive()
        {
            if (!ServerGame.Inst.isDedicatedServer)
                networkView.RPC("RecvRevive", RPCMode.All);
            else
                networkView.RPC("RecvRevive", RPCMode.Others);
        }

        [RPC]
        void RecvRevive(NetworkMessageInfo info)
        {
            //ServerCheck

            m_stateManager.SetState(CharacterState.FALLING);

            if (m_jobStat != null)
                m_health = m_jobStat.MaxHealth;
        }

        void Revive()
        {
            if (!Network.isServer) return;

            transform.position = server.RevivalLocation;
            rigidbody2D.velocity = Vector2.zero;

            m_stateManager.SetState(CharacterState.FALLING);

            m_weaponManager.ReloadAll();
            m_health = m_jobStat.MaxHealth;

            m_ropableController.CutRopeAll();
            m_ropeController.CutRope();

            BroadcastRevive();
        }

        public void RemoveCharacterFromNetwork()
        {
            m_ropeController.CutRope();

            Network.RemoveRPCs(transformView.viewID);
            Network.RemoveRPCs(controllerView.viewID);
            Network.Destroy(gameObject);
        }

        public void ToAirMaterial()
        {
            bodyCollider.enabled = false;
            bodyCollider.sharedMaterial = onAirMaterial;
            bodyCollider.enabled = true;
        }

        public void ToGroundMaterial()
        {
            bodyCollider.enabled = false;
            bodyCollider.sharedMaterial = bodyMaterial;
            bodyCollider.enabled = true;
        }

        //Weapon
        public void TryFire()
        {
            m_weaponManager.TryFire();
        }

        public void TryFire(WeaponType type)
        {
            m_weaponManager.TryFire(type);
        }
        public void TryFireCharged()
        {
            m_weaponManager.TryFireCharged();
        }


        public void ChangeWeapon(KeyCode code)
        {
            m_weaponManager.ChangeWeapon(code);
        }


        // Build
        public void TryBuild(BuildingData building, Vector2 location)
        {
            C2S.Build pck = new C2S.Build(building.type, location);

            if (Network.isServer)
                Build(pck.SerializeToBytes(), new NetworkMessageInfo());
            else
                networkView.RPC("Build", RPCMode.Server, pck.SerializeToBytes());
        }
        [RPC]
        public void Build(byte[] pckData, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;

            C2S.Build pck = C2S.Build.DeserializeFromBytes(pckData);

            m_buildController.Build(pck);
        }

        public void SelectBuildTool(BuildingData bData)
        {
            m_buildController.SelectBuildTool(bData);
        }
       

        // Ropes
        RopableController m_ropableController;
       
        public RopableID GetRopableID()
        {
            return m_ropableController.GetRopableID();
        }

        public void CutInfectingRope(Rope rope)
        {
            m_ropableController.CutInfectingRope(rope);
        }

        public void Roped(Rope newRope, Vector2 position)
        {
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position, 5);

            newRope.MakeHingeJoint(rigidbody2D, position, transform.InverseTransformPoint(position));

            m_ropableController.Roped(newRope);
        }

        public void CutRopeAll()
        {
            m_ropableController.CutRopeAll();
        }

        RopeController m_ropeController;

        public void StopRoping()
        {
            m_ropeController.StopRoping();
        }
        public void RopeFired(Rope newRope)
        {
            m_ropeController.RopeFired(newRope);
        }
        public void OnFireRope(Rope newRope)
        {
            m_ropeController.OnFireRope(newRope);
        }
        public void ModifyRopeLength(float modifier)
        {
            m_ropeController.ModifyRopeLength(modifier);
        }
        public Vector2 GetRopeDirection()
        {
            return m_ropeController.GetRopeDirection();
        }

    }

    public class CharacterSM : StateManager<CharacterState>
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

            Debug.Log(newState);
        }
    }

    public class CharacterEM : EnvManager<CharacterEnv>
    { }


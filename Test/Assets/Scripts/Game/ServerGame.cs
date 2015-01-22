using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using C2S = Packet.C2S;
using S2C = Packet.S2C;
using FocusManager;
using InGameMenu;
using Maps;

    [RequireComponent(typeof(ParticleManager), typeof(PlayerManager), typeof(ProjectileManager))]
    class ServerGame : MonoBehaviour
    {
        private static ServerGame instance;

        public static ServerGame Inst
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServerGame();
                }
                return instance;
            }
        }

        public Vector3 spawnPosition;
        public GameObject playerPrefab;

        public Vector2 RevivalLocation;

        public bool isDedicatedServer = false;

        public KeyFocusManager keyFocusManager;
        public MouseFocusManager mouseFocusManager;

        public TeamSelector teamSelector;
        public JobSelector jobSelector;
        public NameSelector nameSelector;
        public BuildMenu buildMenu;
        public ScoreBoard scoreBoard;

        CTeam[] m_teams;

        public CTeam GetTeam(Team team)
        {
            return m_teams[(int)team];
        }

        void Awake()
        {
            instance = this;
            m_teams = new CTeam[2] { new CTeam(), new CTeam() };

            keyFocusManager = new KeyFocusManager(InputKeyFocus.PLAYER);
            mouseFocusManager = new MouseFocusManager(InputMouseFocus.PLAYER);

            int projectileLayer = LayerMask.NameToLayer("Projectile");
            Physics2D.IgnoreLayerCollision(projectileLayer, projectileLayer);
            Physics2D.IgnoreLayerCollision(projectileLayer, LayerMask.NameToLayer("Particle"));
        }

        void Start()
        {
            RevivalLocation = new Vector2(0f, 3f);
        }

        void OnServerInitialized()
        {
            Debug.Log("Server Initialized!");

            StartServerGame();
        }

        public void StartServerGame()
        {
            RecvClearGame();

            Map.Inst.Load();

            int ID = PlayerManager.Inst.SetID(PlayerManager.Inst.GetUniqueID(), Network.player);
            SetPlayerID(ID, new NetworkMessageInfo());
        }

        void OnPlayerConnected(NetworkPlayer player)
        {
            if (PlayerManager.Inst.Exists(player))
            {
                //Clear previous connection;
                return;
            }

            int ID = PlayerManager.Inst.SetID(PlayerManager.Inst.GetUniqueID(), player);
            networkView.RPC("SetPlayerID", player, ID);
        }

        [RPC]
        public void ServerSelectTeam(byte[] updateData, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;

            C2S.UpdatePlayerTeam update = C2S.UpdatePlayerTeam.DeserializeFromBytes(updateData);
            if (!PlayerManager.Inst.IsValidPlayer(update.playerID, info.sender)) return;

            //TODO::TeamSelect Validation (team balance)

            OnRecvPlayerTeam(update);
            
            BroadcastPlayerTeam(updateData);

            ServerPlayer player = PlayerManager.Inst.Get(update.playerID);
            if (player == null)
            {
                ReadyToEnterCharacter(PlayerManager.Inst.GetSetting(update.playerID));
            }
        }

        void BroadcastPlayerTeam(byte[] updateData)
        {
            networkView.RPC("RecvPlayerTeam", RPCMode.Others, updateData);
        }

        [RPC]
        void RecvPlayerTeam(byte[] updateData, NetworkMessageInfo info)
        {
            //ServerCheck

            C2S.UpdatePlayerTeam update = C2S.UpdatePlayerTeam.DeserializeFromBytes(updateData);

            OnRecvPlayerTeam(update);
        }

        void OnRecvPlayerTeam(C2S.UpdatePlayerTeam update)
        {
            PlayerManager.Inst.UpdatePlayer(update);

            ServerPlayer player = PlayerManager.Inst.Get(update.playerID);
            if (player != null)
            {
                AddPlayerToTeam(player, update.team);
                player.OnChangeTeam(update.team);
            }
        }

        public void AddPlayerToTeam(ServerPlayer player, Team team)
        {
            if (player.GetTeam() != Team.NONE)
                GetTeam(player.GetTeam()).RemovePlayer(player);

            GetTeam(team).AddPlayer(player);
        }

        [RPC]
        public void ServerSetPlayerName(byte[] updateData, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;

            C2S.UpdatePlayerName update = C2S.UpdatePlayerName.DeserializeFromBytes(updateData);
            if (!PlayerManager.Inst.IsValidPlayer(update.playerID, info.sender)) return;

            PlayerManager.Inst.UpdatePlayer(update);


            if (!isDedicatedServer)
                networkView.RPC("RecvSetPlayerName", RPCMode.All, update.SerializeToBytes());
            else
                networkView.RPC("RecvSetPlayerName", RPCMode.Others, update.SerializeToBytes());

            ReadyToEnterCharacter(PlayerManager.Inst.GetSetting(update.playerID));
        }

        [RPC]
        void RecvSetPlayerName(byte[] updateData)
        {
            //ServerCheck

            C2S.UpdatePlayerName update = C2S.UpdatePlayerName.DeserializeFromBytes(updateData);

            PlayerManager.Inst.UpdatePlayer(update);
        }

        void ReadyToEnterCharacter(PlayerSetting setting)
        {
            if (!Network.isServer) return;

            PlayerSettingError error = setting.IsSettingCompleted();
            if (error == PlayerSettingError.NONE)
            {
                EnterCharacter(setting);
                return;
            }

            S2C.PlayerNotReady notReady = new S2C.PlayerNotReady(error);
            NetworkPlayer player = PlayerManager.Inst.GetPlayer(setting.playerID);

            if (player == Network.player) // ServerPlayer
                PlayerNotReadyToEnter(notReady.SerializeToBytes(), new NetworkMessageInfo());
            else
                networkView.RPC("PlayerNotReadyToEnter", player, notReady.SerializeToBytes());
        }

        [RPC]
        public void PlayerNotReadyToEnter(byte[] notReadyData, NetworkMessageInfo info)
        {
            S2C.PlayerNotReady notReady = S2C.PlayerNotReady.DeserializeFromBytes(notReadyData);

            switch (notReady.error)
            {
                case PlayerSettingError.NAME:
                    nameSelector.Open();
                    break;

                case PlayerSettingError.TEAM:
                    teamSelector.Open();
                    break;
            }

        }
    

        public void EnterCharacter(PlayerSetting setting)
        {
            if (!Network.isServer) return;

            Debug.Log(String.Format("Player Ready {0}", setting.playerID));

            GameObject newPlayer = (GameObject)Network.Instantiate(playerPrefab, spawnPosition, Quaternion.identity, 1);
            ServerPlayer serverPlayer = newPlayer.GetComponent<ServerPlayer>();
            
            PlayerManager.Inst.Set(setting.playerID, serverPlayer);
            serverPlayer.Init(setting);
        }

        /// <summary>
        /// Player Connection, Disconnection
        /// </summary>
        /// <param name="player"></param>

        void OnPlayerDisconnected(NetworkPlayer player)
        {
            Debug.Log(String.Format("Player Disconnected! {0}", player));

            int playerID = PlayerManager.Inst.GetID(player);
            ServerPlayer character = PlayerManager.Inst.Get(playerID);

            if (character != null)
                character.RemoveCharacterFromNetwork();

            networkView.RPC("RecvPlayerRemove", RPCMode.Others, playerID);
        }

        [RPC]
        void RecvPlayerRemove(int player)
        {
            PlayerManager.Inst.Remove(player);
            PlayerManager.Inst.RemoveSetting(player);
        }

        public void ClearGame()
        {
            networkView.RPC("RecvClearGame", RPCMode.All);
        }

        [RPC]
        public void RecvClearGame()
        {
            PlayerManager.Inst.Clear();
            ProjectileManager.Inst.Clear();
            Map.Inst.Clear();
        }

        void OnDisconnectedFromServer(NetworkDisconnection info)
        {
            OnServerDown();
        }

        public void OnServerDown()
        {
            PlayerManager.Inst.Clear();
            ProjectileManager.Inst.Clear();

            Map.Inst.Clear();
        }

        public void OpenInGameMenu(InGameMenuType menu)
        {
            switch(menu)
            {
                case InGameMenuType.BUILD_MENU:
                    buildMenu.Open();
                    break;

                case InGameMenuType.JOB_SELECTOR:
                    jobSelector.Open();
                    break;

                case InGameMenuType.NAME_SELECTOR:
                    nameSelector.Open();
                    break;

                case InGameMenuType.TEAM_SELECTOR:
                    teamSelector.Open();
                    break;

                case InGameMenuType.SCORE_BOARD:
                    scoreBoard.Open();
                    break;

                default:
                    break;
            }
        }

        private int m_ID = -1;

        void SetMyID(int newID)
        {
            m_ID = newID;
        }

        public int GetID()
        {
            return m_ID;
        }

        [RPC]
        public void SetPlayerID(int newID, NetworkMessageInfo info)
        {
            //ServerCheck

            SetMyID(newID);
            nameSelector.Open();
        }

        public void SelectTeam(Team team)
        {
            C2S.UpdatePlayerTeam selectTeam = new C2S.UpdatePlayerTeam(m_ID, team);

            if (Network.isServer)
                ServerSelectTeam(selectTeam.SerializeToBytes(), new NetworkMessageInfo());
            else if (Network.isClient)
                networkView.RPC("ServerSelectTeam", RPCMode.Server, selectTeam.SerializeToBytes());
        }

        public void SetPlayerName(string name)
        {
            C2S.UpdatePlayerName pck = new C2S.UpdatePlayerName(m_ID, name);

            if (Network.isServer)
            {
                ServerSetPlayerName(pck.SerializeToBytes(), new NetworkMessageInfo());
            }
            else if (Network.isClient)
            {
                networkView.RPC("ServerSetPlayerName", RPCMode.Server, pck.SerializeToBytes());
            }
        }


        bool mapLoaded;
        bool playerLoaded;

        public void OnPlayerLoadCompleted()
        {
            if (!Network.isClient) return;

            playerLoaded = true;

            if (IsPlayerMapLoaded())
            {
                foreach (var obj in FindObjectsOfType<Projectile>())
                {
                    obj.SendMessage("OnPlayerMapLoaded", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void OnMapLoadCompleted(Map map)
        {
            if (!Network.isClient) return;

            mapLoaded = true;

            if (IsPlayerMapLoaded())
            {
                foreach (var obj in FindObjectsOfType<Projectile>())
                {
                    obj.SendMessage("OnPlayerMapLoaded", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public bool IsPlayerMapLoaded()
        {
            return mapLoaded && playerLoaded;
        }


    }

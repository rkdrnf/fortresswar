using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using C2S = Communication.C2S;
using S2C = Communication.S2C;
using FocusManager;
using InGameMenu;
using Maps;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using Communication;

    [RequireComponent(typeof(PlayerManager), typeof(ProjectileManager))]
    class ServerGame : NetworkBehaviour
    {
        private static ServerGame instance;

        public static ServerGame Inst
        {
            get
            {
                return instance;
            }
        }

        public Vector3 spawnPosition;
        public ServerPlayer playerPrefab;

        public Vector2 RevivalLocation;

        public bool isDedicatedServer = false;

        public KeyFocusManager keyFocusManager;
        public MouseFocusManager mouseFocusManager;

        public Map mapPrefab;
        public MapData mapData;

        Map m_currentMap;
        public Map CurrentMap
        {
            get { return m_currentMap; }
            set { m_currentMap = value;  }
        }
        
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
            Debug.LogError("[ServerGame] Awake");
            instance = this;
            m_teams = new CTeam[2] { new CTeam(), new CTeam() };
            RevivalLocation = new Vector2(0f, 3f);

            keyFocusManager = new KeyFocusManager(InputKeyFocus.PLAYER);
            mouseFocusManager = new MouseFocusManager(InputMouseFocus.PLAYER);

            int projectileLayer = LayerMask.NameToLayer("Projectile");
            Physics2D.IgnoreLayerCollision(projectileLayer, projectileLayer);
            Physics2D.IgnoreLayerCollision(projectileLayer, LayerMask.NameToLayer("Particle"));
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            CreateMap();

            Debug.LogError("OnSTartServer");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.LogError("[ServerGame] Client Instantiated");
        }

        void Start()
        {
            Debug.LogError("[ServerGame] Start");

            if (isClient)
            { 
                C2S.AddNewPlayer addPlayer = new C2S.AddNewPlayer();
                GameNetworkManager.Inst.client.Send((short)PacketType.AddNewPlayer, addPlayer);
            }
        }

        public void OnNewPlayerJoin(NetworkConnection conn)
        {
            if (PlayerManager.Inst.Exists(conn))
            {
                //Clear previous connection;
                return;
            }

            int ID = PlayerManager.Inst.SetID(PlayerManager.Inst.GetUniqueID(), conn);

            NetworkServer.SendToClient(conn.connectionId, (short)PacketType.SetPlayerID, new IntegerMessage(ID));
        }
        

        public void Run()
        {
            Debug.Log("ServerGame Started!");
        }
        
        void CreateMap()
        {
            Map newMap = (Map)Instantiate(mapPrefab, transform.position, transform.rotation);

            //NetworkHash128 netHash = newMap.GetComponent<NetworkIdentity>().assetId;
            //ClientScene.RegisterSpawnHandler(netHash, Map.SpawnHandler, Map.UnspawnHandler);

            NetworkServer.Spawn(newMap.gameObject);

            if (!newMap.Load(mapData))
            {
                Debug.LogError("Failed to Load Map data");
            }

        }

        

        public void UpdatePlayerTeamRequest(C2S.UpdatePlayerTeam pck)
        {
            if (!isServer) return;

            S2C.BroadcastPlayerTeam broadcastTeam = new S2C.BroadcastPlayerTeam(pck.playerID, pck.team);
            NetworkServer.SendToAll((short)PacketType.BroadcastPlayerTeam, broadcastTeam);
        }

        public void ReceivePlayerTeam(S2C.BroadcastPlayerTeam pck)
        {
            UpdatePlayerTeam(pck.playerID, pck.team);

            if (isServer)
            { 
                ServerPlayer player = PlayerManager.Inst.Get(pck.playerID);
                if (player == null)
                {
                    TryEnterCharacter(pck.playerID);
                }
            }
        }

        void UpdatePlayerTeam(int playerID, Team team)
        {
            PlayerManager.Inst.UpdatePlayerTeam(playerID, team);

            ServerPlayer player = PlayerManager.Inst.Get(playerID);
            if (player != null)
            {
                AddPlayerToTeam(player, team);
                player.OnChangeTeam(team);
            }
        }

        public void AddPlayerToTeam(ServerPlayer player, Team team)
        {
            if (player.GetTeam() != Team.NONE)
                GetTeam(player.GetTeam()).RemovePlayer(player);

            GetTeam(team).AddPlayer(player);
        }

        public void UpdatePlayerNameRequest(C2S.UpdatePlayerName pck)
        {
            if (!isServer) return;

            S2C.BroadcastPlayerName broadcastPck = new S2C.BroadcastPlayerName(pck.playerID, pck.name);
            NetworkServer.SendToAll((short)PacketType.BroadcastPlayerName, broadcastPck);
        }

        public void ReceivePlayerName(S2C.BroadcastPlayerName pck)
        {
            PlayerManager.Inst.UpdatePlayerName(pck.playerID, pck.name);

            if (isServer)
            {
                TryEnterCharacter(pck.playerID);
            }
        }

        void TryEnterCharacter(int playerID)
        {
            if (!isServer) return;

            PlayerSetting setting = PlayerManager.Inst.GetSetting(playerID);

            if (setting == null) return;

            PlayerSettingError error = setting.IsSettingCompleted();
            if (error == PlayerSettingError.NONE)
            {
                OnCharacterPrepared(playerID);
                return;
            }

            S2C.PlayerNotReady notReady = new S2C.PlayerNotReady(error);
            NetworkConnection playerCon = PlayerManager.Inst.GetConnection(setting.playerID);

            NetworkServer.SendToClient(playerCon.connectionId, (short)PacketType.PlayerNotReady, notReady);
        }

        public void PreparePlayer(S2C.PlayerNotReady pck)
        {
            switch (pck.error)
            {
                case PlayerSettingError.NAME:
                    nameSelector.Open();
                    break;

                case PlayerSettingError.TEAM:
                    teamSelector.Open();
                    break;
            }
        }

        public void OnCharacterPrepared(int playerID)
        {
            if (!isServer) return;

            Debug.Log(String.Format("Player Ready {0}", playerID));

            S2C.PlayerPrepared prepared = new S2C.PlayerPrepared();

            NetworkConnection conn = PlayerManager.Inst.GetConnection(playerID);
            NetworkServer.SendToClient(conn.connectionId, (short)PacketType.PlayerPrepared, prepared);
        }

        public void AddPlayerRequest()
        {
            Debug.Log("[Client] AddPlayerRequest. ClientScene.AddPlayer");
            ClientScene.AddPlayer(0);
        }

        public void AddPlayerCharacter(NetworkConnection conn, short playerControllerID)
        {
            int playerID = PlayerManager.Inst.GetID(conn);
            if (playerID == -1)
            {
                Debug.LogError(string.Format("Player ID doesn't exist for con {0}", conn.ToString()));
                return;
            }

            PlayerSetting setting = PlayerManager.Inst.GetSetting(playerID);
            if (setting == null)
            {
                Debug.LogError(string.Format("Player Setting doesn't exist for ID {0}", playerID));
                return;
            }


            ServerPlayer newPlayer = (ServerPlayer)Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

            PlayerManager.Inst.Set(setting.playerID, newPlayer);
            newPlayer.Init(setting);

            NetworkServer.AddPlayerForConnection(conn, newPlayer.gameObject, playerControllerID);
        }

        /// <summary>
        /// Player Connection, Disconnection
        /// </summary>
        /// <param name="player"></param>
        /// 

        public void OnGamePlayerDisconnected(NetworkConnection conn)
        {
            Debug.Log(String.Format("Player Disconnected! {0}", conn.address));

            int playerID = PlayerManager.Inst.GetID(conn);
            ServerPlayer character = PlayerManager.Inst.Get(playerID);

            if (character != null)
                character.RemoveCharacterFromNetwork();

            RpcRecvPlayerRemove(playerID);
        }

        [ClientRpc]
        void RpcRecvPlayerRemove(int player)
        {
            PlayerManager.Inst.Remove(player);
            PlayerManager.Inst.RemoveSetting(player);
        }

        public void ClearGame()
        {
            GetComponent<NetworkView>().RPC("RecvClearGame", RPCMode.All);
        }

        [RPC]
        public void RecvClearGame()
        {
            PlayerManager.Inst.Clear();
            ProjectileManager.Inst.Clear();
            if (m_currentMap != null)
            {
                m_currentMap.Clear();
            }
        }

        void OnDisconnectedFromServer(NetworkDisconnection info)
        {
            OnServerDown();
        }

        public void OnServerDown()
        {
            PlayerManager.Inst.Clear();
            ProjectileManager.Inst.Clear();

            if (m_currentMap != null)
            {
                m_currentMap.Clear();
            }
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

        public void SetPlayerID(int newID)
        {
            SetMyID(newID);
            nameSelector.Open();
        }

        public void SelectTeam(Team team)
        {
            C2S.UpdatePlayerTeam selectTeam = new C2S.UpdatePlayerTeam(m_ID, team);
            GameNetworkManager.Inst.client.Send((short)PacketType.UpdatePlayerTeam, selectTeam);
        }

        public void UpdatePlayerName(string name)
        {
            C2S.UpdatePlayerName pck = new C2S.UpdatePlayerName(m_ID, name);
            GameNetworkManager.Inst.client.Send((short)PacketType.UpdatePlayerName, pck);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using C2S = Packet.C2S;
using S2C = Packet.S2C;

namespace Server
{
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


        Client.ClientGame myClient
        {
            get { return Client.ClientGame.Inst; }
        }

        public Vector3 spawnPosition;
        public GameObject playerPrefab;

        public Vector2 RevivalLocation;

        public bool isDedicatedServer = false;

        void Awake()
        {
            instance = this;
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
            myClient.ClientClearGame();

            Game.Inst.LoadMap();

            int ID = PlayerManager.Inst.SetID(PlayerManager.Inst.GetUniqueID(), Network.player);
            myClient.SetPlayerID(ID, new NetworkMessageInfo());
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

            ServerPlayer player = PlayerManager.Inst.Get(update.playerID);

            PlayerManager.Inst.UpdatePlayer(update);

            if (player != null) 
                player.BroadcastDie(); // Player Exists.
            else    
                ReadyToEnterCharacter(PlayerManager.Inst.GetSetting(update.playerID));



            if (info.sender == Network.player)
                networkView.RPC("SetPlayerTeam", RPCMode.All, updateData);
            else
                networkView.RPC("SetPlayerTeam", RPCMode.Others, updateData);
        }



        [RPC]
        public void ServerSetPlayerName(byte[] updateData, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;

            C2S.UpdatePlayerName update = C2S.UpdatePlayerName.DeserializeFromBytes(updateData);
            if (!PlayerManager.Inst.IsValidPlayer(update.playerID, info.sender)) return;

            PlayerManager.Inst.UpdatePlayer(update);


            if (info.sender == Network.player)
                networkView.RPC("ClientSetPlayerName", RPCMode.All, update.SerializeToBytes());
            else
                networkView.RPC("ClientSetPlayerName", RPCMode.Others, update.SerializeToBytes());

            ReadyToEnterCharacter(PlayerManager.Inst.GetSetting(update.playerID));
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
                myClient.PlayerNotReadyToEnter(notReady.SerializeToBytes(), new NetworkMessageInfo());
            else
                networkView.RPC("PlayerNotReadyToEnter", player, notReady.SerializeToBytes());
        }

        public void EnterCharacter(PlayerSetting setting)
        {
            if (!Network.isServer) return;

            Debug.Log(String.Format("Player Ready {0}", setting.playerID));

            GameObject newPlayer = (GameObject)Network.Instantiate(playerPrefab, spawnPosition, Quaternion.identity, 0);
            ServerPlayer serverPlayer = newPlayer.GetComponent<ServerPlayer>();

            serverPlayer.Init(setting.playerID);

            if (setting.playerID == myClient.GetID()) // own player
            {
                serverPlayer.isServerPlayer = true;
                newPlayer.networkView.RPC("SetOwner", RPCMode.AllBuffered, setting.playerID);
            }
            else
            { 
                newPlayer.networkView.RPC("SetOwner", RPCMode.OthersBuffered, setting.playerID);
            }
        }

        [RPC]
        void PlayerListRequest(int requestorID, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;
            if (!PlayerManager.Inst.IsValidPlayer(requestorID, info.sender)) return;

            S2C.PlayerList settings = new S2C.PlayerList(PlayerManager.Inst.GetSettings());

            byte[] settingsData = settings.SerializeToBytes();

            networkView.RPC("SetPlayerList", info.sender, settingsData);
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

            character.RemoveCharacterFromNetwork();

            networkView.RPC("OnPlayerRemoved", RPCMode.All, playerID);
        }

        public void ClearGame()
        {
            networkView.RPC("ClientClearGame", RPCMode.All);
        }

        void OnDisconnectedFromServer(NetworkDisconnection info)
        {
            if (!Network.isServer) return;

            OnServerDown();

            if (!isDedicatedServer)
                myClient.OnServerDown();
        }

        void OnServerDown()
        {
            PlayerManager.Inst.Clear();
            ProjectileManager.Inst.Clear();

            Game.Inst.ClearMap();
        }
    }
}

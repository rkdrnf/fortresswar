using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;
using C2S = Packet.C2S;
using S2C = Packet.S2C;
using UnityEngine;
using FocusManager;


namespace Client
{
    class ClientGame : MonoBehaviour
    {
        private static ClientGame instance;
    
        public static ClientGame Inst
        {
            get
            {
                if (instance == null)
                {
                    instance = new ClientGame();
                }
                return instance;
            }
        }
    
        private int ID = -1;
    
        Server.ServerGame myServer 
        {
            get { return Server.ServerGame.Inst; }
        }
    
        public TeamSelector teamSelector;
        public JobSelector jobSelector;
        public NameSelector nameSelector;
    
        public KeyFocusManager keyFocusManager;
        public MouseFocusManager mouseFocusManager;
    
        void Awake()
        {
            instance = this;
        }
    
        void Start()
        {
            keyFocusManager = new KeyFocusManager(InputKeyFocus.PLAYER);
            mouseFocusManager = new MouseFocusManager(InputMouseFocus.PLAYER);
    
            
        }
    
        /// <summary>
        /// SET PLAYER ID
        /// </summary>
        /// <param name="newID"></param>
        /// <param name="info"></param>
        /// 
        void SetMyID(int newID)
        {
            ID = newID;
        }
    
        public int GetID()
        {
            return ID;
        }
        [RPC]
        public void SetPlayerID(int newID, NetworkMessageInfo info)
        {
            //ServerCheck
    
            
            SetMyID(newID);
    
            if (Network.isClient)
            {
                networkView.RPC("PlayerListRequest", RPCMode.Server, ID);
            }
    
            nameSelector.Open();
        }
    
        [RPC]
        void SetPlayerList(byte[] playersData, NetworkMessageInfo info)
        {
            if (!Network.isClient)
                return;
    
            List<PlayerSetting> settings = S2C.PlayerList.DeserializeFromBytes(playersData).settings;
    
            foreach (PlayerSetting setting in settings)
            {
                C_PlayerManager.Inst.SetSetting(setting);
                PlayerBehaviour character = C_PlayerManager.Inst.Get(setting.playerID);
                character.OnSettingChange();
            }
        }
    
        /// <summary>
        /// SELECT TEAM
        /// </summary>
        /// <param name="team"></param>
        public void SelectTeam(Team team)
        {
            C2S.UpdatePlayerTeam selectTeam = new C2S.UpdatePlayerTeam(ID, team);
    
            if (Network.isServer)
                myServer.ServerSelectTeam(selectTeam.SerializeToBytes(), new NetworkMessageInfo());
            else if (Network.isClient)
                networkView.RPC("ServerSelectTeam", RPCMode.Server, selectTeam.SerializeToBytes());
        }
    
        [RPC]
        void SetPlayerTeam(byte[] updateData, NetworkMessageInfo info)
        {
            //ServerCheck
    
            C2S.UpdatePlayerTeam update = C2S.UpdatePlayerTeam.DeserializeFromBytes(updateData);

            C_PlayerManager.Inst.UpdatePlayer(update);

            PlayerBehaviour player = C_PlayerManager.Inst.Get(update.playerID);
            if (player != null)
            {
                player.OnChangeTeam(update.team);
            }
        }
    
        /// <summary>
        /// SET PLAYER NAME
        /// </summary>
        /// <param name="updateData"></param>
        /// 
    
        public void SetPlayerName(string name)
        {
            C2S.UpdatePlayerName pck = new C2S.UpdatePlayerName(ID, name);
    
            if (Network.isServer)
            {
                myServer.ServerSetPlayerName(pck.SerializeToBytes(), new NetworkMessageInfo());
            }
            else if (Network.isClient)
            {
                ClientGame.Inst.networkView.RPC("ServerSetPlayerName", RPCMode.Server, pck.SerializeToBytes());
            }
        }
    
        [RPC]
        void ClientSetPlayerName(byte[] updateData)
        {
            //ServerCheck
    
            C2S.UpdatePlayerName update = C2S.UpdatePlayerName.DeserializeFromBytes(updateData);
    
            C_PlayerManager.Inst.UpdatePlayer(update);
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
    
    
        [RPC]
        void OnPlayerRemoved(int player)
        {
            C_PlayerManager.Inst.Remove(player);
            C_PlayerManager.Inst.RemoveSetting(player);
        }
    
        [RPC]
        public void ClientClearGame()
        {
            C_PlayerManager.Inst.Clear();
            ProjectileManager.Inst.Clear();
            Game.Inst.ClearMap();
        }

        void OnDisconnectedFromServer(NetworkDisconnection info)
        {
            if (!Network.isClient) return;
                
            OnServerDown();
        }

        public void OnServerDown()
        {
            C_PlayerManager.Inst.Clear();
            ProjectileManager.Inst.Clear();

            ClearMap();
        }

        public void ClearMap()
        {
            Game.Inst.ClearMap();
        }
    }
}

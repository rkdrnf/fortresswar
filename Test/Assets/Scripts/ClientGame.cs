using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;
using C2S = Packet.C2S;
using S2C = Packet.S2C;
using UnityEngine;
using FocusManager;

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

    ServerGame myServer 
    {
        get { return ServerGame.Inst; }
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
    void PlayerListRequest(int requestorID, NetworkMessageInfo info)
    {
        if (!Network.isServer) return;
        if (!PlayerManager.Inst.IsValidPlayer(requestorID, info.sender)) return;

        S2C.PlayerList settings = new S2C.PlayerList(PlayerManager.Inst.GetSettings());

        byte[] settingsData = settings.SerializeToBytes();

        networkView.RPC("SetPlayerList", info.sender, settingsData);
    }

    [RPC]
    void SetPlayerList(byte[] playersData, NetworkMessageInfo info)
    {
        if (!Network.isClient)
            return;

        List<PlayerSetting> settings = S2C.PlayerList.DeserializeFromBytes(playersData).settings;

        foreach (PlayerSetting setting in settings)
        {
            PlayerManager.Inst.SetSetting(setting);
            PlayerBehaviour character = PlayerManager.Inst.Get(setting.playerID);
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
        if (!Network.isClient) return;
        //ServerCheck

        C2S.UpdatePlayerTeam update = C2S.UpdatePlayerTeam.DeserializeFromBytes(updateData);

        OnSelectTeam(update);
    }

    public void OnSelectTeam(C2S.UpdatePlayerTeam update)
    {
        PlayerManager.Inst.UpdatePlayer(update);

        PlayerBehaviour player = PlayerManager.Inst.Get(update.playerID);
        if (player != null)
        {
            player.ChangeTeam(update);
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
        if (!Network.isClient) return;
        //ServerCheck

        C2S.UpdatePlayerName update = C2S.UpdatePlayerName.DeserializeFromBytes(updateData);

        PlayerManager.Inst.UpdatePlayer(update);
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
        PlayerManager.Inst.Remove(player);
        PlayerManager.Inst.RemoveSetting(player);
    }

    [RPC]
    public void ClientClearGame()
    {
        PlayerManager.Inst.Clear();
        ProjectileManager.Inst.Clear();
        Game.Inst.ClearMap();
    }
}

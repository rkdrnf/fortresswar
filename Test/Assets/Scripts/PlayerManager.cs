using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading;
using Const;
using S2C = Packet.S2C;
using C2S = Packet.C2S;

public class PlayerManager : MonoBehaviour
{
    private PlayerObjManager playerObjManager;

    private Dictionary<int, PlayerSetting> settingDic;

    private Dictionary<int, NetworkPlayer> playerIDDic;

    int idCount;


    private static PlayerManager instance;

    public static PlayerManager Inst
    {
        get
        {
            if (instance == null)
            {
                instance = new PlayerManager();
            }
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
        playerObjManager = new PlayerObjManager();
        settingDic = new Dictionary<int, PlayerSetting>();
        playerIDDic = new Dictionary<int, NetworkPlayer>();
    }

    

    // Dictionary Functions
    public PlayerBehaviour Get(int player)
    {
        return playerObjManager.Get(player);
    }

    public void Set(int player, PlayerBehaviour character)
    {
        playerObjManager.Set(player, character);
    }

    public void Remove(int player)
    {
        playerObjManager.Remove(player);
    }

    public bool Exists(NetworkPlayer player)
    {
        foreach(var data in playerIDDic)
        {
            if (data.Value == player)
                return true;
        }
        return false;
    }

    public void Clear()
    {
        playerObjManager.Clear();
    }

    public PlayerSetting GetSetting(int playerID)
    {
        if (settingDic.ContainsKey(playerID))
            return settingDic[playerID];

        return null;
    }

    public void RemoveSetting(int playerID)
    {
        settingDic.Remove(playerID);
    }

    public void UpdatePlayer(C2S.UpdatePlayerName update)
    {
        if (settingDic.ContainsKey(update.playerID) == false)
        {
            PlayerSetting setting = new PlayerSetting(update.playerID);
            settingDic[update.playerID] = setting;
        }

        settingDic[update.playerID].name = update.name;
    }

    public void UpdatePlayer(C2S.UpdatePlayerTeam update)
    {
        if (settingDic.ContainsKey(update.playerID) == false)
        {
            PlayerSetting setting = new PlayerSetting(update.playerID);
            settingDic[update.playerID] = setting;
        }
        settingDic[update.playerID].team = update.team;
    }

    public void UpdatePlayer(C2S.UpdatePlayerStatus update)
    {
        if (settingDic.ContainsKey(update.playerID) == false)
        {
            PlayerSetting setting = new PlayerSetting(update.playerID);
            settingDic[update.playerID] = setting;
        }

        settingDic[update.playerID].status = update.status;
    }

    public void SetSetting(PlayerSetting setting)
    {
        settingDic[setting.playerID] = setting;
    }

    public List<PlayerSetting> GetSettings()
    {
        return settingDic.Values.ToList<PlayerSetting>();
    }

    public int GetUniqueID()
    {
        return Interlocked.Increment(ref idCount);
    }

    public int SetID(int ID, NetworkPlayer player)
    {
        playerIDDic[ID] = player;
        return ID;
    }

    public int GetID(NetworkPlayer player)
    {
        foreach(var data in playerIDDic)
        {
            if (data.Value == player)
                return data.Key;
        }

        return -1;
    }

    public bool IsValidPlayer(int ID, NetworkPlayer player)
    {
        return playerIDDic[ID] == player;
    }

    public NetworkPlayer GetPlayer(int ID)
    {
        return playerIDDic[ID];
    }
}

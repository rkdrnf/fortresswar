using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading;
using Const;
using S2C = Communication.S2C;
using C2S = Communication.C2S;
using UnityEngine.Networking;

public class PlayerManager : MonoBehaviour
{
    private PlayerObjManager playerObjManager;

    private Dictionary<int, PlayerSetting> settingDic;

    private Dictionary<int, NetworkConnection> playerConDic;

    int idCount;


    private Dictionary<ServerPlayer, bool> playerLoadDic;

    object managerLock = new object();

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
        playerConDic = new Dictionary<int, NetworkConnection>();
        playerLoadDic = new Dictionary<ServerPlayer, bool>();
    }



    // Dictionary Functions
    public ServerPlayer Get(int player)
    {
        return playerObjManager.Get(player);
    }

    public void Set(int player, ServerPlayer character)
    {
        playerObjManager.Set(player, character);
    }

    public void Remove(int player)
    {
        playerLoadDic.Remove(Get(player));
        playerObjManager.Remove(player);
    }

    public bool Exists(NetworkConnection player)
    {
        foreach (var data in playerConDic)
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

    public void UpdatePlayerName(int playerID, string name)
    {
        if (settingDic.ContainsKey(playerID) == false)
        {
            PlayerSetting setting = new PlayerSetting(playerID);
            settingDic[playerID] = setting;
        }

        settingDic[playerID].name = name;
    }

    public void UpdatePlayerTeam(int playerID, Team team)
    {
        if (settingDic.ContainsKey(playerID) == false)
        {
            PlayerSetting setting = new PlayerSetting(playerID);
            settingDic[playerID] = setting;
        }
        settingDic[playerID].team = team;
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

    public int SetID(int ID, NetworkConnection conn)
    {
        playerConDic[ID] = conn;
        return ID;
    }

    public int GetID(NetworkConnection conn)
    {
        foreach (var data in playerConDic)
        {
            if (data.Value == conn)
                return data.Key;
        }

        return -1;
    }

    public bool IsValidPlayer(int ID, NetworkConnection conn)
    {
        return playerConDic[ID] == conn;
    }

    public NetworkConnection GetConnection(int ID)
    {
        return playerConDic[ID];
    }

    public void StartLoadUser(ServerPlayer player)
    {
        lock (managerLock)
        {
            playerLoadDic.Add(player, false);
        }
    }

    public void CompleteLoad(ServerPlayer player)
    {
        lock (managerLock)
        {
            playerLoadDic[player] = true;
        }
    }

    public bool IsLoadComplete()
    {
        bool loadedAll = true;

        lock (managerLock)
        {
            foreach (bool loaded in playerLoadDic.Values)
            {
                loadedAll = loaded;

                if (loadedAll == false)
                    break;
            }
        }

        return loadedAll;
    }
}

public class PlayerObjManager : MonoObjManager<int, ServerPlayer>
{ }


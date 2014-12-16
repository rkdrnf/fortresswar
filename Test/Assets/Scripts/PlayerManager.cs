using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerObjManager playerObjManager;

    private Dictionary<NetworkPlayer, PlayerSetting> settingDic;


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
        settingDic = new Dictionary<NetworkPlayer, PlayerSetting>();
    }

    void FixedUpdate()
    {
        foreach (PlayerBehaviour player in playerObjManager.Values)
        {
            if (player.IsDead())
            {
                player.UpdateRevivalTimer(Time.deltaTime);

                if (player.CanRevive())
                {
                    player.Revive();
                }

                continue;
            }

            if (Game.Inst.map.CheckInBorder(player) == false)
            {
                player.networkView.RPC("Die", RPCMode.All);
            }
        }
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log(String.Format("Player Disconnected! {0}", player));

        PlayerBehaviour character = playerObjManager.Get(player);

        character.RemoveCharacterFromNetwork();

        networkView.RPC("OnPlayerRemoved", RPCMode.All, player);
    }

    [RPC]
    void OnPlayerRemoved(NetworkPlayer player)
    {
        playerObjManager.Remove(player);
        settingDic.Remove(player);
    }

    // Dictionary Functions
    public PlayerBehaviour Get(NetworkPlayer player)
    {
        return playerObjManager.Get(player);
    }

    public void Set(NetworkPlayer player, PlayerBehaviour character)
    {
        playerObjManager.Set(player, character);
    }

    public void Remove(NetworkPlayer player)
    {
        playerObjManager.Remove(player);
    }

    public bool Exists(NetworkPlayer player)
    {
        return playerObjManager.Exists(player);
    }

    public void Clear()
    {
        playerObjManager.Clear();
    }

    public PlayerSetting GetSetting(NetworkPlayer player)
    {
        if (settingDic.ContainsKey(player))
            return settingDic[player];

        return null;
    }

    public void SetSetting(PlayerSetting setting)
    {
        settingDic[setting.player] = setting;
    }

    public List<PlayerSetting> GetSettings()
    {
        return settingDic.Values.ToList<PlayerSetting>();
    }
}

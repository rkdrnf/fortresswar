using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerObjManager playerObjManager;

    private static PlayerManager instance;

    public static PlayerManager Instance
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

            if (Game.Instance.map.CheckInBorder(player) == false)
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

        networkView.RPC("OnCharacterRemoved", RPCMode.All, player);
    }

    [RPC]
    void OnCharacterRemoved(NetworkPlayer player)
    {
        playerObjManager.Remove(player);
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
}

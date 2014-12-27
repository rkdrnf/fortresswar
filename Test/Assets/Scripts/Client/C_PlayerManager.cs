﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using UnityEngine;
using System.Threading;

namespace Client
{
    class C_PlayerManager
    {
        private PlayerObjManager playerObjManager;

        private Dictionary<int, PlayerSetting> settingDic;

        private Dictionary<PlayerBehaviour, bool> playerLoadDic;

        private static C_PlayerManager instance;

        object managerLock = new object();

        public static C_PlayerManager Inst
        {
            get
            {
                if (instance == null)
                {
                    instance = new C_PlayerManager();
                }
                return instance;
            }
        }

        public C_PlayerManager()
        {
            playerObjManager = new PlayerObjManager();
            settingDic = new Dictionary<int, PlayerSetting>();
            playerLoadDic = new Dictionary<PlayerBehaviour, bool>();
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
            playerLoadDic.Remove(Get(player));
            playerObjManager.Remove(player);
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

        public void StartLoadUser(PlayerBehaviour player)
        {
            lock(managerLock)
            { 
                playerLoadDic.Add(player, false);
            }
        }

        public void CompleteLoad(PlayerBehaviour player)
        {
            lock(managerLock)
            {
                playerLoadDic[player] = true;
            }
        }

        public bool IsLoadComplete()
        {
            bool loadedAll = true;

            lock(managerLock)
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

    public class PlayerObjManager : MonoObjManager<int, PlayerBehaviour>
    { }


}

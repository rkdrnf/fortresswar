using UnityEngine;
using System.Collections;
using System.Text;
using System;
using S2C = Packet.S2C;

public class GameMenu : MonoBehaviour
{
    PlayerSetting setting;

    void Awake()
    {
        gameObject.SetActive(false);
        setting = new PlayerSetting(Network.player, "");
    }
    void OnGUI()
    {
        setting.name = GUI.TextField(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 80, 200f, 20f), setting.name, 15);

        if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 20, 100f, 20f), "Enter"))
        {
            setting.player = Network.player;

            if (Network.isServer)
            {
                Game.Inst.OnPlayerReady(setting.Serialize(), new NetworkMessageInfo());
            }
            else if (Network.isClient)
            {
                Game.Inst.networkView.RPC("OnPlayerReady", RPCMode.Server, setting.Serialize());
            }
            
            gameObject.SetActive(false);
        }
    }
}

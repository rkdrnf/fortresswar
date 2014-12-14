using UnityEngine;
using System.Collections;
using System.Text;
using System;
using S2C = Packet.S2C;

public class GameMenu : MonoBehaviour
{
    S2C.GameSetting setting;

    void Awake()
    {
        gameObject.SetActive(false);
        setting = new S2C.GameSetting();
    }
    void OnGUI()
    {
        setting.name = GUI.TextField(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 80, 200f, 20f), setting.name, 15);

        if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 20, 100f, 20f), "Enter"))
        {
            if (Network.isServer)
            {
                Game.Instance.OnPlayerReady(Network.player, setting.Serialize());
            }
            else if (Network.isClient)
            {
                Game.Instance.networkView.RPC("OnPlayerReady", RPCMode.Server, Network.player, setting.Serialize());
            }
            
            gameObject.SetActive(false);
        }
    }
}

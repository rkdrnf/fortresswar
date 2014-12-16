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
        setting = new PlayerSetting(-1, "");
    }
    void OnGUI()
    {
        setting.name = GUI.TextField(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 80, 200f, 20f), setting.name, 15);

        if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 20, 100f, 20f), "Enter"))
        {
            setting.playerID = Game.Inst.GetID();

            if (Network.isServer)
            {
                Game.Inst.OnEnterCharacter(setting);
            }
            else if (Network.isClient)
            {
                Game.Inst.networkView.RPC("EnterCharacter", RPCMode.Server, setting.SerializeToBytes());
            }
            
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Text;
using System;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
public class GameMenu : MonoBehaviour
{
    C2S.UpdatePlayerName updateName;

    void Awake()
    {
        gameObject.SetActive(false);
        updateName = new C2S.UpdatePlayerName(-1, "");
    }
    void OnGUI()
    {
        updateName.name = GUI.TextField(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 80, 200f, 20f), updateName.name, 15);

        if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 20, 100f, 20f), "Enter"))
        {
            updateName.playerID = Game.Inst.GetID();

            if (Network.isServer)
            {
                //Server Specific
                Game.Inst.ServerSetPlayerName(updateName.SerializeToBytes(), new NetworkMessageInfo());
            }
            else if (Network.isClient)
            {
                Game.Inst.networkView.RPC("ServerSetPlayerName", RPCMode.Server, updateName.SerializeToBytes());
            }
            
            gameObject.SetActive(false);
        }
    }
}

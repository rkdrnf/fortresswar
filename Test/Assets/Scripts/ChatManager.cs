using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ChatManager : MonoBehaviour {

    const int CHAT_WINDOW_TIME = 5;

    double chatWindowTimer;
    
    List<Chat> chatList;

    string writingMessage;

    bool focused;

    void Awake()
    {
        chatList = new List<Chat>();
        writingMessage = "";
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        ShowChat();
        if (chatWindowTimer > 0f)
        {
            chatWindowTimer -= Time.deltaTime;
        }
	}

    void Chat(string text)
    {
        networkView.RPC("BroadCastChat", RPCMode.All, text);
    }


    [RPC]
    void BroadCastChat(string text, NetworkMessageInfo info)
    {
        NewChat(info.sender, text);
        ShowChat();
    }

    void NewChat(NetworkPlayer player, string text)
    {
        chatList.Add(new Chat(player, text));
    }

    void ShowChat()
    {
        chatWindowTimer = CHAT_WINDOW_TIME;
    }

    void OnGUI()
    {
        // don't draw chat window if timer is expired.
        if (chatWindowTimer <= 0f)
        {
            return;
        }

        GUI.BeginScrollView(new Rect(20, Screen.height - 300, 300, 200), Vector2.zero, new Rect(0, 0, 220, 200));
        GUIStyle areaStyle = new GUIStyle();
        areaStyle.wordWrap = true;
        areaStyle.stretchHeight = true;
        string chatLog = String.Join("\n", chatList.Select(c => c.text).ToArray<string>());
        GUI.TextArea(new Rect(0, 0, 300, 200), chatLog, areaStyle);

        GUI.EndScrollView();
        writingMessage = GUI.TextField(new Rect(20, Screen.height - 100, 300, 20), writingMessage);
    }
}

﻿using UnityEngine;
using System.Collections;
using System.Text;
using System;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using Util;
using Const;

public class NameSelector : MonoBehaviour
{
    private NameSelectorSM stateManager;

    string playerName;
    

    void Start ()
    {
        stateManager = new NameSelectorSM();
        stateManager.SetState(NameSelectorState.OFF);

        playerName = "";
    }

    public void Open()
    {
        stateManager.SetState(NameSelectorState.ON);
    }

    public void Close()
    {
        stateManager.SetState(NameSelectorState.OFF);
    }


    void OnGUI()
    {
        if (stateManager.IsInState(NameSelectorState.OFF))
            return;

        playerName = GUI.TextField(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 80, 200f, 20f), playerName, 15);

        if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 20, 100f, 20f), "Enter"))
        {
            Close();
            ClientGame.Inst.SetPlayerName(playerName);
            
        }
    }
}

class NameSelectorSM : StateManager<NameSelectorState>
{
    public override void SetState(NameSelectorState newState)
    {
        ClientGame client = ClientGame.Inst;
        switch(newState)
        {
            case NameSelectorState.ON:
                client.keyFocusManager.FocusTo(InputKeyFocus.NAME_SELECTOR);
                client.mouseFocusManager.FocusTo(InputMouseFocus.NAME_SELECTOR);
                StateUtil.SetState<NameSelectorState>(ref state, newState);
                break;

            case NameSelectorState.OFF:
                client.keyFocusManager.FreeFocus(InputKeyFocus.NAME_SELECTOR);
                client.mouseFocusManager.FreeFocus(InputMouseFocus.NAME_SELECTOR);
                StateUtil.SetState<NameSelectorState>(ref state, newState);
                break;
        }
    }
}

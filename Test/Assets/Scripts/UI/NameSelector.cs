using UnityEngine;
using System.Collections;
using System.Text;
using System;
using S2C = Communication.S2C;
using C2S = Communication.C2S;
using Util;
using Const;
using Server;


namespace InGameMenu
{
    public class NameSelector : MonoBehaviour
    {
        private NameSelectorSM stateManager;

        string playerName;

        public NameSelector()
        {
            Debug.Log("Start nameConstructor");
            stateManager = new NameSelectorSM();
            playerName = "";
        }


        void Start()
        {
            stateManager.SetState(NameSelectorState.OFF);
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
                ServerGame.Inst.UpdatePlayerName(playerName);

            }
        }
    }

    class NameSelectorSM : StateManager<NameSelectorState>
    {
        public override void SetState(NameSelectorState newState)
        {
            ServerGame game = ServerGame.Inst;
            switch (newState)
            {
                case NameSelectorState.ON:
                    game.keyFocusManager.FocusTo(InputKeyFocus.NAME_SELECTOR);
                    game.mouseFocusManager.FocusTo(InputMouseFocus.NAME_SELECTOR);
                    StateUtil.SetState<NameSelectorState>(ref state, newState);
                    break;

                case NameSelectorState.OFF:
                    game.keyFocusManager.FreeFocus(InputKeyFocus.NAME_SELECTOR);
                    game.mouseFocusManager.FreeFocus(InputMouseFocus.NAME_SELECTOR);
                    StateUtil.SetState<NameSelectorState>(ref state, newState);
                    break;
            }
        }
    }
}

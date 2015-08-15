using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Const;
using Util;
using UnityEngine.Networking.Match;
using System.Linq;

public class Menu : MonoBehaviour {

    MenuState state;

    bool IsInState(MenuState state)
    {
        return Util.StateUtil.IsInState(this.state, state);
    }

    void SetState(MenuState state)
    {
        Util.StateUtil.SetState(ref this.state, state);

        Camera menuCamera = GameObject.Find("MenuCamera").GetComponent<Camera>();

        switch (state)
        {
            case MenuState.OFF:
                menuCamera.enabled = false;
                GameNetworkManager.Inst.ClearServerList();
                break;

            case MenuState.ON:
                menuCamera.enabled = true;
                GameNetworkManager.Inst.ClearServerList();
                break;
        }

        return;
    }

	public void RefreshServerList()
	{
        if (IsInState(MenuState.OFF))
            return;

        GameNetworkManager.Inst.RefreshHostList();
	}

	public void StartServer()
	{
        if (IsInState(MenuState.OFF))
            return;

        SetState(MenuState.OFF);
		GameNetworkManager.Inst.Host();
	}

	void OnGUI()
	{
        if (IsInState(MenuState.OFF))
            return;

        MatchDesc[] hostList = GameNetworkManager.Inst.GetRooms();

		if (hostList.Length > 0)
		{
			for (int i = 0; i < hostList.Length; i++) 
			{
				if (GUI.Button(new Rect(20, 70 * (i + 1) + 20, 450, 50)
                    , string.Format("Name: {0}, IP: {1}, Players: {2}"
                    , hostList[i].name
                    , String.Join(".", hostList[i].directConnectInfos.Select(info => info.publicAddress).ToArray())
                    , hostList[i].currentSize
                    )))
				{
                    SetState(MenuState.OFF);
                    GameNetworkManager.Inst.Connect(hostList[i]);
				}
			}
		}
	}

	void Update()
	{
		if (Input.GetButtonUp ("Cancel"))
		{
            if (IsInState(MenuState.OFF))
            {
                SetState(MenuState.ON);
                return;
            }

            if (IsInState(MenuState.ON))
            {
                SetState(MenuState.OFF);
                return;
            }
		}
	}
}

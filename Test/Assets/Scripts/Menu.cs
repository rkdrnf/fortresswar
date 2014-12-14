using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Const;
using Util;

public class Menu : MonoBehaviour {

	public GameObject netManagerObj;


	NetworkManager netManager;

    MenuState state;

    bool IsInState(MenuState state)
    {
        return Util.StateUtil.IsInState(this.state, state);
    }

    void SetState(MenuState state)
    {
        Util.StateUtil.SetState(out this.state, state);

        Camera menuCamera = GameObject.Find("MenuCamera").camera;

        switch (state)
        {
            case MenuState.OFF:
                menuCamera.enabled = false;
                netManager.ClearServerList();
                break;

            case MenuState.ON:
                menuCamera.enabled = true;
                netManager.ClearServerList();
                break;
        }

        return;
    }

	void Awake()
	{
		netManager = netManagerObj.GetComponent<NetworkManager> ();
	}

	public void RefreshServerList()
	{
        if (IsInState(MenuState.OFF))
            return;

		netManager.RefreshHostList ();
	}

	public void StartServer()
	{
        if (IsInState(MenuState.OFF))
            return;

        SetState(MenuState.OFF);
		netManager.StartServer ();
	}

	void OnGUI()
	{
        if (IsInState(MenuState.OFF))
            return;

		HostData[] hostList = netManager.GetServerList ();

		if (hostList.Length > 0)
		{
			for (int i = 0; i < hostList.Length; i++) 
			{
				if (GUI.Button(new Rect(20, 70 * (i + 1) + 20, 450, 50), string.Format("Name: {0}, IP: {1}:{2}, Players: {3}, Nat:{4}", hostList[i].gameName, String.Join(".", hostList[i].ip), hostList[i].port, hostList[i].connectedPlayers, hostList[i].useNat)))
				{
                    SetState(MenuState.OFF);
					var cError = netManager.Connect(hostList[i]);
					Debug.Log(cError);
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

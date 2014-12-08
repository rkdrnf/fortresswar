using UnityEngine;
using System.Collections;
using System.Text;
using System;

public class Menu : MonoBehaviour {

	public GameObject netManagerObj;


	NetworkManager netManager;

	void Awake()
	{
		netManager = netManagerObj.GetComponent<NetworkManager> ();
	}

	void Start()
	{
	}

	public void RefreshServerList()
	{
		netManager.RefreshHostList ();
	}

	public void StartServer()
	{
		netManager.StartServer ();
	}

	void OnGUI()
	{
		HostData[] hostList = netManager.GetServerList ();

		if (hostList.Length > 0)
		{
			for (int i = 0; i < hostList.Length; i++) 
			{
				if (GUI.Button(new Rect(20, 70 * (i + 1) + 20, 450, 50), string.Format("Name: {0}, IP: {1}:{2}, Players: {3}, Nat:{4}", hostList[i].gameName, String.Join(".", hostList[i].ip), hostList[i].port, hostList[i].connectedPlayers, hostList[i].useNat)))
				{
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
			Camera menuCamera = GameObject.Find("MenuCamera").camera;
			menuCamera.enabled = !menuCamera.enabled;

			netManager.ClearServerList();
		}
	}
}

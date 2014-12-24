using UnityEngine;
using System.Collections;
using System;

public class NetworkManager : MonoBehaviour {

	HostData[] hostList;

	void Awake() {
		Network.sendRate = 100f;
		ClearServerList ();
	}

	void Start() {

	}

	public void StartServer()
	{
		Network.InitializeServer (16, 25001, !Network.HavePublicAddress());
		MasterServer.RegisterHost ("War", "WarGame123", "Fun");
	}

	public void RefreshHostList()
	{
		MasterServer.RequestHostList ("War");
		hostList = MasterServer.PollHostList ();
		MasterServer.ClearHostList();
	}

	public HostData[] GetServerList()
	{
		return hostList;
	}
	
	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.RegistrationSucceeded) {
			Debug.Log("Server Registered!");
		}
	}

	public NetworkConnectionError Connect(HostData hostData)
	{
		return Network.Connect(hostData);
	}

	public void ClearServerList()
	{
		hostList = new HostData[]{};
	}

	void OnConnectedToServer()
	{
		Debug.Log("Connected To Server");
	}
}

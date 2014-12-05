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
		Network.InitializeServer (16, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost ("War", "WarGame", "Fun");
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
	
	void OnServerInitialized()
	{
		Debug.Log ("Server Initialized!");

		Game.ClearGame ();
		GameObject serverPlayer = Game.MakeNetworkPlayer ();
		serverPlayer.GetComponent<PlayerBehaviour>().SetOwner();
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

		Game.ClearGame ();
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log(String.Format("Player Connected {0}", player));

		Game.spawnNetworkPlayer (player);
		Game.map.drawMapNetwork (player);

	}

	[RPC]
	void EnterNewPlayer(NetworkMessageInfo nmInfo)
	{
		Game.spawnNetworkPlayer (nmInfo.sender);
	}
}

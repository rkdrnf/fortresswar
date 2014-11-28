using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	public GameObject gameObj;

	HostData[] hostList;

	Game game;

	void Awake() {
		game = gameObj.GetComponent<Game> ();
		Network.sendRate = 25f;
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

		game.ClearGame ();
		
		game.spawnNetworkPlayer (networkView.viewID);
	}

	public NetworkConnectionError Connect(HostData hostData)
	{
		return Network.Connect(hostData);
	}

	public void ClearServerList()
	{
		hostList = new HostData[0]{};
	}

	void OnConnectedToServer()
	{
		Debug.Log("Connected To Server");

		game.ClearGame ();

		networkView.RPC ("EnterNewPlayer", RPCMode.Server);
	}

	[RPC]
	void EnterNewPlayer(NetworkMessageInfo nmInfo)
	{
		game.spawnNetworkPlayer (nmInfo..viewID);
	}

	[RPC]
	void EnterNewPlayerResponse(NetworkViewID viewID)
	{
		game.spawnPlayer (viewID);
	}
}

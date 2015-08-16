using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using Communication;

public class GameNetworkManager : NetworkManager {

    private NetworkMatch m_match;
    private MatchDesc[] m_rooms;

    private static GameNetworkManager instance;

    public static GameNetworkManager Inst
    {
        get
        {
            if (instance == null)
            {
                instance = new GameNetworkManager();
            }
            return instance;
        }
    }

	void Awake() {
        instance = this;

        m_match = gameObject.AddComponent<NetworkMatch>();
		Network.sendRate = 100f;
		ClearServerList ();
	}

	void Start() {

	}

	public void Host()
	{
        CreateMatchRequest matchRequest = new CreateMatchRequest();
        matchRequest.name = "Test";
        matchRequest.size = 4;
        matchRequest.advertise = true;
        matchRequest.password = "";

        m_match.CreateMatch(matchRequest, OnMatchCreate);
	}

    public override void OnMatchCreate(CreateMatchResponse matchInfo)
    {
        base.OnMatchCreate(matchInfo);
        Debug.Log("Room created!");
        
        PacketHandler.RegisterServerPacketHandlers();
        ServerGame.Inst.Run();
    }

	public void RefreshHostList()
	{
        m_match.ListMatches(0, 20, "", OnMatchList);
	}

    public override void OnMatchList(ListMatchResponse matchList)
    {
        if (matchList.success)
        {
            m_rooms = matchList.matches.ToArray();
        }
    }

	public MatchDesc[] GetRooms()
	{
        return m_rooms;
	}
	
	public void Connect(MatchDesc room)
	{
        m_match.JoinMatch(room.networkId, "", OnMatchJoined);
	}

	public void ClearServerList()
	{
		m_rooms = new MatchDesc[]{};
	}

    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("New Player Joined!");
        //ServerGame.Inst.OnNewPlayerJoin(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("Player Disconnected!");
        ServerGame.Inst.OnGamePlayerDisconnected(conn);

        base.OnServerDisconnect(conn);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("Connected to Server!");
        base.OnClientConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
    }

    public override void OnStartClient(NetworkClient client)
    {
        Debug.Log("[GameNetworkManager] OnStartClient");
        base.OnStartClient(client);

        PacketHandler.RegisterClientPacketHandlers(client);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("AddPlayer called");
        ServerGame.Inst.AddPlayerCharacter(conn, playerControllerId);
    }


    

    
}

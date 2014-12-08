//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.18444
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Threading;

public class Game : MonoBehaviour
{
    private static Game instance;

	public static Game Instance
	{
		get
		{
			if (instance == null) {
				instance = new Game ();
			}
			return instance;
		}
	}

    public static bool IsInitialized() { return instance != null; }

	public bool IsMapLoaded() { return map != null; }

	public GameObject netManagerObject;
	public Vector3 spawnPosition;
	public GameObject playerPrefab;
    public ProjectileSet projectileSet;

	
    Map map;
	PlayerBehaviour[] players;
	NetworkManager netManager;
	MapLoader mapLoader;

    

    public void Init()
    {
        instance = this;

        projectileObjectTable = new Hashtable();
        playerObjectTable = new Hashtable();
    }

	void Awake()
	{
        Init();
		players = new PlayerBehaviour[]{};
		netManager = netManagerObject.GetComponent<NetworkManager> ();
		mapLoader = GetComponent<MapLoader> ();

	}

	public void LoadMap(Map loadingMap)
	{
		this.map = loadingMap;
	}

	public void StartServerGame()
	{
		ClearGame ();

		if (!IsMapLoaded())
		{
			LoadMap(mapLoader.GetMap());
		}

		GameObject serverPlayer = MakeNetworkPlayer ();
        PlayerBehaviour character = serverPlayer.GetComponent<PlayerBehaviour>();
		character.SetOwner();

        RegisterCharacter(Network.player, character);
	}

	public void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log(String.Format("Player Connected {0}", player));
		
		GameObject newPlayer = Game.Instance.MakeNetworkPlayer ();
        PlayerBehaviour character = newPlayer.GetComponent<PlayerBehaviour>();
		newPlayer.networkView.RPC ("SetOwner", player);
        RegisterCharacter(player, character);

		MakeNetworkMap (player, map.mapName);
	}

	public GameObject MakeNetworkPlayer()
	{
		return (GameObject)Network.Instantiate (playerPrefab, spawnPosition, Quaternion.identity, 0);
	}

    public void spawnNetworkPlayer(NetworkPlayer client)
    {

    }

	public void MakeNetworkMap(NetworkPlayer player, string mapName)
	{
		networkView.RPC ("ClientMakeMap", player, mapName);
	}

    public void ClearGame()
    {
		foreach (PlayerBehaviour player in players)
		{
			Network.Destroy(player.gameObject);
		}

		map = null;
		networkView.RPC ("ClientClearGame", RPCMode.Others);
    }

	[RPC]
	public void ClientMakeMap(string mapName)
	{
		this.map = mapLoader.GetMap(mapName);
	}

	[RPC]
	public void ClientClearGame()
	{
		map = null;
	}


    Hashtable projectileObjectTable;
    long totalProjectileCount = 0;

    private long GetUniqueKeyForNewProjectile()
    {
        return Interlocked.Increment(ref totalProjectileCount);
    }

    public Projectile GetProjectile(long projectileID)
    {
        return (Projectile)projectileObjectTable[projectileID];
    }

    public void ClientRegisterProjectile(long projectileID, Projectile projectile)
    {
        projectileObjectTable.Add(projectileID, projectile);
    }

    public void ServerRegisterProjectile(Projectile projectile)
    {
        long id = GetUniqueKeyForNewProjectile();
        projectileObjectTable.Add(id, projectile);
    }

    Hashtable playerObjectTable;

    public PlayerBehaviour GetCharacter(NetworkPlayer player)
    {
        return (PlayerBehaviour)playerObjectTable[player];
    }

    public void RegisterCharacter(NetworkPlayer player, PlayerBehaviour character)
    {
        playerObjectTable.Add(player, character);
    }
}
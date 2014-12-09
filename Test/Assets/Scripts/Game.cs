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
using System.Collections.Generic;
using Newtonsoft.Json;

using S2C = Packet.S2C;
using C2S = Packet.C2S;


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
    public MapData mapData;

    Map map;
	PlayerBehaviour[] players;
	NetworkManager netManager;
	MapLoader mapLoader;

    public void Init()
    {
        instance = this;

        projectileObjectTable = new Dictionary<long, Projectile>();
        playerObjectTable = new Dictionary<NetworkPlayer, PlayerBehaviour>();
    }

	void Awake()
	{
        Init();
		players = new PlayerBehaviour[]{};
		netManager = netManagerObject.GetComponent<NetworkManager> ();
		mapLoader = GetComponent<MapLoader> ();

	}

	public void LoadMap()
	{
        GameObject mapPrefab = mapLoader.GetMap();

        GameObject mapObj = (GameObject)Network.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity, 0);
	}

    public void SetMap(Map map)
    {
        this.map = map;
    }

	public void StartServerGame()
	{
		ClearGame ();

		if (!IsMapLoaded())
		{
			LoadMap();
		}

		GameObject serverPlayer = MakeNetworkPlayer ();
        PlayerBehaviour character = serverPlayer.GetComponent<PlayerBehaviour>();
		character.SetOwner();

        RegisterCharacter(Network.player, character);

        Debug.Log(JsonConvert.SerializeObject(Network.player));
	}

	public void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log(String.Format("Player Connected {0}", player));
		
		GameObject newPlayer = Game.Instance.MakeNetworkPlayer ();
        PlayerBehaviour character = newPlayer.GetComponent<PlayerBehaviour>();
		newPlayer.networkView.RPC ("SetOwner", player);
        RegisterCharacter(player, character);

		//MakeNetworkMap (player, map.mapName); StartServerGame 에서 Network.Instantiate

        Debug.Log(JsonConvert.SerializeObject(player));
	}

	public GameObject MakeNetworkPlayer()
	{
		return (GameObject)Network.Instantiate (playerPrefab, spawnPosition, Quaternion.identity, 0);
	}

    /*
	public void MakeNetworkMap(NetworkPlayer player, string mapName)
	{
		networkView.RPC ("ClientMakeMap", player, mapName);
	}
    */
    public void ClearGame()
    {
		foreach (PlayerBehaviour player in players)
		{
			Network.Destroy(player.gameObject);
		}

		map = null;
		networkView.RPC ("ClientClearGame", RPCMode.Others);
    }

    /*
	[RPC]
	public void ClientMakeMap(string mapName)
	{
		this.map = mapLoader.GetMap(mapName);
	}
    */
	[RPC]
	public void ClientClearGame()
	{
		map = null;
	}


    Dictionary<long, Projectile> projectileObjectTable;
    long totalProjectileCount = 0;

    public long GetUniqueKeyForNewProjectile()
    {
        return Interlocked.Increment(ref totalProjectileCount);
    }

    public Projectile GetProjectile(long projectileID)
    {
        return (Projectile)projectileObjectTable[projectileID];
    }

    public void RegisterProjectile(long projectileID, Projectile projectile)
    {
        projectileObjectTable.Add(projectileID, projectile);
    }

    // Projectile management

    [RPC]
    public void DestroyProjectile(string destroyProjectileJson)
    {
        S2C.DestroyProjectile pck = S2C.DestroyProjectile.Deserialize(destroyProjectileJson);

        if (Network.isServer)
        {
            Debug.Log(string.Format("[SERVER] projectile {0} Destroyed", pck.projectileID));
            Destroy((UnityEngine.Object)projectileObjectTable[pck.projectileID].gameObject);
            networkView.RPC("DestroyProjectile", RPCMode.Others, destroyProjectileJson);
        }
        else if (Network.isClient)
        {
            Debug.Log(string.Format("[CLIENT] projectile {0} Destroyed", pck.projectileID));
            Destroy((UnityEngine.Object)projectileObjectTable[pck.projectileID].gameObject);
        }
    }

    Dictionary<NetworkPlayer, PlayerBehaviour> playerObjectTable;

    public PlayerBehaviour GetCharacter(NetworkPlayer player)
    {
        return (PlayerBehaviour)playerObjectTable[player];
    }

    public void RegisterCharacter(NetworkPlayer player, PlayerBehaviour character)
    {
        Debug.Log(string.Format("NetworkPlayer {0}, Character {1} registered", player, character));
        playerObjectTable.Add(player, character);
    }
}
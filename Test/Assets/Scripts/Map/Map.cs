using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using S2C = Packet.S2C;
using C2S = Packet.C2S;

using Server;
using Const;

public class Map : MonoBehaviour {

    string mapName;

    int mapWidth;
    int mapHeight;

    Dictionary<int, Tile> tileList = new Dictionary<int,Tile>();

    double mapLoadTime;

    Parallax backgroundPar;

    void Awake()
    {
        networkView.group = NetworkViewGroup.GAME;
    }

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        mapLoadTime = 0f;
        Game.Inst.SetMap(this);

        if (Network.isServer)
        {
            Debug.Log("[Server] map instantiated by " + info.sender);
            //Send Clients MapInfo;

            OnSetMapInfo(Game.Inst.mapData.name);
            networkView.RPC("SetMapInfo", RPCMode.OthersBuffered, Game.Inst.mapData.name);
        }
    }

    [RPC]
    void SetMapInfo(string mapName, NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //CheckServer

        OnSetMapInfo(mapName);
        networkView.RPC("RequestMapStatus", RPCMode.Server);
    }

    void OnSetMapInfo(string mapName)
    {
        MapData mapData = Resources.Load("Maps/" + mapName, typeof(MapData)) as MapData;
        Load(mapData);

        if (Network.isServer)
            mapLoadTime = Network.time;
    }

    [RPC]
    void RequestMapStatus(NetworkMessageInfo info)
    {
        if (!Network.isServer) return;

        S2C.MapInfo mapInfo = new S2C.MapInfo(tileList);

        networkView.RPC("SetMapStatus", info.sender, mapInfo.SerializeToBytes());
    }

    [RPC]
    void SetMapStatus(byte[] mapInfoData, NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //CheckServer

        mapLoadTime = info.timestamp;

        S2C.MapInfo mapInfo = S2C.MapInfo.DeserializeFromBytes(mapInfoData);

        foreach(var tileStatus in mapInfo.tileStatusList)
        {
            Tile tile = GetTile(tileStatus.ID);
            tile.DamageInternal(tile.maxHealth - tileStatus.health, Vector2.zero);
        }

        Client.ClientGame.Inst.OnMapLoadCompleted();
    }

    public Tile GetTile(int ID)
    {
        return tileList[ID];
    }

    public void AddTile(Tile tile)
    {
        tile.transform.parent = this.transform;
        tileList.Add(tile.ID, tile);
    }

    public Dictionary<int, Tile> GetTileList()
    {
        return tileList;
    }

    public void Clear()
    {
        tileList.Clear();
    }

    public void Load(MapData mapData)
    {
        mapWidth = mapData.mapWidth;
        mapHeight = mapData.mapHeight;
        mapName = mapData.mapName;

        LoadBackground(mapData.backgroundImage);

        foreach (TileData tileData in mapData.tiles)
        {
            GameObject obj = Instantiate(mapData.tileSet.tiles[(int)tileData.tileType]) as GameObject;
            Tile tile = obj.GetComponent<Tile>();
            tile.ID = tileData.ID;
            tile.health = tileData.health;
            tile.transform.localPosition = new Vector3(tileData.x, tileData.y, 0);
            tile.map = this;

            AddTile(tile);
        }
    }

    public void LoadBackground(Sprite image)
    {
        backgroundPar = GetComponentInChildren<Parallax>();
        backgroundPar.SetImage(image, mapWidth, mapHeight);
    }

    public void OnApply(Sprite backgroundImage, int width, int height)
    {
        mapWidth = width;
        mapHeight = height;

        LoadBackground(backgroundImage);
    }

    [RPC]
    public void ClientDamageTile(byte[] damageTileData, NetworkMessageInfo info)
    {
        // (!Network.isClient) return;
        //ServerCheck

        //old Info
        if (mapLoadTime == 0f || mapLoadTime > info.timestamp) return;
            

        S2C.DamageTile pck = S2C.DamageTile.DeserializeFromBytes(damageTileData);
        Tile tile = GetTile(pck.tileID);
        tile.DamageInternal(pck.damage, pck.point);
    }

    public bool CheckInBorder(Transform obj)
    {
        return (obj.position.x > -mapWidth / 2f
            && obj.position.x < mapWidth / 2f
            && obj.position.y > -mapHeight / 2f
            && obj.position.y < mapHeight / 2f
            );
    }

    public bool CheckInBorder(ServerPlayer player)
    {
        return CheckInBorder(player.transform);
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Destroy(gameObject);
    }

    
}

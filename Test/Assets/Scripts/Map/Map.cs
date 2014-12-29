﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using S2C = Packet.S2C;
using C2S = Packet.C2S;

using Server;
using Const;

public class Map : MonoBehaviour {


    private Dictionary<int, Tile> tileList = new Dictionary<int,Tile>();
    
    MapData mapData;

    double mapLoadTime;

    void Awake()
    {
        networkView.group = NetworkViewGroup.GAME;
    }

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (Network.isServer)
        {
            Debug.Log("[Server] map instantiated by " + info.sender);
            //Send Clients MapInfo;

            OnSetMapInfo(Game.Inst.mapData.name);
            networkView.RPC("SetMapInfo", RPCMode.OthersBuffered, Game.Inst.mapData.name);
        }

        mapLoadTime = 0f;
        Game.Inst.SetMap(this);
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
        this.mapData = Resources.Load("Maps/" + mapName, typeof(MapData)) as MapData;
        this.Load(mapData);
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
            tile.DamageInternal(tile.maxHealth - tileStatus.health);
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
        mapData = null;
    }

    public void Load(MapData mapData)
    {
        this.mapData = mapData;

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

    [RPC]
    public void ClientDamageTile(byte[] damageTileData, NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //ServerCheck

        //old Info
        if (mapLoadTime == 0f || mapLoadTime > info.timestamp) return;
            

        S2C.DamageTile pck = S2C.DamageTile.DeserializeFromBytes(damageTileData);
        Tile tile = GetTile(pck.tileID);
        tile.DamageInternal(pck.damage);
    }

    public bool CheckInBorder(Transform obj)
    {
        return (obj.position.x > -mapData.mapWidth / 2f
            && obj.position.x < mapData.mapWidth / 2f
            && obj.position.y > -mapData.mapHeight / 2f
            && obj.position.y < mapData.mapHeight / 2f
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
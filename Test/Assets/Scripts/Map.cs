using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using S2C = Packet.S2C;
using C2S = Packet.C2S;

public class Map : MonoBehaviour {


    private Dictionary<int, Tile> tileList = new Dictionary<int,Tile>();
    
    MapData mapData;

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (Network.isServer)
        {
            Debug.Log("[Server] map instantiated by " + info.sender);
            //Send Clients MapInfo;

            networkView.RPC("SetMapInfo", RPCMode.AllBuffered, Game.Inst.mapData.name);
        }

        Game.Inst.SetMap(this);
    }

    [RPC]
    void SetMapInfo(string mapName)
    {
        this.mapData = Resources.Load("Maps/" + mapName, typeof(MapData)) as MapData;
        this.Load(mapData);
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
    public void BroadCastDamageTile(string damageTileJson)
    {
        if (Network.isClient)
        {
            S2C.DamageTile pck = S2C.DamageTile.Deserialize(damageTileJson);
            Tile tile = GetTile(pck.tileID);
            tile.DamageInternal(pck.damage);
        }
    }

    public bool CheckInBorder(Transform obj)
    {
        return (obj.position.x > -mapData.mapWidth / 2f
            && obj.position.x < mapData.mapWidth / 2f
            && obj.position.y > -mapData.mapHeight / 2f
            && obj.position.y < mapData.mapHeight / 2f
            );
    }

    public bool CheckInBorder(PlayerBehaviour player)
    {
        return CheckInBorder(player.transform);
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Destroy(gameObject);
    }

    
}

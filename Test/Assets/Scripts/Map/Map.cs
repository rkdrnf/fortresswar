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

    Dictionary<GridCoord, Tile> tileList = new Dictionary<GridCoord, Tile>();

    double mapLoadTime;

    Parallax backgroundPar;

    LayerMask tileLayer;

    void Awake()
    {
        tileLayer = LayerMask.GetMask("Tile");
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
            Tile tile = GetTile(tileStatus.coord);
            tile.SetHealth(tileStatus.health);
        }

        ServerGame.Inst.OnMapLoadCompleted(this);
    }

    public Tile GetTile(GridCoord coord)
    {
        if (tileList.ContainsKey(coord))
            return tileList[coord];
        else
            return null;
    }

    public void AddTile(Tile tile)
    {
        tile.transform.parent = this.transform;
        tileList.Add(tile.coord, tile);
    }

    public Dictionary<GridCoord, Tile> GetTileList()
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
            tile.coord = tileData.coord;
            tile.transform.localPosition = tileData.coord.ToVector2();
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
        Tile tile = GetTile(pck.tileCoord);
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

    public static bool IsLayerExists(Vector2 pos, LayerMask mask)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, float.MaxValue, mask);

        if (hit)
        {
            return true;
        }
        else return false;
    }

    public static GameObject GetLayerObjectAt(Vector2 pos, LayerMask mask)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, float.MaxValue, mask);

        if (hit)
        {
            return hit.collider.gameObject;
        }
        else return null;
    }

    public static Vector2 GetGridPos(Vector2 pos)
    {
        return new Vector2(Mathf.Floor(pos.x + 0.5f), Mathf.Floor(pos.y + 0.5f));
    }


    public int GetTileIndex(Tile tile)
    {
        return Map.GetTileIndex(tile, mapWidth);
    }

    static public int GetTileIndex(Tile tile, int mapWidth)
    {
        return Mathf.FloorToInt(tile.transform.localPosition.x) + Mathf.FloorToInt(tile.transform.localPosition.y) * mapWidth;
    }

    public GridCoord ToGridCoord(int index)
    {
        return GridCoord.ToCoord(index, mapWidth);
    }
}
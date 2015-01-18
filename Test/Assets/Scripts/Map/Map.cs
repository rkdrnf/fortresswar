using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using S2C = Packet.S2C;
using C2S = Packet.C2S;

using Server;
using Const;
using Architecture;

public class Map : MonoBehaviour {

    string m_name;

    int m_width;
    int m_height;

    double m_mapLoadTime;

    Parallax m_backgroundPar;

    

    private static Map instance;

    public static Map Inst
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        
        networkView.group = NetworkViewGroup.GAME;
    }

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        instance = this;

        m_mapLoadTime = 0f;

        if (Network.isServer)
        {
            Debug.Log("[Server] map instantiated by " + info.sender);
            //Send Clients MapInfo;

            OnSetMapInfo(Game.Inst.mapData.name, Network.time);
            networkView.RPC("SetMapInfo", RPCMode.OthersBuffered, Game.Inst.mapData.name);
        }
    }

    [RPC]
    void SetMapInfo(string mapName, NetworkMessageInfo info)
    {
        if (!Network.isClient) return;
        //CheckServer

        OnSetMapInfo(mapName, info.timestamp);

        ServerGame.Inst.OnMapLoadCompleted(this);
    }

    void OnSetMapInfo(string mapName, double time)
    {
        MapData mapData = Resources.Load("Maps/" + mapName, typeof(MapData)) as MapData;
        Load(mapData);

        m_mapLoadTime = time;
    }

    public void Load(MapData mapData)
    {
        m_width = mapData.mapWidth;
        m_height = mapData.mapHeight;
        m_name = mapData.mapName;

        LoadBackground(mapData.backgroundImage);

        if (!Network.isServer) return;
        LoadTiles(mapData);
        LoadBuildings();
    }

    public void LoadBackground(Sprite image)
    {
        m_backgroundPar = GetComponentInChildren<Parallax>();
        m_backgroundPar.SetImage(image, m_width, m_height);
    }

    public void LoadTiles(MapData mapData)
    {
        if (!Network.isServer) return; //서버만 데이터에서 로딩, 클라는 network로 초기화

        foreach (Tile tile in mapData.tiles)
        {
            /*
            Tile tile = (Tile)Network.Instantiate(mapData.tileSet.tiles[(int)tileData.tileType], tileData.coord.ToVector2(), Quaternion.identity, 4);
            tile.Init(tileData);

            TileManager.Inst.Add(tile);
             * */
        }
    }

    public void LoadBuildings()
    {
        if (!Network.isServer) return; //서버만 데이터에서 로딩, 클라는 network로 초기화
    }

    public bool CheckInBorder(Transform obj)
    {
        return (obj.position.x > -m_width / 2f
            && obj.position.x < m_width / 2f
            && obj.position.y > -m_height / 2f
            && obj.position.y < m_height / 2f
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


    public GridCoord ToGridCoord(int index)
    {
        return GridCoord.ToCoord(index, m_width);
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Communication;
using Server;
using Const;
using Architecture;
using UnityEngine.Networking;

using S2C = Communication.S2C;
using C2S = Communication.C2S;

namespace Maps
{ 
public class Map : NetworkBehaviour {

    string m_name;

    int m_width;
    int m_height;
    int m_chunkSize;

    double m_mapLoadTime;

    Parallax m_backgroundPar;

    public TileManager m_tileManager = null; // scene init
    public BuildingManager m_buildingManager = null; // scene init

    public MapData m_mapData = null; // scene init
    

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
        instance = this;

        m_mapLoadTime = 0f;
    }

    void Start()
    {
        Debug.Log("[Map] Start");
    }

    public void SetMap(string mapName)
    {
        m_name = mapName;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("[Map] OnStartClient");
    }

    public void OnNewPlayerJoin(NetworkConnection conn)
    {
        Debug.Log("[Map] PlayerConnect");

        SendCurrentMapData(conn);
    }

    void SendCurrentMapData(NetworkConnection conn)
    {
        S2C.MapInfo pck = new S2C.MapInfo(); 

        pck.m_mapName = m_mapData.name;
        pck.m_tiles = new List<S2C.TileStatus>();
        pck.m_buildings = new List<S2C.BuildingStatus>();

        foreach (Tile tile in m_tileManager.GetTiles())
        {
            pck.m_tiles.Add(new S2C.TileStatus(tile));
        }
        foreach (Building building in m_buildingManager.GetBuildings())
        {
            pck.m_buildings.Add(new S2C.BuildingStatus(building));
        }

        NetworkServer.SendToClient(conn.connectionId, (int)PacketType.SendMapInfo, pck);
    }

    public void ReceiveMapInfo(S2C.MapInfo mapInfo)
    {
        Debug.Log("[Client] map instantiated");

        Load(mapInfo);

        ServerGame.Inst.OnMapLoadCompleted(this);
    }

    public void Load(S2C.MapInfo mapInfo)
    {
        MapData mapData = (MapData)Resources.Load("Maps/" + mapInfo.m_mapName);
        Load(mapData);

        LoadTiles(mapInfo.m_tiles);
        LoadBuildings(mapInfo.m_buildings);
    }

    public bool Load(MapData mapData)
    {
        m_mapData = mapData;

        m_width = mapData.mapWidth;
        m_height = mapData.mapHeight;
        m_name = mapData.mapName;
        m_chunkSize = mapData.chunkSize;

        Lights.ShadowPane.Inst.Init(m_width, m_height);
        LoadBackground(mapData.backgroundImage);

        if (!isServer)
        {
            Debug.Log("Not server. Remote Client will receive map data from server.");
            return true;
        }

        LoadTiles(mapData.tiles);
        LoadBuildings(new List<Building>());
        
        m_mapLoadTime = Network.time;

        return true;
    }
    
    public void LoadBackground(Sprite image)
    {
        m_backgroundPar = GetComponentInChildren<Parallax>();
        m_backgroundPar.SetImage(image, m_width, m_height);
    }

    public void LoadTiles(IEnumerable<Tile> tiles)
    {
        if (!isServer) return; //서버만 데이터에서 로딩, 클라는 network로 초기화

        foreach (Tile tile in tiles)
        {
            m_tileManager.New(tile);
        }
    }

    public void LoadBuildings(IEnumerable<Building> buildings)
    {
        if (!isServer) return; //서버만 데이터에서 로딩, 클라는 network로 초기화
    }

    public void LoadTiles(IEnumerable<S2C.TileStatus> tiles)
    {
        if (tiles == null) return;
        foreach (S2C.TileStatus tile in tiles)
        {
            Tile newTile = new Tile(tile);
            m_tileManager.Add(newTile);
        }
    }

    public void LoadBuildings(IEnumerable<S2C.BuildingStatus> buildings)
    {
        if (buildings == null) return;
        foreach (S2C.BuildingStatus building in buildings)
        {
            Building newBuilding = new Building(building);
            m_buildingManager.Add(newBuilding);
        }
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

    

    public void Clear()
    {
        m_buildingManager.Clear();
        m_tileManager.Clear();
    }
}
}
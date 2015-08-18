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
    bool m_loadCompleted;

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
        ServerGame.Inst.CurrentMap = this;
    }

    public void SetMap(string mapName)
    {
        m_name = mapName;
    }

    public override void OnStartServer()
    {
        Debug.LogError("[Map] StartServer");
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.LogError("[Map] OnStartClient");
    }

    void Start()
    {
        Debug.LogError("[Map] Start");

        if (!isServer)
        {
            C2S.RequestMapInfo req = new C2S.RequestMapInfo();
            GameNetworkManager.Inst.client.Send((short)PacketType.RequestMapInfo, req);
        }
    }

    public void SendCurrentMapData(NetworkConnection conn)
    {
        S2C.MapInfo pck = new S2C.MapInfo();

        pck.m_mapName = m_mapData.name;
        pck.m_dirtyTiles = new List<S2C.TileStatus>();
        pck.m_dirtyBuildings = new List<S2C.BuildingStatus>();

        foreach (Tile tile in m_tileManager.GetDirtyStructures())
        {
            pck.m_dirtyTiles.Add(new S2C.TileStatus(tile));
        }
        foreach (Building building in m_buildingManager.GetDirtyStructures())
        {
            pck.m_dirtyBuildings.Add(new S2C.BuildingStatus(building));
        }

        NetworkServer.SendToClient(conn.connectionId, (short)PacketType.SendMapInfo, pck); 
    }

    public void ReceiveMapInfo(S2C.MapInfo mapInfo)
    {
        Debug.Log("[Client] received map info");

        Load(mapInfo);
    }

    void CompleteLoadMap()
    {
        m_mapLoadTime = Network.time;
        m_loadCompleted = true;

        ServerGame.Inst.OnMapLoadCompleted(this);
    }

    public bool Load(MapData mapData)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        m_mapData = mapData;
        m_width = mapData.mapWidth;
        m_height = mapData.mapHeight;
        m_name = mapData.mapName;
        m_chunkSize = mapData.chunkSize;

        Lights.ShadowPane.Inst.Init(m_width, m_height);
        LoadBackground(mapData.backgroundImage);

        LoadTiles(mapData.tiles);
        LoadBuildings(new List<Building>());

        if (!isServer)
        {
            Debug.Log("Remote Client will receive dirtied map data from server.");
            return true;
        }

        CompleteLoadMap();

        sw.Stop();
        Debug.Log(string.Format("Server Map Loading Time : {0} secs", sw.Elapsed.TotalSeconds));

        return true;
    }

    public void Load(S2C.MapInfo mapInfo)
    {
        MapData mapData = (MapData)Resources.Load("Maps/" + mapInfo.m_mapName);
        Load(mapData);

        LoadDirtyTiles(mapInfo.m_dirtyTiles);
        LoadDirtyBuildings(mapInfo.m_dirtyBuildings);

        CompleteLoadMap();
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

    public void LoadDirtyTiles(IEnumerable<S2C.TileStatus> tiles)
    {
        if (tiles == null) return;
        foreach (S2C.TileStatus dirtyTile in tiles)
        {
            Tile tile = m_tileManager.Get(dirtyTile.m_ID);
            tile.SetDirtyStatus(dirtyTile);
        }
    }

    public void LoadDirtyBuildings(IEnumerable<S2C.BuildingStatus> buildings)
    {
        if (buildings == null) return;
        foreach (S2C.BuildingStatus dirtyBuilding in buildings)
        {
            Building building = m_buildingManager.Get(dirtyBuilding.m_ID);
            building.SetDirtyStatus(dirtyBuilding);
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
        if (!m_loadCompleted) return true;

        return CheckInBorder(player.transform);
    }

    

    public void Clear()
    {
        m_buildingManager.Clear();
        m_tileManager.Clear();
    }
}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using S2C = Packet.S2C;
using C2S = Packet.C2S;

using Server;
using Const;
using Architecture;

namespace Maps
{ 
public class Map : MonoBehaviour {

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

        networkView.group = NetworkViewGroup.GAME;
        m_mapLoadTime = 0f;
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        S2C.MapInfo pck = new S2C.MapInfo(m_mapData.name, m_tileManager.GetTiles(), m_buildingManager.GetBuildings());
        networkView.RPC("RecvMap", player, pck.SerializeToBytes());
    }

    [RPC]
    void RecvMap(byte[] pckData, NetworkMessageInfo info)
    {
        //ServerCheck

        S2C.MapInfo pck = S2C.MapInfo.DeserializeFromBytes(pckData);

        Debug.Log("[Client] map instantiated by " + info.sender);

        Load(pck);

        ServerGame.Inst.OnMapLoadCompleted(this);
    }

    public void Load(S2C.MapInfo mapinfo)
    {
        Load(mapinfo.m_mapName);

        LoadTiles(mapinfo.m_tiles);
        LoadBuildings(mapinfo.m_buildings);
    }

    public void Load(string mapName)
    {
        MapData mapData = (MapData)Resources.Load("Maps/" + mapName);
        Load(mapData);

    }

    public void Load()
    {
        Load(m_mapData);
    }

    public void Load(MapData mapData)
    {
        m_width = mapData.mapWidth;
        m_height = mapData.mapHeight;
        m_name = mapData.mapName;
        m_chunkSize = mapData.chunkSize;

        Lights.ShadowPane.Inst.Init(m_width, m_height);
        LoadBackground(mapData.backgroundImage);

        

        if (!Network.isServer) return;
        LoadTiles(mapData.tiles);
        LoadBuildings(new List<Building>());



        m_mapLoadTime = Network.time;
    }
    
    public void LoadBackground(Sprite image)
    {
        m_backgroundPar = GetComponentInChildren<Parallax>();
        m_backgroundPar.SetImage(image, m_width, m_height);
    }

    public void LoadTiles(IEnumerable<Tile> tiles)
    {
        if (!Network.isServer) return; //서버만 데이터에서 로딩, 클라는 network로 초기화

        foreach (Tile tile in tiles)
        {
            m_tileManager.New(tile);
        }
    }

    public void LoadBuildings(IEnumerable<Building> buildings)
    {
        if (!Network.isServer) return; //서버만 데이터에서 로딩, 클라는 network로 초기화
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
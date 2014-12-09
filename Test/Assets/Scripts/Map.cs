using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Map : MonoBehaviour {


    private List<Tile> tileList = new List<Tile>();

    MapData mapData;

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (Network.isServer)
        {
            Debug.Log("[Server] map instantiated by " + info.sender);
            //Send Clients MapInfo;

            networkView.RPC("SetMapInfo", RPCMode.AllBuffered, Game.Instance.mapData.name);
        }

        Game.Instance.SetMap(this);
    }

    [RPC]
    void SetMapInfo(string mapName)
    {
        this.mapData = Resources.Load("Maps/" + mapName, typeof(MapData)) as MapData;
        this.Load(mapData);
    }

	// Use this for initialization
	void Start () {
	}

    public void AddTile(Tile tile, Vector3 pos)
    {
        tile.transform.parent = this.transform;
        tile.transform.position = pos;
        tileList.Add(tile);
    }

    public List<Tile> GetTileList()
    {
        return tileList;
    }

    public void Clear()
    {
        Fix();
        for (int i = 0; i < tileList.Count; ++i)
        {
            if(tileList[i] != null)
                DestroyImmediate(tileList[i].gameObject);
        }
        tileList.Clear();
    }

    public void Load(MapData mapData)
    {
        this.mapData = mapData;

        foreach (TileData tileData in mapData.tiles)
        {
            GameObject obj = Instantiate(mapData.tileSet.tiles[(int)tileData.tileType]) as GameObject;
            Tile tile = obj.GetComponent<Tile>();
            tile.health = tileData.health;

            AddTile(tile, new Vector3(tileData.x, tileData.y, 0));
        }
    }

    public void Fix()
    {
        Tile[] tiles = transform.GetComponentsInChildren<Tile>();
        tileList.Clear();
        for (int i = 0; i < tiles.Length; ++i)
        {
            tileList.Add(tiles[i]);
        }
    }
}

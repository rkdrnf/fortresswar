using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour {


    private List<Tile> tileList = new List<Tile>();

	int mapWidth;
	int mapHeight;
	const int tileSize = 8;

    public TileSet tileSet;

	// Use this for initialization
	void Start () {
	}


	public void drawMapNetwork(NetworkPlayer player)
	{
		networkView.RPC ("drawMap", player, "defaultMap");
	}

	[RPC]
	public void drawMap(string mapName)
	{
		//clear map;
		//draw map;
	}

    public void AddTile(Tile tile, Vector3 pos)
    {
        tile.transform.parent = this.transform;
        tile.transform.position = pos;
        tileList.Add(tile);
    }

    public override string ToString()
    {
        Fix();
        string output = tileList.Count.ToString() + "\n";
        for (int i = 0; i < tileList.Count; ++i)
        {
            if(tileList[i] != null)
                output += tileList[i].ToString() + "\n";
        }
        return output;
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

    public void Load(string mapData)
    {
        Clear();
        string[] lines = mapData.Split(new char[] { '\n' });

        int tileCount = int.Parse(lines[0]);

        for (int i = 0; i < tileCount; ++i)
        {
            string[] val = lines[i + 1].Split(new char[] { '\t' });

            float x = float.Parse(val[0]);
            float y = float.Parse(val[1]);
            int tileType = int.Parse(val[2]);
            int health = int.Parse(val[3]);

            GameObject obj = Instantiate(tileSet.tiles[tileType]) as GameObject;
            Tile tile = obj.GetComponent<Tile>();
            tile.health = health;

            AddTile(tile, new Vector3(x, y, 0));
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

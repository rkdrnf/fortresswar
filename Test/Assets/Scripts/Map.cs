using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {


	Tile[] tileList;

	int mapWidth;
	int mapHeight;
	const int tileSize = 8;

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
}

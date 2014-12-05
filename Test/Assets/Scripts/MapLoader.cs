using UnityEngine;
using System.Collections;
using System.IO;

public class MapLoader : MonoBehaviour {

    public GameObject mapPrefab;
    public TextAsset mapData;
    public TileSet tileSet;

	public Map GetMap()
	{
		Map map = (Instantiate(mapPrefab) as GameObject).GetComponent<Map>();
		map.mapName = mapData.name;
		map.tileSet = tileSet;
		map.Load(mapData.text);

		return map;
	}

	public Map GetMap(string mapName)
	{
		Map map = (Instantiate(mapPrefab) as GameObject).GetComponent<Map>();
		map.mapName = mapName;
		map.tileSet = tileSet;
		map.LoadByName ();

		return map;
	}

}

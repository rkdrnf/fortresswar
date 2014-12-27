using UnityEngine;
using System.Collections;
using System.IO;

public class MapLoader : MonoBehaviour {

    public GameObject mapPrefab;
    public TextAsset mapData;
    public TileSet tileSet;

	public GameObject GetMap()
	{
        return mapPrefab;
	}
}

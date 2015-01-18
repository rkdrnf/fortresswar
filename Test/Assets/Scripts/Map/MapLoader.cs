using UnityEngine;
using System.Collections;
using System.IO;
using Data;

public class MapLoader : MonoBehaviour {

    public GameObject mapPrefab;
    public TextAsset mapData;
    public TileSet tileSet;

	public GameObject GetMap()
	{
        return mapPrefab;
	}
}

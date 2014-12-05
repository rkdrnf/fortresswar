using UnityEngine;
using System.Collections;

public class MapLoader : MonoBehaviour {

    public GameObject mapPrefab;
    public TextAsset mapData;
    public TileSet tileSet;

    void Update()
    {
        if (Game.is_initialized)
        {
            if (!Game.is_map_loaded)
            {
                GameObject mapObject = Instantiate(mapPrefab) as GameObject;
                Game.map = mapObject.GetComponent<Map>();
                Game.map.tileSet = tileSet;
                Game.map.Load(mapData.text);
            }
        }
    }
}

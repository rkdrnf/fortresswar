using UnityEngine;
using System.Collections;

public class MapMaker : MonoBehaviour {

    public float tileSize = 1.0f;

    public int mapWidth = 320;
    public int mapHeight = 160;

	public GameObject tile;
	public TileSet tileSet;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos()
	{
		Vector3 pos = Camera.current.transform.position;

		Gizmos.color = new Color(180f, 180f, 180f, 90f);
        for (float y = pos.y - 50.0f; y < pos.y + 50.0f; y += tileSize)
		{
            Gizmos.DrawLine(new Vector3(-100.0f, Mathf.Floor(y / tileSize) * tileSize + 0.5f, 0.0f),
                            new Vector3(100.0f, Mathf.Floor(y / tileSize) * tileSize + 0.5f, 0.0f));
		}

        for (float x = pos.x - 50.0f; x < pos.x + 50.0f; x += tileSize)
		{
            Gizmos.DrawLine(new Vector3(Mathf.Floor(x / tileSize) * tileSize + 0.5f, -100.0f, 0.0f),
                            new Vector3(Mathf.Floor(x / tileSize) * tileSize + 0.5f, 100.0f, 0.0f));
		}
	}

    
}

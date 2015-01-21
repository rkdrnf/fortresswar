using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    
	void Update () {
	
	}

    [ContextMenu("Gen")]
    void Gen()
    {
        PolygonCollider2D col = GetComponent<PolygonCollider2D>();

        col.pathCount = 5;
        col.SetPath(0, new Vector2[3] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) });
        col.SetPath(1, new Vector2[3] { new Vector2(0, 1), new Vector2(0.5f, 1.5f), new Vector2(0, 2)});
        col.SetPath(2, new Vector2[3] { new Vector2(0, 2), new Vector2(0, 3),new Vector2(0.5f, 2.5f)});
        col.SetPath(3, new Vector2[3] { new Vector2(0, 3), new Vector2(0, 4f), new Vector2(1, 4) });
        col.SetPath(4, new Vector2[3] { new Vector2(1, 4), new Vector2(1.5f, 3.5f), new Vector2(2, 4)});
        
    }
}

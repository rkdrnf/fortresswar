using UnityEngine;
using System.Collections;

public class HealthBarBehaviour : MonoBehaviour {

    PlayerBehaviour player;

    Color fullColor;
    Color dyingColor;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        Vector2 targetPos;
        targetPos = Camera.main.WorldToScreenPoint(transform.position);

        GUI.Box(new Rect(targetPos.x, Screen.height - targetPos.y, 60, 20), "Health");
    }
}

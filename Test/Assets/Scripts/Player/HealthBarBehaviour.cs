using UnityEngine;
using System.Collections;

public class HealthBarBehaviour : MonoBehaviour {

    PlayerBehaviour player;

    public Texture2D texture; 

    Color fullColor;
    Color dyingColor;
	// Use this for initialization
	void Start () {
        player = transform.parent.gameObject.GetComponent<PlayerBehaviour>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        Vector2 targetPos;
        targetPos = Camera.main.WorldToScreenPoint(transform.position);

        GUI.DrawTexture(new Rect(targetPos.x - 20, Screen.height - targetPos.y - 10, player.health / 100 * 40, 5), texture, ScaleMode.StretchToFill, false); //displays a healthbar
    }
}

using UnityEngine;
using System.Collections;

public class HealthBarBehaviour : MonoBehaviour {

    PlayerBehaviour player;

    public Texture2D texture;

    public Color fullColor;
    public Color dyingColor;

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

        float healthRate = player.health / 100f;



        GUI.color = Blend(fullColor, dyingColor, healthRate);
        GUI.DrawTexture(new Rect(targetPos.x - 20, Screen.height - targetPos.y - 10, healthRate * 40, 5), texture, ScaleMode.StretchToFill, false); //displays a healthbar
        GUI.color = Color.white;
    }

    Color Blend(Color src, Color tar, float blend)
    {
        float r = tar.r * blend + src.r * (1 - blend);
        float g = tar.g * blend + src.g * (1 - blend);
        float b = tar.b * blend + src.b * (1 - blend);

        return new Color(r, g, b);
    }
}

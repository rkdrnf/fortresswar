using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Crosshair : MonoBehaviour
{
    PlayerBehaviour player;
    public int distance;

    public Texture2D crosshairTexture;

    // Use this for initialization
    void Start()
    {
        player = transform.parent.gameObject.GetComponent<PlayerBehaviour>();
    }
	
    void OnGUI()
    {
        if (player.IsMine())
            DrawCrosshair();
    }

    void DrawCrosshair()
    {
        Vector2 targetPos = Camera.main.WorldToScreenPoint(transform.position);
        targetPos = targetPos + player.lookingDirection.normalized * distance;
    
        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(targetPos.x - 2, Screen.height - targetPos.y - 2, 4, 4), crosshairTexture, ScaleMode.StretchToFill);
    }
}

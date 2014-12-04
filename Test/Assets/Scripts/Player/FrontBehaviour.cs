using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Util;
using Const;

public class FrontBehaviour : MonoBehaviour {

	public Facing facing;

	public LayerMask wallLayer;
	
	PlayerBehaviour player;
	
	HashSet<GameObject> contactingWalls;

    CharacterEnv walledEnv;

	// Use this for initialization
	void Start () {
		player = transform.parent.gameObject.GetComponent<PlayerBehaviour> ();
		
		contactingWalls = new HashSet<GameObject> ();
        walledEnv = facing == Facing.FRONT ? CharacterEnv.WALLED_FRONT : CharacterEnv.WALLED_BACK;
	}
	
	void OnTriggerEnter2D(Collider2D collider)
	{
		if (LayerUtil.HasLayer(collider.gameObject.layer, wallLayer)){
			contactingWalls.Add (collider.gameObject);
			
			//Debug.Log(string.Format("contacting walls: {0}", contactingWalls.Count));
			
			//No Operation when already walled front.
            if (player.IsInEnv(walledEnv))
				return;

            player.SetEnv(walledEnv, true);
		}
	}
	
	void OnTriggerExit2D(Collider2D collider)
	{
		if (LayerUtil.HasLayer (collider.gameObject.layer, wallLayer)) {
			contactingWalls.Remove (collider.gameObject);
			
			//Debug.Log(string.Format("contacting walls: {0}", contactingWalls.Count));
			
			//Set WALLED_FRONT false when no more contacting wallexists
			if (contactingWalls.Count == 0) {
                player.SetEnv(walledEnv, false);
			}
		}
	}
}

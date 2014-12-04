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

	// Use this for initialization
	void Start () {
		player = transform.parent.gameObject.GetComponent<PlayerBehaviour> ();
		
		contactingWalls = new HashSet<GameObject> ();
	}
	
	void OnTriggerEnter2D(Collider2D collider)
	{
		if (LayerUtil.HasLayer(collider.gameObject.layer, wallLayer)){
			contactingWalls.Add (collider.gameObject);
			
			//Debug.Log(string.Format("contacting walls: {0}", contactingWalls.Count));
			
			//No Operation when already grounded.
			if (player.IsInState (CharacterState.WALLED_FRONT))
				return;
			
			player.SetState(CharacterState.WALLED_FRONT, true);
		}
	}
	
	void OnTriggerExit2D(Collider2D collider)
	{
		if (LayerUtil.HasLayer (collider.gameObject.layer, wallLayer)) {
			contactingWalls.Remove (collider.gameObject);
			
			//Debug.Log(string.Format("contacting walls: {0}", contactingWalls.Count));
			
			//Set Grounded false when no more contacting ground exists.
			if (contactingWalls.Count == 0) {
				player.SetState (CharacterState.WALLED_FRONT, false);
			}
		}
	}
}

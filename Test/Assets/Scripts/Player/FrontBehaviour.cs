using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Util;
using Const;

public class FrontBehaviour : MonoBehaviour {

	public Facing facing;

	public LayerMask wallLayer;

    public Transform[] detectionPoints;
	
	PlayerBehaviour player;
	
	HashSet<GameObject> contactingWalls;

    Vector2[] rayDirections;
    float[] rayDistances;

    CharacterEnv walledEnv;

	// Use this for initialization
	void Start () {
		player = transform.parent.gameObject.GetComponent<PlayerBehaviour> ();
		
		contactingWalls = new HashSet<GameObject> ();
        walledEnv = facing == Facing.FRONT ? CharacterEnv.WALLED_FRONT : CharacterEnv.WALLED_BACK;
        rayDirections = new Vector2[detectionPoints.Length];
        rayDistances = new float[detectionPoints.Length];

        for(int i = 0; i < detectionPoints.Length; i++)
        {
            rayDirections[i] = new Vector2(detectionPoints[i].position.x - transform.position.x, detectionPoints[i].position.y - transform.position.y);
            rayDistances[i] = rayDirections[i].magnitude;
        }
	}

    void FixedUpdate()
    {
        for(int i = 0; i < detectionPoints.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirections[i], rayDistances[i], wallLayer);

            if (hit.collider != null && hit.normal.x == (facing == Facing.FRONT ? -1 : 1) && hit.normal.y == 0)
            {
                player.SetEnv(walledEnv, true);
                return;
            }
        }

        player.SetEnv(walledEnv, false);
        return;
    }

	
    /*
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
     * */
}

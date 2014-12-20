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

    Vector2[] rayRightDirections;
    Vector2[] rayLeftDirections;
    float[] rayDistances;

    CharacterEnv walledEnv;

	// Use this for initialization
	void Start () {
		player = transform.parent.gameObject.GetComponent<PlayerBehaviour> ();
		
		contactingWalls = new HashSet<GameObject> ();
        walledEnv = facing == Facing.FRONT ? CharacterEnv.WALLED_FRONT : CharacterEnv.WALLED_BACK;
        rayRightDirections = new Vector2[detectionPoints.Length];
        rayLeftDirections = new Vector2[detectionPoints.Length];
        rayDistances = new float[detectionPoints.Length];

        for(int i = 0; i < detectionPoints.Length; i++)
        {
            rayRightDirections[i] = new Vector2(detectionPoints[i].position.x - transform.position.x, detectionPoints[i].position.y - transform.position.y);
            rayLeftDirections[i] = new Vector2(-detectionPoints[i].position.x + transform.position.x, detectionPoints[i].position.y - transform.position.y);
            rayDistances[i] = rayRightDirections[i].magnitude;
        }
	}

    void FixedUpdate()
    {
        for(int i = 0; i < detectionPoints.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.facingRight ? rayRightDirections[i] : rayLeftDirections[i]), rayDistances[i], wallLayer);

            if (hit.collider != null && hit.normal.x == GetNormal(facing) && hit.normal.y == 0)
            {
                Debug.Log(walledEnv.ToString() + "Walled!");
                player.SetEnv(walledEnv, true);
                return;
            }
            else if (hit.collider != null)
            {
                Debug.Log(hit.normal);
            }
        }

        player.SetEnv(walledEnv, false);
        return;
    }

    int GetNormal(Facing detectorFacing)
    {
        if ((player.facingRight && detectorFacing == Facing.FRONT)
            || ((!player.facingRight) && detectorFacing == Facing.BACK))
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
}

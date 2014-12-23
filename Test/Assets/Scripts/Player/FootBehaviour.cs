using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Const;
using Util;

public class FootBehaviour : MonoBehaviour {

	public LayerMask groundLayer;

	public Transform[] detectionPoints;

	PlayerBehaviour player;

	HashSet<GameObject> contactingGrounds;

	Vector2[] rayDirections;
	float[] rayDistances;

	// Use this for initialization
	void Start () {
		player = transform.parent.gameObject.GetComponent<PlayerBehaviour> ();

		contactingGrounds = new HashSet<GameObject> ();
		rayDirections = new Vector2[detectionPoints.Length];
		rayDistances = new float[detectionPoints.Length];

		for(int i = 0; i < detectionPoints.Length; i++)
		{

			rayDirections[i] = new Vector2(detectionPoints[i].localPosition.x, detectionPoints[i].localPosition.y);
			rayDistances[i] = rayDirections[i].magnitude;
  		}
	}

	void FixedUpdate() {

		for(int i = 0; i < detectionPoints.Length; i++)
		{
			RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirections[i], rayDistances[i], groundLayer);

			//ray hit and normal = (0,1) then ground;
			if (hit.collider != null && hit.normal.x == 0 && hit.normal.y == 1)
			{
                player.OnTouchGround();
                return;
			}
  		}

        player.OnAwayFromGround();
	}

}

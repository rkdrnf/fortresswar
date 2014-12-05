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
				if (player.state == CharacterState.GROUNDED)
				{
					//Maintain State;
					return;
				}

				if (player.IsInState(CharacterState.FALLING, CharacterState.JUMPING_UP, CharacterState.WALL_JUMPING, CharacterState.WALL_WALKING))
				{
					Debug.Log("grounded!");
					player.state = CharacterState.GROUNDED;
					return;
				}
			}
			else if (hit.collider != null)
			{
				Debug.Log(hit.normal);
			}
  		}

		if (player.IsInState(CharacterState.GROUNDED))
		{
			player.state = CharacterState.FALLING;
			return;
		}
		
		if (player.IsInState(CharacterState.JUMPING_UP, CharacterState.WALL_WALKING, CharacterState.WALL_JUMPING, CharacterState.FALLING))
		{
			//Maintain state;
			return;
		}
	}



	//GroundCheck Using Trigger Collider. Deprecated.
	/*
	void OnTriggerEnter2D(Collider2D collider)
	{
		if (LayerUtil.HasLayer(collider.gameObject.layer, groundLayer)){
			contactingGrounds.Add (collider.gameObject);

			Debug.Log(string.Format("contacting grounds: {0}", contactingGrounds.Count));

			//No Operation when already grounded.

            do
            {
                if (player.state == CharacterState.GROUNDED)
                {
                    //Maintain State;
                    break;
                }

                if (player.IsInState(CharacterState.FALLING, CharacterState.JUMPING_UP, CharacterState.WALL_JUMPING, CharacterState.WALL_WALKING))
                {
                    Debug.Log("grounded!");
                    player.state = CharacterState.GROUNDED;
                    break;
                }
            } while (false);
            return;
		}
	}

	void OnTriggerExit2D(Collider2D collider)
	{
		if (LayerUtil.HasLayer (collider.gameObject.layer, groundLayer)) {
			contactingGrounds.Remove (collider.gameObject);

			//Debug.Log(string.Format("contacting grounds: {0}", contactingGrounds.Count));

			//Set Grounded false when no more contacting ground exists.
			if (contactingGrounds.Count == 0) {
                do
                {
                    if (player.IsInState(CharacterState.GROUNDED))
                    {
                        player.state = CharacterState.FALLING;
                        break;
                    }

                    if (player.IsInState(CharacterState.JUMPING_UP, CharacterState.WALL_WALKING, CharacterState.WALL_JUMPING, CharacterState.FALLING))
                    {
                        //Maintain state;
                        break;
                    }
                } while (false);
                return;
			}
		}
	}
	*/

}

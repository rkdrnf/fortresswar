using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Const;
using Util;

public class FootBehaviour : MonoBehaviour {

	public LayerMask groundLayer;

	PlayerBehaviour player;

	HashSet<GameObject> contactingGrounds;

	// Use this for initialization
	void Start () {
		player = transform.parent.gameObject.GetComponent<PlayerBehaviour> ();

		contactingGrounds = new HashSet<GameObject> ();
	}

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

}

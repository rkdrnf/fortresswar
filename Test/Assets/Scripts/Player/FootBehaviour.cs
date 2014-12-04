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
			if (player.IsInState (CharacterState.GROUNDED))
				return;

			player.SetState(CharacterState.GROUNDED, true);
		}
	}

	void OnTriggerExit2D(Collider2D collider)
	{
		if (LayerUtil.HasLayer (collider.gameObject.layer, groundLayer)) {
			contactingGrounds.Remove (collider.gameObject);

			Debug.Log(string.Format("contacting grounds: {0}", contactingGrounds.Count));

			//Set Grounded false when no more contacting ground exists.
			if (contactingGrounds.Count == 0) {
				player.SetState (CharacterState.GROUNDED, false);
			}
		}
	}

}

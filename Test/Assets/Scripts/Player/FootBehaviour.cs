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

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (LayerUtil.HasLayer(collision.gameObject.layer, groundLayer)){
			contactingGrounds.Add (collision.gameObject);

			Debug.Log(string.Format("contacting grounds: {0}", contactingGrounds.Count));

			//No Operation when already grounded.
			if (player.IsInState (CharacterState.GROUNDED))
				return;

			player.SetState(CharacterState.GROUNDED, true);
		}
	}

	void OnCollisionStay2D(Collision2D collision)
	{
		if (LayerUtil.HasLayer(collision.gameObject.layer, groundLayer)) {
			player.SetState(CharacterState.GROUNDED, true);
		}
	}

	void OnCollisionExit2D(Collision2D collision)
	{
		if (LayerUtil.HasLayer (collision.gameObject.layer, groundLayer)) {
			contactingGrounds.Remove (collision.gameObject);

			Debug.Log(string.Format("contacting grounds: {0}", contactingGrounds.Count));

			//Set Grounded false when no more contacting ground exists.
			if (contactingGrounds.Count == 0) {
				player.SetState (CharacterState.GROUNDED, false);
			}
		}
	}

}

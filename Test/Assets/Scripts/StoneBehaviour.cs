using UnityEngine;
using System.Collections;

public class StoneBehaviour : Tile  {

	// Use this for initialization
	void Awake () {
		this.anim = GetComponent<Animator> ();

		this.health = 100;
	}
}

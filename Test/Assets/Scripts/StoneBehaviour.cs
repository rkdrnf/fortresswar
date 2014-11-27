using UnityEngine;
using System.Collections;

public class StoneBehaviour : MonoBehaviour {

	int health;

	Animator anim;

	// Use this for initialization
	void Start () {
		this.anim = GetComponent<Animator> ();

		this.health = 100;
	
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		anim.SetInteger ("Health", health);
		if (health < 1)
						Destroy (gameObject);
	}


	public void Damage(int damage)
	{
		health -= damage;
	}
}

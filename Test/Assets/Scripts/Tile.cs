using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	protected bool destroyable;
	protected int health;
	protected Animator anim;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Damage(int damage)
	{
		if (destroyable) {
						health -= damage;
						anim.SetInteger ("Health", health);
						if (health < 1)
								Destroy (gameObject);
				}
	}
}

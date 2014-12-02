using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class Tile : MonoBehaviour {

	public bool destroyable;
	public int health;
	public Animator anim;

	// Use this for initialization
	void Start () {
		anim.SetInteger ("Health", health);
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

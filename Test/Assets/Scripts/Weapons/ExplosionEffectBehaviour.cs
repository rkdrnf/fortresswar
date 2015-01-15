using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class ExplosionEffectBehaviour : MonoBehaviour {

    public bool standing;
    public float animationTime;
    public Vector2 offset;

	// Use this for initialization
	void Start () {
        if (standing)
            transform.rotation = Quaternion.identity;

        transform.position = (Vector2)transform.position + offset;
        Destroy(this.gameObject, animationTime);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

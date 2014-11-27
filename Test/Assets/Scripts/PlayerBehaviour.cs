using UnityEngine;
using System.Collections;

public class PlayerBehaviour : MonoBehaviour {


	public GameObject projectile;

	const int MOVE_SPEED = 4;

	const float FIRE_RATE = 0.2f;
	const int FIRE_POWER = 20;

	bool facingRight = true;

	float timer;

	// Use this for initialization
	void Start () {
		timer = 0f;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float move = Input.GetAxis ("Horizontal") * MOVE_SPEED;

		rigidbody2D.velocity = new Vector2 (move, rigidbody2D.velocity.y);

		if (move < 0 && facingRight) {
			Flip();

				}
		if (move > 0 && !facingRight) {
			Flip();
				}

		if (Input.GetButton ("Fire1")) {

			timer += Time.deltaTime;
			if (timer > FIRE_RATE)
			{
				timer = 0f;
				GameObject bullet = Instantiate(projectile, transform.position, transform.rotation) as GameObject;

				Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, transform.position.z));
				Vector3 direction = worldMousePosition - transform.position;
		
				bullet.GetComponent<GunBullet>().Fire(direction, 20);
			}
		}
	}

	void Flip() {
		facingRight = !facingRight;
		Vector3 scale = transform.localScale;
		scale.x = -scale.x;
		transform.localScale = scale;
	}
}

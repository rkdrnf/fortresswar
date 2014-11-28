using UnityEngine;
using System.Collections;

public class PlayerBehaviour : MonoBehaviour {

	public GameObject projectile;
	public GameObject netManager;

	const int MOVE_SPEED = 4;

	const float FIRE_RATE = 0.2f;
	const int FIRE_POWER = 20;

	bool facingRight = true;



	NetworkViewID viewID;

	float timer;

	Vector3 pos;
	Quaternion rot;
	int m;


	void Awake()
	{
		timer = 0f;
		netManager = GameObject.Find ("NetworkManager");
	}

	// Use this for initialization
	void Start () {

	}

	public void SetPlayerViewID(NetworkViewID networkViewID)
	{
		viewID = networkViewID;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (viewID != netManager.networkView.viewID)
					return;

		float move = Input.GetAxis ("Horizontal") * MOVE_SPEED;

		if (move != 0) {
			rigidbody2D.velocity = new Vector2 (move, rigidbody2D.velocity.y);
		}

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

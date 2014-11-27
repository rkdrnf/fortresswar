using UnityEngine;
using System.Collections;

public class GunBullet : MonoBehaviour {

	const int DAMAGE = 20;

	const int RANGE = 4;

	Vector3 startPosition;
	Vector3 currentPosition;

	// Use this for initialization
	void Start () {
		startPosition = transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		currentPosition = transform.position;

		if ((currentPosition - startPosition).sqrMagnitude > RANGE * RANGE) {
			Destroy(gameObject);
				}
	
	}

	public void Fire(Vector3 direction, int power)
	{
		float hypotenuse = Mathf.Sqrt ((direction.x * direction.x) + (direction.y * direction.y));
		float powerRateX = direction.x / hypotenuse;
		float powerRateY = direction.y / hypotenuse;

		rigidbody2D.AddForce(new Vector2(powerRateX * power, powerRateY * power), ForceMode2D.Impulse);
	}

	void OnTriggerEnter2D(Collider2D targetCollider)
	{
		if (targetCollider.gameObject.CompareTag ("Tile")) {

			GameObject tile = targetCollider.gameObject;
			tile.GetComponent<StoneBehaviour>().Damage(DAMAGE);

			Destroy(gameObject);
		}
		else
		{
			return;
		}
	}


}

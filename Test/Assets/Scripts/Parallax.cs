using UnityEngine;
using System.Collections;

public class Parallax : MonoBehaviour {

    public int distance;
    public const int observingDistance = 20;

    Transform target;

    Vector2 targetPosOld;

    Sprite image;

    float multiplier;

    Vector2 displacement;

    void Awake()
    {
        target = Camera.main.transform;
        targetPosOld = target.position;
        displacement = Vector2.zero;
        multiplier = (float)observingDistance / (distance + observingDistance);
        Debug.Log("Multiplier: " + multiplier);
    }

	// Update is called once per frame
	void Update () {

        Vector2 targetPosNew = target.position;
        Vector2 moveDistance = (targetPosNew - targetPosOld);

        displacement += (moveDistance * multiplier);
        transform.position = moveDistance - displacement;

        targetPosOld = targetPosNew;
	}

    public void SetImage(Sprite image, float width, float height)
    {
        this.image = image;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = image;

        
        //transform.localScale = new Vector2(width * multiplier / image.bounds.size.x, height * multiplier / image.bounds.size.y);
    }
}

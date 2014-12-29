using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

    float height = 0.3f;
    float heightDamping = 8f;

    public Transform target;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {

	}

    void LateUpdate()
    {
        if (target != null)
        {
            float wantedHeight = target.position.y + height;

            float currentHeight = transform.position.y;

            // Damp the height
            //

            // Set the position of the camera on the x-z plane to:
            // Set the height of the camera
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
            transform.position = new Vector3(target.transform.position.x, currentHeight, -20);


            // Always look at the target
            //transform.LookAt(target);
        }
    }
}

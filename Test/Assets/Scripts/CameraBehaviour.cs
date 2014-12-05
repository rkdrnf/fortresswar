﻿using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

    float height = 0.3f; 

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
            //currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            transform.position = new Vector3(target.position.x, target.position.y, -20);

            // Set the height of the camera
        //transform.position = new Vector3(transform.position.x, currentHeight, -20);


            // Always look at the target
            //transform.LookAt(target);
        }
    }
}

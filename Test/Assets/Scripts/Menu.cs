using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	public void StartServer()
	{
		Network.InitializeServer (16, 25000, !Network.HavePublicAddress());

	}

	void OnServerInitialized()
	{
		Debug.Log ("Server Initialized!");
	}

	public void Refresh()
	{
		Debug.Log ("Refreshed!");
	}


	void OnGUI()
	{
		/*
		if (GUI.Button(new Rect(transform.position.x - 10, transform.position.y - 10, 150, 100), "Host Server"))
			print("Host");

		if (GUI.Button(new Rect(transform.position.x - 10, transform.position.y - 20, 150, 100), "Refresh"))
			print("Refresh");
			*/
	}

	void Update()
	{
		if (Input.GetButtonUp ("Cancel"))
		{
			Camera menuCamera = GameObject.Find("MenuCamera").camera;
			menuCamera.enabled = !menuCamera.enabled;


		}
	}
}

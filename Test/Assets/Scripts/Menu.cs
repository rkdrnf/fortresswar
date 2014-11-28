using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	public GameObject netManagerObj;


	NetworkManager netManager;

	void Awake()
	{
		netManager = netManagerObj.GetComponent<NetworkManager> ();
	}

	void Start()
	{
	}

	public void RefreshServerList()
	{
		netManager.RefreshHostList ();
	}

	public void StartServer()
	{
		netManager.StartServer ();
	}

	void OnGUI()
	{
		HostData[] hostList = netManager.GetServerList ();

		if (hostList.Length > 0)
		{
			for (int i = 0; i < hostList.Length; i++) 
			{
				if (GUI.Button(new Rect(-1, 1, 100, 50), hostList[i].gameName))
				{
					var cError = netManager.Connect(hostList[i]);

					Debug.Log(cError);
				}
			}
		}
	}

	void Update()
	{
		if (Input.GetButtonUp ("Cancel"))
		{
			Camera menuCamera = GameObject.Find("MenuCamera").camera;
			menuCamera.enabled = !menuCamera.enabled;

			netManager.ClearServerList();
		}
	}
}

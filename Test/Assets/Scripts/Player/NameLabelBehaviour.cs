using UnityEngine;
using System.Collections;

public class NameLabelBehaviour : MonoBehaviour {

    PlayerBehaviour player;

	// Use this for initialization
	void Start () {
        player = transform.parent.gameObject.GetComponent<PlayerBehaviour>();
	}
	
	// Update is called once per frame
	void Update () {
        TextMesh label = GetComponent<TextMesh>();

        label.text = player.state.ToString();
	}
}

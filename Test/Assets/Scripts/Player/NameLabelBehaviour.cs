using UnityEngine;
using System.Collections;

public class NameLabelBehaviour : MonoBehaviour {

    PlayerBehaviour player;
    public TextMesh textMesh;

	// Use this for initialization
	void Start () {
        player = transform.parent.gameObject.GetComponent<PlayerBehaviour>();
	}
	
	// Update is called once per frame
    void Update()
    {
        transform.localScale = player.transform.localScale;
        textMesh.text = player.GetSetting().name + "\n" + player.state.ToString();
    }
}

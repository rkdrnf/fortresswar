using UnityEngine;
using System.Collections;

public class NameLabelBehaviour : MonoBehaviour {

    PlayerBehaviour player;
    PlayerSetting setting;
    public TextMesh textMesh;

	// Use this for initialization
	void Start () {
        setting = null;
        player = transform.parent.gameObject.GetComponent<PlayerBehaviour>();
	}
	
	// Update is called once per frame
    void Update()
    {
        if (setting == null)
        {
            PlayerSetting newSetting = PlayerManager.Inst.GetSetting(player.GetOwner());
            if (newSetting == null)
                return;

            setting = newSetting;
        }

        transform.localScale = player.transform.localScale;
        textMesh.text = setting.name + "\n" + player.state.ToString();
    }
}

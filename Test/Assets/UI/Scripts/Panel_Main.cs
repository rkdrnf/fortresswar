using UnityEngine;
using System.Collections;

public class Panel_Main : MonoBehaviour {

    public Menu menu;
    public UIButton Btn_StartServer;
    public UIButton Btn_RefreshServer;

    public void BtnFunc_StartServer()
    {
        Debug.Log("Click");
        menu.StartServer();
        gameObject.SetActive(false);
    }

    public void BtnFunc_RefreshServer()
    {
        menu.RefreshServerList();
    }
}

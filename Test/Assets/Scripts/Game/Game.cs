//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.18444
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Const;

using S2C = Packet.S2C;
using C2S = Packet.C2S;

using ProtoBuf.Meta;
using FocusManager;


public class Game : MonoBehaviour
{
    private static Game instance;

	public static Game Inst
	{
		get
		{
			if (instance == null) {
				instance = new Game ();
			}
			return instance;
		}
	}

    public static bool IsInitialized() { return instance != null; }

	public GameObject netManagerObject;
    public JobSet jobSet;
    public MapData mapData;
    public WeaponSet weaponSet;

    public Map map;
	NetworkManager netManager;
	MapLoader mapLoader;

    void Init()
    {
        instance = this;
        /*
        RuntimeTypeModel.Default.Add(typeof(Vector3), true);
        RuntimeTypeModel.Default.Add(typeof(Vector2), true);
         */
    }

	void Awake()
	{
        Init();
		netManager = netManagerObject.GetComponent<NetworkManager> ();
		mapLoader = GetComponent<MapLoader> ();
	}

	public void LoadMap()
	{
        GameObject mapPrefab = mapLoader.GetMap();

        GameObject mapObj = (GameObject)Network.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity, NetworkViewGroup.GAME);
	}

    public void ClearMap()
    {
        map = null;
    }

    public void SetMap(Map map)
    {
        this.map = map;
    }
}
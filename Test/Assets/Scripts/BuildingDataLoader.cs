using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

public class BuildingDataLoader : MonoBehaviour
{
    private static BuildingDataLoader instance;

    public static BuildingDataLoader Inst
    {
        get
        {
            if (instance == null)
            {
                instance = new BuildingDataLoader();
            }
            return instance;
        }
    }

    void Update()
    {
        if(Input.GetButton("Fire1"))
        {
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            GameObject obj = Map.GetLayerObjectAt(worldMousePosition, buildingCollidingLayers);

            /*
            if(obj != null && obj.GetComponent<Building>() != null)
            {
                obj.GetComponent<Building>().LogForDebug();
            }
             * */
        }
    }

    void Awake()
    {
        instance = this;

        FillScript();

        buildingLayer = LayerMask.NameToLayer("Building");
        fallingBuildingLayer = LayerMask.NameToLayer("FallingBuilding");

        Physics2D.IgnoreLayerCollision(fallingBuildingLayer, fallingBuildingLayer);
    }


    public BuildingDataSet buildingSet;
    private Dictionary<string, BuildingData> buildingNameMap;
    [HideInInspector]
    public int buildingLayer;
    [HideInInspector]
    public int fallingBuildingLayer;
    public LayerMask buildingCollidingLayers;
    public LayerMask fallingBuildingCollidingLayers;

    void FillScript()
    {
        buildingNameMap = new Dictionary<string, BuildingData>();
        foreach (BuildingData data in buildingSet.buildings)
        {
            buildingNameMap.Add(data.buildingName, data);
        }
    }

    public BuildingData GetBuilding(string name)
    {
        return buildingNameMap[name];
    }
}

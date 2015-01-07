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

    void Awake()
    {
        instance = this;

        FillScript();

        buildingLayer = LayerMask.NameToLayer("Building");
        fallingBuildingLayer = LayerMask.NameToLayer("FallingBuilding");
    }


    public BuildingDataSet buildingSet;
    private Dictionary<string, BuildingData> buildingNameMap;
    public int buildingLayer;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const.Structure;
using Data;

public class BuildingData : StructureData
{
    readonly public BuildingType type;
    readonly public Texture2D image;
    readonly public LayerMask invalidLocations;

    public string buildingName;
    public Building building;
    
     
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const.Structure;
using Data;

public class BuildingData : StructureData
{
    public BuildingType type;
    public Texture2D image;
    public LayerMask invalidLocations;

    public string buildingName;
}

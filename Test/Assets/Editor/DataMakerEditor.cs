using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Data;
using System.IO;


public class DataMakerEditor : Editor 
{
    [MenuItem("Assets/Create/Tile")]
    static void CreateTile()
    {
        CreateAsset<TileData>();
    }

    [MenuItem("Assets/Create/Set/TileSet")]
    static void CreateTileSet()
    {
        CreateAsset<TileSet>();
    }

    [MenuItem("Assets/Create/Set/WeaponSet")]
    static void CreateWeapon()
    {
        CreateAsset<WeaponSet>();
    }

    [MenuItem("Assets/Create/Weapon")]
    static void CreateWeaponSet()
    {
        CreateAsset<WeaponData>();
    }

    [MenuItem("Assets/Create/Set/JobSet")]
    static void CreateJobSet()
    {
        CreateAsset<JobSet>();
    }

    [MenuItem("Assets/Create/Job")]
    static void CreateJob()
    {
        CreateAsset<JobStat>();
    }

    [MenuItem("Assets/Create/Particle2D")]
    static void CreateParticle2D()
    {
        CreateAsset<ParticleSystem2DData>();
    }

    [MenuItem("Assets/Create/Set/Particle2DSet")]
    static void CreateParticle2DSet()
    {
        CreateAsset<ParticleSystem2DSet>();
    }

    [MenuItem("Assets/Create/Skill")]
    static void CreateSkill()
    {
        CreateAsset<SkillData>();
    }

    [MenuItem("Assets/Create/Set/SkillSet")]
    static void CreateSkillSet()
    {
        CreateAsset<SkillDataSet>();
    }

    [MenuItem("Assets/Create/Set/Building")]
    static void CreateBuilding()
    {
        CreateAsset<BuildingData>();
    }

    [MenuItem("Assets/Create/Set/BuildingSet")]
    static void CreateBuildingSet()
    {
        CreateAsset<BuildingDataSet>();
    }

    [MenuItem("Assets/Create/Set/MaterialSet")]
    static void CreateMaterialSet()
    {
        CreateAsset<MaterialSet>();
    }

    [MenuItem("Assets/Create/AnimationEffect")]
    static void CreateAnimationEffect()
    {
        CreateAsset<AnimationEffectData>();
    }

    [MenuItem("Assets/Create/Set/AnimationEffectSet")]
    static void CreateAnimationEffectSet()
    {
        CreateAsset<AnimationEffectDataSet>();
    }

    [MenuItem("Assets/Create/LightEffect")]
    static void CreateLightEffect()
    {
        CreateAsset<LightEffectData>();
    }

    [MenuItem("Assets/Create/Set/LightEffectSet")]
    static void CreateLightEffectSet()
    {
        CreateAsset<LightEffectDataSet>();
    }

    static void CreateAsset<T>() where T : ScriptableObject
    {
        T asset = CreateInstance<T>();
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path.Replace(Path.GetFileName(path), "");
        }
        else
        {
            path += Path.DirectorySeparatorChar;
        }

        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + typeof(T).ToString() + ".asset");
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }
}

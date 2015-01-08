using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(MapMaker))]
public class MapMakerEditor : Editor {

    Map map;
	MapMaker maker;

	int tileIndex = 0;
    string mapFileName;
    MapData mapFileAsset;

	void OnEnable()
	{
		maker = target as MapMaker;
        map = maker.transform.GetComponentInChildren<Map>();
	}

	void OnSceneGUI()
	{
        
		Event e = Event.current;

		Vector3 worldMousePos = Camera.current.ScreenToWorldPoint (new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight, 0f));

		Vector2 mousePos = new Vector2(worldMousePos.x, worldMousePos.y);
		RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        
		
		if (e.isKey && e.character == 'a') {
			
			if(hit.collider != null)
			{
				string[] invalidLocations = {"Tile", "Player"};
				if(ArrayUtility.Contains(invalidLocations, hit.transform.gameObject.tag))
				{
					Debug.Log("Unable to locate tile");
					return;
				}
			}
			
			PrefabType prefab = PrefabUtility.GetPrefabType(maker.tile);
			
			if (prefab == PrefabType.Prefab)
			{
				GameObject tileObj = (GameObject)PrefabUtility.InstantiatePrefab(maker.tile);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.coord = new GridCoord(Mathf.FloorToInt((mousePos.x + 0.5f) / maker.tileSize) * maker.tileSize, Mathf.FloorToInt((mousePos.y + 0.5f) / maker.tileSize) * maker.tileSize);
                tile.transform.position = tile.coord.ToVector2();

                maker.tiles.Add(tile);
			}
		}

        if (e.isKey && e.character == 'g')
        {
            maker.ToggleGrid();
        }
	}


	public override void OnInspectorGUI()
	{
        if (map == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Map Object");
            map = (Map)EditorGUILayout.ObjectField(map, typeof(Map), false);
            GUILayout.EndHorizontal();
            return;
        }

		GUILayout.BeginHorizontal();
		GUILayout.Label(" Tile Size ");
        maker.tileSize = EditorGUILayout.IntField(maker.tileSize, GUILayout.Width(100f));
		GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Map Width ");
        maker.mapWidth = EditorGUILayout.IntField(maker.mapWidth, GUILayout.Width(100f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Map Height ");
        maker.mapHeight = EditorGUILayout.IntField(maker.mapHeight, GUILayout.Width(100f));
        GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(" TileSet ");
		maker.tileSet = (TileSet)EditorGUILayout.ObjectField (maker.tileSet, typeof(TileSet), false, GUILayout.Width(150f));
		GUILayout.EndHorizontal();

		if (maker.tileSet != null) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(" Tile ");

			var index = EditorGUILayout.IntPopup ("Select Tile", tileIndex
                 , maker.tileSet.tiles.Select (t => t != null ? t.name : "").ToArray ()
                 , maker.tileSet.tiles.Select (t => ArrayUtility.IndexOf (maker.tileSet.tiles, t)).ToArray ()
			);
			if (index != tileIndex && maker.tileSet.tiles.Length > 0) {
					tileIndex = index;

					maker.tile = maker.tileSet.tiles [tileIndex];

					maker.tileSize = Mathf.FloorToInt(maker.tile.renderer.bounds.size.x);

					//move focus to first sceneview
					if (SceneView.sceneViews.Count > 0) { SceneView sceneView = (SceneView)SceneView.sceneViews[0]; sceneView.Focus(); }
			}
				
			GUILayout.EndHorizontal();
		}

        GUILayout.BeginHorizontal();
        GUILayout.Label("Background");
        maker.backgroundImage = (Sprite)EditorGUILayout.ObjectField(maker.backgroundImage, typeof(Sprite), false, GUILayout.Width(150f));
        
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        mapFileAsset = (MapData)EditorGUILayout.ObjectField(mapFileAsset, typeof(MapData), false, GUILayout.Width(100));
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        if (GUILayout.Button("Load Map", GUILayout.MinWidth(100)))
        {
            LoadMap();
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        mapFileName = EditorGUILayout.TextField(mapFileName, GUILayout.Width(100));
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        if (GUILayout.Button("Save Map", GUILayout.MinWidth(100)))
        {
            SaveMap();
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Map", GUILayout.MinWidth(100)))
        {
            Clear();
        }
        if (GUILayout.Button("Fix Map", GUILayout.MinWidth(100)))
        {
            Fix();
        }
        if (GUILayout.Button("Apply", GUILayout.MinWidth(100)))
        {
            Apply();
        }
        GUILayout.EndHorizontal();

		SceneView.RepaintAll ();
	}


	[MenuItem("Assets/Create/TileSet")]
	static void CreateTileSet()
	{
        CreateAsset<TileSet>();
	}

    [MenuItem("Assets/Create/WeaponSet")]
    static void CreateWeapon()
    {
        CreateAsset<WeaponSet>();
    }

    [MenuItem("Assets/Create/Weapon")]
    static void CreateWeaponSet()
    {
        CreateAsset<WeaponData>();
    }

    [MenuItem("Assets/Create/JobSet")]
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

    [MenuItem("Assets/Create/Particle2DSet")]
    static void CreateParticle2DSet()
    {
        CreateAsset<ParticleSystem2DSet>();
    }

    [MenuItem("Assets/Create/Skill")]
    static void CreateSkill()
    {
        CreateAsset<SkillData>();
    }

    [MenuItem("Assets/Create/SkillSet")]
    static void CreateSkillSet()
    {
        CreateAsset<SkillDataSet>();
    }

    [MenuItem("Assets/Create/Building")]
    static void CreateBuilding()
    {
        CreateAsset<BuildingData>();
    }

    [MenuItem("Assets/Create/BuildingSet")]
    static void CreateBuildingSet()
    {
        CreateAsset<BuildingDataSet>();
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

    public void LoadMap()
    {
        map.Load(mapFileAsset);
    }

    public void SaveMap()
    {
        Apply();

        string filePath = AssetDatabase.GenerateUniqueAssetPath(string.Format("Assets/Resources/Maps/{0}.asset", mapFileName));

        MapData mapAsset = CreateInstance<MapData>();
        mapAsset.init(mapFileName, maker.mapWidth, maker.mapHeight, maker.tileSize, maker.tileSet, maker.tiles, maker.backgroundImage);

        //mapAsset.hideFlags = HideFlags.NotEditable;

        AssetDatabase.CreateAsset(mapAsset, filePath);
        AssetDatabase.SaveAssets();

        Selection.activeObject = mapAsset;
        EditorGUIUtility.PingObject(mapAsset);

        Debug.Log("Map File Saved");
    }

    public void Clear()
    {
        Fix();
        foreach (var tile in map.GetTileList())
        {
            if (tile.Value != null)
                DestroyImmediate(tile.Value.gameObject);
        }
        map.Clear();
    }

    public void Fix()
    {
        Tile[] tiles = map.GetComponentsInChildren<Tile>();
        map.Clear();
        for (int i = 0; i < tiles.Length; ++i)
        {
            map.GetTileList().Add(tiles[i].coord, tiles[i]);
        }
    }

    public void Apply()
    {
        map.OnApply(maker.backgroundImage, maker.mapWidth, maker.mapHeight);
    }
}

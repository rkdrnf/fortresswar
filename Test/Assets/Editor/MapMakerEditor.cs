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
                tile.transform.position = new Vector3(Mathf.Floor((mousePos.x + 0.5f) / maker.tileSize) * maker.tileSize, Mathf.Floor((mousePos.y + 0.5f) / maker.tileSize) * maker.tileSize, 0f);

                tile.ID = GetTileIndex(tile);
                if (tile != null)
                {
                    map.AddTile(tile);
                }
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
        maker.tileSize = EditorGUILayout.FloatField(maker.tileSize, GUILayout.Width(100f));
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

					maker.tileSize = maker.tile.renderer.bounds.size.x;

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
		TileSet tileSet = CreateInstance<TileSet> ();
		var path = AssetDatabase.GetAssetPath (Selection.activeObject);

		if (string.IsNullOrEmpty (path)) {
			path = "Assets";
		} else if (Path.GetExtension (path) != "")
		{
			path.Replace (Path.GetFileName (path), "");
		}
		else
		{
			path += Path.DirectorySeparatorChar;
		}

		var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "TileSet.asset");
		AssetDatabase.CreateAsset (tileSet, assetPathAndName);
		AssetDatabase.SaveAssets ();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = tileSet;
	}

    [MenuItem("Assets/Create/WeaponSet")]
    static void CreateWeapon()
    {
        WeaponSet projSet = CreateInstance<WeaponSet>();
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

        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "WeaponSet.asset");
        AssetDatabase.CreateAsset(projSet, assetPathAndName);
        AssetDatabase.SaveAssets();
        Selection.activeObject = projSet;
        EditorGUIUtility.PingObject(projSet);
    }

    [MenuItem("Assets/Create/Weapon")]
    static void CreateWeaponSet()
    {
        WeaponData projSet = CreateInstance<WeaponData>();
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

        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "Weapon.asset");
        AssetDatabase.CreateAsset(projSet, assetPathAndName);
        AssetDatabase.SaveAssets();
        Selection.activeObject = projSet;
        EditorGUIUtility.PingObject(projSet);
    }

    [MenuItem("Assets/Create/JobSet")]
    static void CreateJobSet()
    {
        JobSet jobSet = CreateInstance<JobSet>();
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

        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "JobSet.asset");
        AssetDatabase.CreateAsset(jobSet, assetPathAndName);
        AssetDatabase.SaveAssets();
        Selection.activeObject = jobSet;
        EditorGUIUtility.PingObject(jobSet);
    }

    [MenuItem("Assets/Create/Job")]
    static void CreateJob()
    {
        JobStat job = CreateInstance<JobStat>();
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

        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "Job.asset");
        AssetDatabase.CreateAsset(job, assetPathAndName);
        AssetDatabase.SaveAssets();
        Selection.activeObject = job;
        EditorGUIUtility.PingObject(job);
    }

    [MenuItem("Assets/Create/Particle2D")]
    static void CreateParticle2D()
    {
        ParticleSystem2DData particle = CreateInstance<ParticleSystem2DData>();
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

        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "Particle.asset");
        AssetDatabase.CreateAsset(particle, assetPathAndName);
        AssetDatabase.SaveAssets();
        Selection.activeObject = particle;
        EditorGUIUtility.PingObject(particle);
    }

    [MenuItem("Assets/Create/Particle2DSet")]
    static void CreateParticle2DSet()
    {
        ParticleSystem2DSet particle = CreateInstance<ParticleSystem2DSet>();
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

        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "ParticleSet.asset");
        AssetDatabase.CreateAsset(particle, assetPathAndName);
        AssetDatabase.SaveAssets();
        Selection.activeObject = particle;
        EditorGUIUtility.PingObject(particle);
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
        mapAsset.init(mapFileName, maker.mapWidth, maker.mapHeight, maker.tileSize, maker.tileSet, map.GetTileList(), maker.backgroundImage);

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
            map.GetTileList().Add(GetTileIndex(tiles[i]), tiles[i]);
        }
    }

    public void Apply()
    {
        map.OnApply(maker.backgroundImage, maker.mapWidth, maker.mapHeight);
    }

    public int GetTileIndex(Tile tile)
    {
        return Mathf.FloorToInt(tile.transform.localPosition.x) + Mathf.FloorToInt(tile.transform.localPosition.y) * maker.mapWidth;
    }
}

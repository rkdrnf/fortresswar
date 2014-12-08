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
    TextAsset mapFileAsset;

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
				GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(maker.tile);
                if (tile.GetComponent<Tile>() != null)
                {
                    map.AddTile(tile.GetComponent<Tile>(), new Vector3(Mathf.Floor((mousePos.x + 0.5f) / maker.width) * maker.width, Mathf.Floor((mousePos.y + 0.5f) / maker.height) * maker.height, 0f));
                }
			}
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
		GUILayout.Label(" Grid Width ");
		maker.width = EditorGUILayout.FloatField(maker.width, GUILayout.Width(100f));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(" Grid Height ");
        maker.height = EditorGUILayout.FloatField(maker.height, GUILayout.Width(100f));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(" TileSet ");
		maker.tileSet = (TileSet)EditorGUILayout.ObjectField (maker.tileSet, typeof(TileSet), false, GUILayout.Width(150f));
        map.tileSet = maker.tileSet;
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

					maker.width = maker.tile.renderer.bounds.size.x;
					maker.height = maker.tile.renderer.bounds.size.y;


					//move focus to first sceneview
					if (SceneView.sceneViews.Count > 0) { SceneView sceneView = (SceneView)SceneView.sceneViews[0]; sceneView.Focus(); }
			}
				
			GUILayout.EndHorizontal();
		}

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        mapFileAsset = (TextAsset)EditorGUILayout.ObjectField(mapFileAsset, typeof(TextAsset), false, GUILayout.Width(100));
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
            map.Clear();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fix Map", GUILayout.MinWidth(100)))
        {
            map.Fix();
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
    static void CreateWeaponSet()
    {
        ProjectileSet projSet = CreateInstance<ProjectileSet>();
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

    public void LoadMap()
    {
        map.Load(mapFileAsset.text);
    }

    public void SaveMap()
    {
        string filePath = AssetDatabase.GenerateUniqueAssetPath(string.Format("Assets{0}Resources{0}Maps{0}{1}.txt", Path.DirectorySeparatorChar, mapFileName));

        using (StreamWriter sw = File.CreateText(filePath))
        {
            sw.Write(map.ToString());
        }
        Debug.Log("Map File Saved");

        AssetDatabase.Refresh();

        TextAsset mapFile = (TextAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset));
        if (mapFile == null)
        {
            Debug.Log("MapFile not loaded properly");
            return;
        }

        AssetDatabase.SaveAssets();

        EditorGUIUtility.PingObject(mapFile);
    }
}

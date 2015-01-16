using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Data; 

[CustomEditor(typeof(MapMaker))]
public class MapMakerEditor : Editor {

	MapMaker maker;

	int tileIndex = 0;
    
    string mapFileName;
    MapData mapFileAsset;

    double drawTimer;

	void OnEnable()
	{
		maker = target as MapMaker;
        drawTimer = 0f;
	}

	void OnSceneGUI()
	{
        
		Event e = Event.current;

		Vector3 worldMousePos = Camera.current.ScreenToWorldPoint (new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight, 0f));

		Vector2 mousePos = new Vector2(worldMousePos.x, worldMousePos.y);
		

		
		if (e.isKey && e.character == 'a') {
            if (Time.realtimeSinceStartup - drawTimer <= 0.15f)
            {
                return;
            }
            drawTimer = Time.realtimeSinceStartup;

            List<Vector2> points = GetPointsInRange(worldMousePos, maker.m_brushSize);

            if (points.Count == 0)
            { 
                PutTile(worldMousePos);
                
                return;
            }

            foreach(Vector2 point in points)
            {
                PutTile(point);
            }
		}

        if (e.isKey && e.character == 'd')
        {
            if (Time.realtimeSinceStartup - drawTimer <= 0.15f)
            {
                return;
            }
            drawTimer = Time.realtimeSinceStartup;

            List<Vector2> points = GetPointsInRange(worldMousePos, maker.m_brushSize);

            if (points.Count == 0)
            {
                DeleteTile(worldMousePos);

                return;
            }

            foreach (Vector2 point in points)
            {
                DeleteTile(point);
            }
        }

        if (e.isKey && e.character == 'g')
        {
            maker.ToggleGrid();
        }
	}

    void PutTile(Vector2 point)
    {
        GridCoord coord = new GridCoord(Mathf.FloorToInt((point.x + 0.5f) / maker.m_tileSize) * maker.m_tileSize, Mathf.FloorToInt((point.y + 0.5f) / maker.m_tileSize) * maker.m_tileSize);

        if (maker.m_tiles.ContainsKey(coord)) return;

        /*
        RaycastHit2D hit = Physics2D.Raycast(point, Vector2.zero, float.MaxValue, LayerMask.GetMask("Tile", "Building"));

        if (hit.collider != null)
        {
            //Debug.Log("Unable to locate tile");
            return;
        }*/

        PrefabType prefab = PrefabUtility.GetPrefabType(maker.m_brushTile);

        if (prefab == PrefabType.Prefab)
        {
            Tile tile = (Tile)PrefabUtility.InstantiatePrefab(maker.m_brushTile);
            tile.m_coord = coord;
            tile.transform.position = tile.m_coord.ToVector2();
            tile.m_health = tile.m_maxHealth;

            maker.Add(tile);
        }
    }

    void DeleteTile(Vector2 point)
    {
        RaycastHit2D hit = Physics2D.Raycast(point, Vector2.zero, float.MaxValue, LayerMask.GetMask("Tile", "Building"));

        if (hit.collider == null)
        {
            return;
        }

        maker.Remove(hit.collider.gameObject.GetComponent<Tile>());
    }

    List<Vector2> GetPointsInRange(Vector2 center, float radius)
    {
        List<Vector2> points = new List<Vector2>();

        int yBottom = Mathf.CeilToInt(center.y - radius);
        int yTop = Mathf.FloorToInt(center.y + radius);

        for(int y = yBottom; y <= yTop; y++)
        {
            float yLength = center.y - y;
            float xLengthSqr = (radius * radius) - (yLength * yLength);
            float xLength = xLengthSqr < 0 ? 0 : Mathf.Sqrt(xLengthSqr);

            int xLeft = Mathf.CeilToInt(center.x - xLength);
            int xRight = Mathf.FloorToInt(center.x + xLength);

            for(int x = xLeft; x <= xRight; x++)
            {
                points.Add(new Vector2(x, y));
            }
        }

        return points;
    }


	public override void OnInspectorGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(" Tile Size ");
        maker.m_tileSize = EditorGUILayout.IntField(maker.m_tileSize, GUILayout.Width(100f));
		GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Map Width ");
        maker.m_width = EditorGUILayout.IntField(maker.m_width, GUILayout.Width(100f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Map Height ");
        maker.m_height = EditorGUILayout.IntField(maker.m_height, GUILayout.Width(100f));
        GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(" TileSet ");
		maker.m_tileSet = (TileSet)EditorGUILayout.ObjectField (maker.m_tileSet, typeof(TileSet), false, GUILayout.Width(150f));
		GUILayout.EndHorizontal();

		if (maker.m_tileSet != null) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(" Tile ");

			var index = EditorGUILayout.IntPopup ("Select Tile", tileIndex
                 , maker.m_tileSet.tiles.Select (t => t != null ? t.name : "").ToArray ()
                 , maker.m_tileSet.tiles.Select (t => ArrayUtility.IndexOf (maker.m_tileSet.tiles, t)).ToArray ()
			);
			if ((index != tileIndex || maker.m_brushTile == null) && maker.m_tileSet.tiles.Length > 0) {
					tileIndex = index;

					maker.m_brushTile = maker.m_tileSet.tiles [tileIndex];

					maker.m_tileSize = Mathf.FloorToInt(maker.m_brushTile.renderer.bounds.size.x);

					//move focus to first sceneview
					if (SceneView.sceneViews.Count > 0) { SceneView sceneView = (SceneView)SceneView.sceneViews[0]; sceneView.Focus(); }
			}
				
			GUILayout.EndHorizontal();
		}

        GUILayout.BeginHorizontal();
        GUILayout.Label("Brush Size");
        maker.m_brushSize = EditorGUILayout.FloatField(maker.m_brushSize);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Background");
        maker.m_backgroundImage = (Sprite)EditorGUILayout.ObjectField(maker.m_backgroundImage, typeof(Sprite), false, GUILayout.Width(150f));
        
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        mapFileAsset = (MapData)EditorGUILayout.ObjectField(mapFileAsset, typeof(MapData), false, GUILayout.Width(100));
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        if (GUILayout.Button("Load", GUILayout.MinWidth(100)))
        {
            Load();
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        mapFileName = EditorGUILayout.TextField(mapFileName, GUILayout.Width(100));
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        if (GUILayout.Button("Save", GUILayout.MinWidth(100)))
        {
            Save();
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", GUILayout.MinWidth(100)))
        {
            maker.Clear();
        }
        if (GUILayout.Button("Reload Tiles", GUILayout.MinWidth(100)))
        {
            maker.ReloadTiles();
        }
        if (GUILayout.Button("Apply", GUILayout.MinWidth(100)))
        {
            maker.Apply();
        }
        GUILayout.EndHorizontal();

		SceneView.RepaintAll ();
	}

    public void Load()
    {
        maker.Load(mapFileAsset);
    }

    public void Save()
    {
        maker.Apply();

        string filePath = AssetDatabase.GenerateUniqueAssetPath(string.Format("Assets/Resources/Maps/{0}.asset", maker.m_name));

        MapData mapAsset = CreateInstance<MapData>();
        maker.InitMapData(ref mapAsset);
        
        //mapAsset.hideFlags = HideFlags.NotEditable;

        AssetDatabase.CreateAsset(mapAsset, filePath);
        AssetDatabase.SaveAssets();

        Selection.activeObject = mapAsset;
        EditorGUIUtility.PingObject(mapAsset);
    }

    
}

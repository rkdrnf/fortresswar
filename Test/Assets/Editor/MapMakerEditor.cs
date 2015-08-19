using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Data;
using Architecture;
using Util;

[CustomEditor(typeof(MapMaker))]
public class MapMakerEditor : Editor {

	MapMaker maker;

	int tileIndex = 0;
    
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

            List<GridCoordDist> points = USelection.GetCoordsInRange(worldMousePos, maker.m_brushSize);

            if (points.Count == 0)
            { 
                PutTile(worldMousePos);
                
                return;
            }

            foreach(GridCoordDist point in points)
            {
                PutTile(point.coord.ToVector2());
            }
		}

        if (e.isKey && e.character == 'd')
        {
            if (Time.realtimeSinceStartup - drawTimer <= 0.15f)
            {
                return;
            }
            drawTimer = Time.realtimeSinceStartup;

            List<GridCoordDist> points = USelection.GetCoordsInRange(worldMousePos, maker.m_brushSize);

            if (points.Count == 0)
            {
                DeleteTile(worldMousePos);

                return;
            }

            foreach (GridCoordDist point in points)
            {
                DeleteTile(point.coord.ToVector2());
            }
        }

        if (e.isKey && e.character == 'g')
        {
            maker.ToggleGrid();
        }
	}

    void PutTile(Vector2 point)
    {
        GridCoord coord = GridCoord.ToCoord(point);
         
        if (maker.m_tiles.ContainsKey(coord)) return;

        Tile tile = maker.GenBrushTile(coord);

        maker.Add(tile);
    }

    void DeleteTile(Vector2 point)
    {
        GridCoord coord = new GridCoord((short)(Mathf.FloorToInt((point.x + 0.5f) / maker.m_tileSize) * maker.m_tileSize), (short)(Mathf.FloorToInt((point.y + 0.5f) / maker.m_tileSize) * maker.m_tileSize));
        
        maker.Remove(coord);
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
        GUILayout.Label(" chunkSize ");
        maker.m_chunkSize = EditorGUILayout.IntField(maker.m_chunkSize, GUILayout.Width(150f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" TileChunkPrefab ");
        maker.m_chunkPrefab = (TileChunk)EditorGUILayout.ObjectField(maker.m_chunkPrefab, typeof(TileChunk), false, GUILayout.Width(150f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" TileManager ");
        maker.m_tileManager = (TileManager)EditorGUILayout.ObjectField(maker.m_tileManager, typeof(TileManager), false, GUILayout.Width(150f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" BuildingManager ");
        maker.m_buildingManager = (BuildingManager)EditorGUILayout.ObjectField(maker.m_buildingManager, typeof(BuildingManager), false, GUILayout.Width(150f));
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

					maker.m_tileSize = Mathf.FloorToInt(maker.m_brushTile.size.x);

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
        maker.m_name = EditorGUILayout.TextField(maker.m_name, GUILayout.Width(100));
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

        if (GUILayout.Button("Generate", GUILayout.MinWidth(100)))
        {
            maker.GenTerrain();
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

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MapMaker))]
public class MapMakerEditor : Editor {

	MapMaker maker;

	void OnEnable()
	{
		maker = (MapMaker)target;
	}

	void OnSceneGUI()
	{
		Event e = Event.current;

		Vector3 worldMousePos = Camera.current.ScreenToWorldPoint (new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight, 0f));

		Vector2 mousePos = new Vector2(worldMousePos.x, worldMousePos.y);
		RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

		
		if (e.isKey && e.character == 'a') {
			Debug.Log (e.mousePosition);
			Debug.Log (worldMousePos);
			
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
				tile.transform.position = new Vector3(Mathf.Floor((mousePos.x + 0.5f) / maker.width) * maker.width, Mathf.Floor((mousePos.y + 0.5f) / maker.height) * maker.height, 0f);
			}
		}
	}


	public override void OnInspectorGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(" Grid Width ");
		maker.width = EditorGUILayout.FloatField(maker.width, GUILayout.Width(50));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(" Grid Height ");
		maker.height = EditorGUILayout.FloatField(maker.height, GUILayout.Width(50));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Tile");
		maker.tile = (GameObject)EditorGUILayout.ObjectField (maker.tile, typeof(GameObject), false);
		GUILayout.EndHorizontal();

		SceneView.RepaintAll ();
	}

	/*
	void OnSceneGUI()
	{
		int controlID = GUIUtility.GetControlID (FocusType.Passive);
		if (Event.current.type == EventType.mouseDown)
		{
			RaycastHit hit;
			if (Physics.Raycast(Event.current.mouseRay, out hit, Mathf.Infinity, LayerMask.GetMask("Tile")))
			{
				Debug.Log("Hit a part of the terrain");
			}

			hit.transform.position.y += 5;

			if(Event.current.type == EventType.layout)
			{
				HandleUtility.AddDefaultControl(controlID);
			}
		}
	}
	*/
}

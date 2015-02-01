using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ShadowTest : MonoBehaviour {

    protected MeshFilter meshFilter;
    protected Mesh mesh;
	// Use this for initialization
	void Start () {
        meshFilter = GetComponent<MeshFilter>();

        if (!Application.isPlaying)  // 에디터모드
        {
            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }
            mesh = meshFilter.sharedMesh;
        }
        else // 플레이모드
        {
            mesh = meshFilter.mesh;
        }

        DrawMesh();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    [ContextMenu("draw")]
    void DrawMesh()
    {
        mesh.Clear(false);
        mesh.vertices = new Vector3[] { new Vector3(0, 3, -1), new Vector3(3, 3, -1), new Vector3(3, 0, -1), new Vector3(0, 0, -1) };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.uv = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(0, 1) };

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Const;
using Data;
using System;
using System.Linq;

[ExecuteInEditMode]
public abstract class PolygonGenerator<T, DT> : MonoBehaviour 
    where T : Structure<T, DT> 
    where DT : StructureData
{
    public int chunkSize;
    public GridCoord m_coord;

    public List<Vector3> newVertices = new List<Vector3>();
    public List<int> newTriangles = new List<int>();
    public List<Vector2> newUV = new List<Vector2>();
    

    private float tUnit = 0.2f;
    private Vector2 tStone = new Vector2(0, 3);
    private Vector3 tDirt = new Vector3(0, 4);

    private int squareCount;

    public T[,] blocks;

    public List<Vector3> colVertices = new List<Vector3>();
    public List<int> colTriangles = new List<int>();
    private int colCount;

    private MeshCollider col;

    protected bool update = false;

    // Use this for initialization
    protected MeshFilter meshFilter;
    protected Mesh mesh;

    void Start()
    {
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

        col = GetComponent<MeshCollider>();

        //GenTerrain();
        BuildMesh();
        UpdateMesh();
    }

    public void Init(GridCoord coord, int chunkSize)
    {
        m_coord = coord;
        this.chunkSize = chunkSize;

        blocks = new T[chunkSize, chunkSize];
        
        //meshFilter = GetComponent<MeshFilter>();
        //_mesh = meshFilter.sharedMesh;
        //col = GetComponent<MeshCollider>();

        //GenTerrain();
        //BuildMesh();
        //UpdateMesh();
    }

    void BuildMesh()
    {
        for (int px = 0; px < blocks.GetLength(0); px++)
        {
            for (int py = 0; py < blocks.GetLength(1); py++)
            {
                if (blocks[px, py] != null)
                {
                    GenCollider(px, py);
                    GenSquare(px, py, blocks[px, py]);
                }
            }
        }
    }

    protected abstract Vector2 GetTexture(T structure);

    void GenSquare(int px, int py, T structure)
    {
        Vector2 texture = GetTexture(structure);

        float x = px - 0.5f;
        float y = py - 0.5f;

        float z = 0;

        newVertices.Add(new Vector3(x, y + 1, z));
        newVertices.Add(new Vector3(x + 1, y + 1, z));
        newVertices.Add(new Vector3(x + 1, y, z));
        newVertices.Add(new Vector3(x, y, z));

        newTriangles.Add((squareCount * 4));
        newTriangles.Add((squareCount * 4) + 1);
        newTriangles.Add((squareCount * 4) + 2);
        newTriangles.Add((squareCount * 4) + 0);
        newTriangles.Add((squareCount * 4) + 2);
        newTriangles.Add((squareCount * 4) + 3);

        newUV.Add(new Vector2(tUnit * texture.x, tUnit * texture.y + tUnit));
        newUV.Add(new Vector2(tUnit * texture.x + tUnit, tUnit * texture.y + tUnit));
        newUV.Add(new Vector2(tUnit * texture.x + tUnit, tUnit * texture.y));
        newUV.Add(new Vector2(tUnit * texture.x, tUnit * texture.y));

        squareCount++;
    }

    void UpdateMesh()
    {
        if (!Application.isPlaying) //에디터모드
        {
            mesh.Clear(false);
        }
        else //플레이모드
        {
            mesh.Clear();
        }
        
        mesh.vertices = newVertices.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.triangles = newTriangles.ToArray();

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        squareCount = 0;
        newVertices.Clear();
        newTriangles.Clear();
        newUV.Clear();

        Mesh newMesh = new Mesh();
        newMesh.vertices = colVertices.ToArray();
        newMesh.triangles = colTriangles.ToArray();
        col.sharedMesh = newMesh;

        colVertices.Clear();
        colTriangles.Clear();
        colCount = 0;
    }

    void GenCollider(int px, int py)
    {
        float x = px - 0.5f;
        float y = py - 0.5f;
        //Top
        if (Block(px, py + 1) == null)
        {
            colVertices.Add(new Vector3(x, y + 1, 1));
            colVertices.Add(new Vector3(x + 1, y + 1, 1));
            colVertices.Add(new Vector3(x + 1, y + 1, 0));
            colVertices.Add(new Vector3(x, y + 1, 0));

            ColliderTriangles();

            colCount++;
        }

        //bot
        if (Block(px, py - 1) == null)
        {
            colVertices.Add(new Vector3(x, y, 0));
            colVertices.Add(new Vector3(x + 1, y, 0));
            colVertices.Add(new Vector3(x + 1, y, 1));
            colVertices.Add(new Vector3(x, y, 1));

            ColliderTriangles();
            colCount++;
        }


        //left
        if (Block(px - 1, py) == null)
        {
            colVertices.Add(new Vector3(x, y, 1));
            colVertices.Add(new Vector3(x, y + 1, 1));
            colVertices.Add(new Vector3(x, y + 1, 0));
            colVertices.Add(new Vector3(x, y, 0));

            ColliderTriangles();
            colCount++;
        }

        //right
        if (Block(px + 1, py) == null)
        {
            colVertices.Add(new Vector3(x + 1, y + 1, 1));
            colVertices.Add(new Vector3(x + 1, y, 1));
            colVertices.Add(new Vector3(x + 1, y, 0));
            colVertices.Add(new Vector3(x + 1, y + 1, 0));

            ColliderTriangles();
            colCount++;
        }
    }

    void ColliderTriangles()
    {
        colTriangles.Add(colCount * 4);
        colTriangles.Add((colCount * 4) + 1);
        colTriangles.Add((colCount * 4) + 2);
        colTriangles.Add((colCount * 4) + 0);
        colTriangles.Add((colCount * 4) + 2);
        colTriangles.Add((colCount * 4) + 3);
    }

    T Block(int x, int y)
    {
        if (x == -1 || x == blocks.GetLength(0) || y == -1 || y == blocks.GetLength(1))
        {
            return null;
        }

        return blocks[x, y];
    }

    public int Count()
    {
        int count = 0;
        for (int px = 0; px < blocks.GetLength(0); px++)
        {
            for (int py = 0; py < blocks.GetLength(1); py++)
            {
                if (blocks[px, py] != null)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public abstract void AddBlock(GridCoord coord, T block);
    public abstract void RemoveBlock(T block);
    
    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying) return;

        if(update)
        {
            BuildMesh();
            UpdateMesh();
            update = false;
        }
    }

    void OnRenderObject()
    {
        if (Application.isPlaying) return;

        if (update)
        {
            BuildMesh();
            UpdateMesh();
            update = false;
        }
    }
}

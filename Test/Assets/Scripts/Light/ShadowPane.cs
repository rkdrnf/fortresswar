using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Lights
{ 
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ShadowPane : MonoBehaviour
    {
        MeshFilter meshFilter;
        Mesh mesh;

        public List<Vector3> newVertices = new List<Vector3>();
        public List<int> newTriangles = new List<int>();
        public List<Vector2> newUV = new List<Vector2>();

        private int squareCount;
        bool update = false;

        Texture2D lightMap;

        int m_width;
        int m_height;

        private static ShadowPane instance;

        public static ShadowPane Inst
        {
            get { return instance; }
        }

        void Awake()
        {
            instance = this;

            meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;
        }

        public void Init(int mapWidth, int mapHeight)
        {
            m_width = mapWidth;
            m_height = mapHeight;
            transform.localScale = new Vector3(mapWidth + 1, mapHeight + 1, 1);
            lightMap = new Texture2D((m_width + 1) * 8, (m_height + 1) * 8, TextureFormat.Alpha8, false);
            renderer.material.SetTexture("_MainTex", lightMap);
            renderer.material.SetFloat("_BoundX", 1f / ((m_width + 1)));
            renderer.material.SetFloat("_BoundY", 1f / ((m_height + 1)));

            for(int i = 0; i < lightMap.width; i++)
            {
                for (int j = 0; j < lightMap.height; j++)
                {
                    lightMap.SetPixel(i, j, new Color(0, 0, 0, 0));
                }
            }

            //TestDraw();
        }

        void TestDraw()
        {
            for(int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    lightMap.SetPixel(i, j, new Color(0, 0, 0, 0));
                }
            }

            update = true;
        }

        void UpdateMesh()
        {
            mesh.Clear(false);

            mesh.vertices = newVertices.ToArray();
            mesh.triangles = newTriangles.ToArray();
            mesh.uv = newUV.ToArray();

            mesh.Optimize();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            squareCount = 0;
            newVertices.Clear();
            newTriangles.Clear();
            newUV.Clear();
        }

        void BuildMesh()
        {
            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
            timer.Stop();
            TimeSpan timespan = timer.Elapsed;
            //Debug.Log("Build Mesh Time: " + timespan);
        }

        void GenSquare(int px, int py, int intensity)
        {
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

            newUV.Add(new Vector2(intensity, 0));
            newUV.Add(new Vector2(intensity, 0));
            newUV.Add(new Vector2(intensity, 0));
            newUV.Add(new Vector2(intensity, 0));
             
            squareCount++;
        }

        void Update()
        {
            if (update)
            {
                //BuildMesh();
                //BuildCollider();
                //UpdateMesh();
                lightMap.Apply();
                update = false;

                //Debug.Log("mesh updated Frame: " + Time.frameCount);
            }
        }

        public void UpdateLight(GridCoord coord, float intensity)
        {
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        lightMap.SetPixel((coord.x + (m_width / 2)) * 8 + i, (coord.y + (m_height / 2)) * 8 + j, new Color(0, 0, 0, intensity));
                    }
                }
                update = true;
            } 
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
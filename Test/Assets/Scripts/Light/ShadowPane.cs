using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Architecture;

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
            GetComponent<Renderer>().material.SetTexture("_MainTex", lightMap);
            GetComponent<Renderer>().material.SetFloat("_BoundX", 1f / ((m_width + 1)));
            GetComponent<Renderer>().material.SetFloat("_BoundY", 1f / ((m_height + 1)));

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

        void Update()
        {
            if (update)
            {
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

        public void UpdateLight(GridCoord coord)
        {
            Building building = BuildingManager.Inst.Get(coord);

            if (building != null)
            { 
                UpdateLight(coord, 1);
                return;
            }

            Tile tile = TileManager.Inst.Get(coord);

            if (tile != null)
            {
                UpdateLight(coord, tile.m_health == 0 ? 0.5f : 1f);
                return;
            }

            UpdateLight(coord, 0);


        }
    }
}
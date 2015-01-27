using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Const;
using Data;
using System;
using System.Linq;



namespace Architecture
{
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


        private float tUnit = 0.125f;
        private Vector2 tStone = new Vector2(0, 3);
        private Vector3 tDirt = new Vector3(0, 4);

        private int squareCount;

        public T[,] blocks;

        public List<Vector3> colVertices = new List<Vector3>();
        public List<int> colTriangles = new List<int>();
        private int colCount;

        public List<List<Vector2>> colPaths = new List<List<Vector2>>();
        public int pathCount;
        public List<List<Vector2>> closedPaths = new List<List<Vector2>>();

        //private MeshCollider col;
        private PolygonCollider2D col;

        private bool update = false;

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

            col = GetComponent<PolygonCollider2D>();

            //GenTerrain();
            //BuildMesh();
            //UpdateMesh();
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
            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
            for (int px = 0; px < blocks.GetLength(0); px++)
            {
                for (int py = 0; py < blocks.GetLength(1); py++)
                {
                    T block = blocks[px, py];
                    if (block != null)
                    {
                        if (blocks[px, py].CanCollide())
                            GenCollider2DNew(block, px, py);

                        GenSquare(px, py, block);
                    }
                }
            }

            MergeBetweenPaths();

            timer.Stop();
            TimeSpan timespan = timer.Elapsed;
            //Debug.Log("Build Mesh Time: " + timespan);
        }
        /*
        void BuildCollider()
        {
            GridDirection
        }

        void ConnectPath(List<GridDirection direction, int px, int py)
        {
            foreach(GridDirection next in NextDirection(direction))
            {

            }
        }
        */

        void UpdateCollider()
        {
            if (colPaths.Count > 0)
            {
                //Debug.Log("Open Path remains");
                return;
            }

            col.pathCount = closedPaths.Count;
            for (int i = 0; i < closedPaths.Count; i++)
            {
                col.SetPath(i, closedPaths[i].ToArray());
                //  Debug.Log("PathLength : " + closedPaths[i].Count);

            }

            //Debug.Log("PathCount : " + closedPaths.Count);

            pathCount = 0;
            colPaths.Clear();
            closedPaths.Clear();
        }

        bool Collidable(T block)
        {
            if (block == null)
                return false;

            return block.CanCollide();
        }

        void GenCollider2DNew(T block, int px, int py)
        {
            T top = Block(px, py + 1);
            T right = Block(px + 1, py);
            T bottom = Block(px, py - 1);
            T left = Block(px - 1, py);

            bool canColTop = Collidable(top);
            bool canColLeft = Collidable(left);
            bool canColBot = Collidable(bottom);
            bool canColRight = Collidable(right);

            if (!canColBot)
            {
                MergePath(new Vector2(px - 0.5f, py - 0.5f), new Vector2(px + 0.5f, py - 0.5f));
            }

            if (!canColLeft)
            {
                MergePath(new Vector2(px - 0.5f, py - 0.5f), new Vector2(px - 0.5f, py + 0.5f));
            }

            if (!canColTop)
            {
                MergePath(new Vector2(px - 0.5f, py + 0.5f), new Vector2(px + 0.5f, py + 0.5f));
            }

            if (!canColRight)
            {
                MergePath(new Vector2(px + 0.5f, py - 0.5f), new Vector2(px + 0.5f, py + 0.5f));
            }
        }

        void MergePath(Vector2 a, Vector2 b)
        {
            //Debug.Log("A : " + a + "B : " + b);
            foreach (List<Vector2> path in colPaths)
            {
                if (TryAddNode(path, a, b))
                    return;

                if (TryAddNode(path, b, a))
                    return;
            }

            //Debug.Log(string.Format("New Path A : {0} B: {1}", a, b));

            colPaths.Add(new List<Vector2>(new Vector2[] { a, b }));
        }

        void MergeBetweenPaths()
        {
            while (colPaths.Count > 0)
            {
                List<Vector2> path = colPaths.First();
                colPaths.Remove(path);

                List<Vector2> head = null;
                List<Vector2> tail = null;

                foreach (List<Vector2> remain in colPaths)
                {
                    if (path.First() == remain.First())
                    {
                        remain.Reverse();
                        head = remain;
                    }
                    else if (path.First() == remain.Last())
                    {
                        head = remain;
                    }

                    if (path.Last() == remain.First())
                    {
                        if (remain == head) break;
                        tail = remain;
                    }
                    else if (path.Last() == remain.Last())
                    {
                        if (remain == head) break;
                        remain.Reverse();
                        tail = remain;
                    }
                }

                colPaths.Remove(head);
                colPaths.Remove(tail);

                //Debug.Log("MergePath : " + String.Join(", ", path.Select((a) => a.ToString()).ToArray()));

                if (head != null)
                {
                    //Debug.Log("HeadPath : " + String.Join(", ", head.Select((a) => a.ToString()).ToArray()));
                    if ((head[head.Count - 1] - head[head.Count - 2]).normalized == (path[1] - path[0]).normalized)
                    {
                        head.Remove(head.Last());
                        path.RemoveAt(0);
                        path = head.Concat(path).ToList();
                    }
                    else
                    {
                        head.Remove(head.Last());
                        path = head.Concat(path).ToList();
                    }


                }

                if (tail != null)
                {
                    Debug.Log("TailPath : " + String.Join(", ", tail.Select((a) => a.ToString()).ToArray()));
                    if ((path[path.Count - 1] - path[path.Count - 2]).normalized == (tail[1] - tail[0]).normalized)
                    {
                        path.Remove(path.Last());
                        tail.RemoveAt(0);
                        path = path.Concat(tail).ToList();
                    }
                    else
                    {
                        tail.Remove(tail.First());
                        path = path.Concat(tail).ToList();
                    }
                }

                if (CheckClosed(path))
                {
                    if ((path[path.Count - 1] - path[path.Count - 2]).normalized == (path[1] - path[0]).normalized)
                    {
                        path.Remove(path.First());
                        path.Remove(path.Last());
                    }

                    closedPaths.Add(path);
                }
                else
                {
                    colPaths.Add(path);
                }
            }
        }

        bool TryAddNode(List<Vector2> path, Vector2 a, Vector2 b)
        {
            if (path.First() == a)
            {
                if (path.Count > 1 && (path[1] - path[0]).normalized == (a - b)) //같은 방향
                {
                    path[0] = b;
                }
                else
                {
                    //Debug.Log(string.Format("Different direction A: {0} B: {1}", a, b));
                    path.Insert(0, b);
                }

                if (CheckClosed(path))
                {
                    path.Remove(path.Last());
                    colPaths.Remove(path);
                    closedPaths.Add(path);
                }
                return true;
            }

            if (path.Last() == a)
            {
                if ((path[path.Count - 2] - path[path.Count - 1]).normalized == (a - b))
                {
                    path[path.Count - 1] = b;
                }
                else
                {
                    path.Add(b);
                }

                if (CheckClosed(path))
                {
                    path.Remove(path.Last());
                    colPaths.Remove(path);
                    closedPaths.Add(path);
                }
                return true;
            }

            return false;
        }

        bool CheckClosed(List<Vector2> path)
        {
            //Closed Check
            return path.First() == path.Last();
        }

        void GenCollider2D(T block, int px, int py)
        {
            List<Vector2> path = new List<Vector2>();

            T top = Block(px, py + 1);
            T right = Block(px + 1, py);
            T bottom = Block(px, py - 1);
            T left = Block(px - 1, py);

            bool canColTop = Collidable(top);
            bool canColLeft = Collidable(left);
            bool canColBot = Collidable(bottom);
            bool canColRight = Collidable(right);

            int length = 0;

            //TopLeft
            if (!canColTop || !canColLeft)
            {
                path.Add(new Vector2(px - 0.5f, py + 0.5f));
                length++;
            }

            //TopRight
            if (!canColTop || !canColRight)
            {
                path.Add(new Vector2(px + 0.5f, py + 0.5f));
                length++;
            }


            //BottomRight
            if (!canColBot || !canColRight)
            {
                path.Add(new Vector2(px + 0.5f, py - 0.5f));
                length++;
            }

            //BottomLeft
            if (!canColLeft || !canColBot)
            {
                path.Add(new Vector2(px - 0.5f, py - 0.5f));
                length++;
            }


            if (length > 0)
            {
                pathCount++;
                if (length == 2)
                {
                    path.Add(new Vector2(px, py));
                }
                colPaths.Add(path);
            }
        }



        GridDirection[] NextDirection(GridDirection inDirection)
        {
            switch (inDirection)
            {
                case GridDirection.UP:
                    return new GridDirection[3] { GridDirection.RIGHT, GridDirection.UP, GridDirection.LEFT };

                case GridDirection.RIGHT:
                    return new GridDirection[3] { GridDirection.DOWN, GridDirection.RIGHT, GridDirection.UP };

                case GridDirection.DOWN:
                    return new GridDirection[3] { GridDirection.LEFT, GridDirection.DOWN, GridDirection.RIGHT };

                case GridDirection.LEFT:
                    return new GridDirection[3] { GridDirection.UP, GridDirection.LEFT, GridDirection.DOWN };
                default:
                    return new GridDirection[0];
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
                mesh.Clear(false);
            }

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

            UpdateCollider();


            //MESH COLLIDER
            /* 
             * 
            Mesh newMesh = new Mesh();
            newMesh.vertices = colVertices.ToArray();
            newMesh.triangles = colTriangles.ToArray();
            col.sharedMesh = newMesh;

            colVertices.Clear();
            colTriangles.Clear();
            colCount = 0;
             * */
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

            if (update)
            {
                BuildMesh();
                //BuildCollider();
                UpdateMesh();
                update = false;

                //Debug.Log("mesh updated Frame: " + Time.frameCount);
            }
        }

        [ContextMenu("Refresh")]
        protected void Refresh()
        {
            BuildMesh();
            UpdateMesh();
            update = false;
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

        public void SendUpdate()
        {
            update = true;
        }
        
    }
}
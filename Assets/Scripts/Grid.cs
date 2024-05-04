using UnityEngine;
using System.Collections.Generic;
using System;

namespace Neo
{
    //[System.Serializable]
    [ExecuteInEditMode]
    public class Grid : MonoBehaviour
    {
        public static readonly float TileRadius = 0.5f; // in meters
        public static readonly float TileDiameter = TileRadius * 2f; // in meters
        public static readonly float TileEpsilon = 0.025f; // in meters

        public enum ECorner
        {
            BottomLeft = 0,
            TopLeft = 1,
            BottomRight = 2,
            TopRight = 3
        }
        public Vector2Int[] Corners { get; protected set; }

        public int LayerMaskWall { get; protected set; }

        [SerializeField]
        protected Transform floorTransform;

        [SerializeField]
        protected Transform[] wallTransforms;

        //[SerializeField]
        protected Vector3 referenceCornerPos;

        [SerializeField]
        [HideInInspector]//Want this to get saved but not show up in the inspector
        protected Vector3 size;

        [SerializeField]
        protected Vector2Int numTiles = new Vector2Int(10, 10);
        public Vector2Int NumTiles {
            get => numTiles;
            protected set {
                if( numTiles.Equals(value) )
                {
                    return;
                }

                numTiles = value;
                DetermineGridInfo();
            }
        }

        public int Area {
            get => NumTiles.x * NumTiles.y;
        }

        public class Node
        {
            public int   CollisionLayers;

            public float Score;
            //public bool[] NeighborAccessible = new bool[4];
            public int[] NeighborLayer = new int[4];
        }
        protected Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();

        //public static readonly Vector3[] Compass = new Vector3[] { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
        public static readonly Vector2Int[] Compass = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        protected void Awake()
        {
            LayerMaskWall = LayerMask.GetMask("Wall");

            Corners = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, NumTiles.y - 2), new Vector2Int(NumTiles.x - 1, 1), new Vector2Int(NumTiles.x - 1, NumTiles.y - 2) };

            Generate();
        }

        public void DetermineGridInfo() //Called from editor also
        {
            var mr = floorTransform.GetComponent<MeshRenderer>();
            var floorScale = floorTransform.localScale;
            floorScale.x = NumTiles.x;
            floorScale.z = NumTiles.y;
            floorTransform.localScale = floorScale;

            float calcPosComp( float posComp )
            {
                var whole = (int)posComp;
                var frac = (posComp - whole);
                var num = NumTiles.x * 0.5f;
                return SafeSign(posComp) * num + frac;
            }

            for(int ix = 0; ix < wallTransforms.Length; ++ix)
            {
                var wall = wallTransforms[ix];
                var wallPos = wall.localPosition;
                if( (ix % 2) != 0 )
                {
                    wallPos.x = calcPosComp(wallPos.x);
                } else
                {
                    wallPos.z = calcPosComp(wallPos.z);
                }
                wall.localPosition = wallPos;

                var wallScale = wall.localScale;
                wallScale.x = (wallScale.x - SafeSign(wallScale.x) * (int)wallScale.x) + SafeSign(wallScale.x) * NumTiles.x;
                wall.localScale = wallScale;
            }

            var texScale = mr.sharedMaterial.mainTextureScale;
            texScale.x = (float)NumTiles.x * 0.5f;
            texScale.y = (float)NumTiles.y * 0.5f;
            mr.sharedMaterial.mainTextureScale = texScale;
            size = new Vector3(NumTiles.x * TileDiameter, 0f, NumTiles.y * TileDiameter);            
        }

        protected Vector3 DetermineReferenceCorner(Transform transform)
        {
            float hStart = transform.position.x - ((NumTiles.x - 1) * 0.5f) * Grid.TileDiameter;;
            float vStart = transform.position.z - ((NumTiles.y - 1) * 0.5f) * Grid.TileDiameter;

            return new Vector3(hStart, 0f, vStart);
        }

        protected void FindObstacleInfo(Transform obstacle, Dictionary<Vector2Int, int> tilesObstructed)
        {
            FindObstacleInfo(obstacle.position, obstacle.localScale, obstacle.gameObject.layer, tilesObstructed);
        }

        protected void FindObstacleInfo(Vector3 pos, Vector3 size, int layers, Dictionary<Vector2Int, int> tilesObstructed)
        {
            int hNumTiles = (int)(size.x / Grid.TileDiameter);
            int vNumTiles = (int)(size.z / Grid.TileDiameter);

            float hStart = pos.x - (((float)hNumTiles - 1f) * 0.5f) * Grid.TileDiameter;
            float vStart = pos.z - (((float)vNumTiles - 1f) * 0.5f) * Grid.TileDiameter;

            for (int iy = 0; iy < vNumTiles; ++iy)
            {
                for (int ix = 0; ix < hNumTiles; ++ix)
                {
                    try
                    {
                        WorldToGrid(new Vector3(hStart + ix * Grid.TileDiameter, 0f, vStart + iy * Grid.TileDiameter), out Vector2Int tile);
                        tilesObstructed.Add(tile, layers);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        protected void CreateNodes(Transform grid)
        {
            Dictionary<Vector2Int, int> obstructions = new Dictionary<Vector2Int, int>();
            try
            {
                foreach (Transform child in grid)
                {
                    if (!child.name.Contains("Obstacle", System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if(!child.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    FindObstacleInfo(child, obstructions);
                }

                var jail =  grid.GetComponentInChildren<Jail>();
                System.Diagnostics.Debug.Assert(jail != null);
                FindObstacleInfo(jail.transform.position, jail.Size, jail.gameObject.layer, obstructions);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            try {
                Vector2Int loc = Vector2Int.zero;
                Node node;
                for (int iy = 0; iy < NumTiles.y; ++iy)
                {
                    loc.y = iy;
                    for (int ix = 0; ix < NumTiles.x; ++ix)
                    {
                        loc.x = ix;

                        node = new Node() { Score = obstructions.ContainsKey(loc) ? float.MaxValue : 0f };
                        nodes.Add(loc, node);
                        for (int id = 0; id < Compass.Length; ++id)
                        {
                            node.NeighborLayer[id] = TryDir(loc, Compass[id]);
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                Debug.LogException(ex);
            }
        }

        public void Generate()
        {
            referenceCornerPos = DetermineReferenceCorner(transform);
            CreateNodes(transform);
        }

        public bool TryDir(Vector3 pos, Vector3 dir)
        {
            Ray ray = new Ray(pos, dir);
            return !Physics.Raycast(ray, out RaycastHit info, TileDiameter, LayerMaskWall);
        }

        public int TryDir(Vector3 pos, Vector3 dir, int extraLayer)
        {
            Ray ray = new Ray(pos, dir);
            if(!Physics.Raycast(ray, out RaycastHit info, TileDiameter, LayerMaskWall | extraLayer))
            {
                return 0;
            }

            return info.collider.gameObject.layer;
        }

        public int TryDir(Vector2Int loc, Vector2Int dir, int extraLayer)
        {
            GridToWorld(loc, out Vector3 pos);
            Ray ray = new Ray(pos, new Vector3(dir.x, 0f, dir.y));
            if(!Physics.Raycast(ray, out RaycastHit info, TileDiameter, LayerMaskWall | extraLayer))
            {
                return 0;
            }

            return info.collider.gameObject.layer;
        }

        public int TryDir(Vector2Int loc, Vector2Int dir)
        {
            return TryDir(loc, dir, 0);
        }

        public bool TileIsObstructed(Vector2Int tile)
        {
            bool withinRange = tile.x >= 0 && tile.x < NumTiles.x && tile.y >= 0 && tile.y < NumTiles.y;
            if(!withinRange)
            {
                return true;
            }

            if( !nodes.TryGetValue(tile, out Node node))
            {
                return true;
            }

            if (Mathf.Approximately(node.Score, float.MaxValue))
            {
                return true;
            }
            return false;
        }

        public bool TileIsObstructed(int x, int y)
        {
            return TileIsObstructed(new Vector2Int(x, y));
        }

        public bool IsObstructed(Vector2Int loc, int dirIndex)
        {
            return (nodes.TryGetValue(loc, out Node node)) ? node.NeighborLayer[dirIndex] != 0 : true;
        }

        public bool IsObstructed(Vector2Int loc, int dirIndex, int extraLayers)
        {
            return (nodes.TryGetValue(loc, out Node node)) ? (node.NeighborLayer[dirIndex] & extraLayers) != 0 : true;
        }

        protected bool  IsInRange(Vector2Int loc)
        {
            return IsInRange(loc.x, loc.y);
        }

        protected bool IsInRange(int x, int y)
        {
            if( x < 0 || x >= NumTiles.x )
            {
                return false;
            }

            if (y < 0 || y >= NumTiles.y)
            {
                return false;
            }

            return true;
        }

        protected static int SafeSign(float f)
        {
            return f >= 0.0f ? 1 : -1;
        }

        protected static int SafeSign(int f)
        {
            return f >= 0 ? 1 : -1;
        }

        public static Vector3 SnapToGrid(Vector3 pos)
        {
            int iX = (int)(pos.x);
            pos.x = (float)iX + SafeSign(pos.x) * TileRadius;

            int iY = (int)(pos.z);
            pos.z = (float)iY + SafeSign(pos.z) * TileRadius;

            return pos;
        }

        public static void SnapToGrid(ref Vector3 pos)
        {
            pos = SnapToGrid(pos);
        }

        public bool GridToWorld(int x, int y, out Vector3 worldPos)
        {
            if( !IsInRange(x, y))
            {
                worldPos = Vector3.zero;
                return false;
            }
            worldPos = new Vector3(referenceCornerPos.x + x * TileDiameter, 0.2f, referenceCornerPos.z + y * TileDiameter);
            return true;
        }

        public bool GridToWorld(Vector2Int tile, out Vector3 worldPos)
        {
            return GridToWorld(tile.x, tile.y, out worldPos);
        }

        public bool WorldToGrid(Vector3 worldPos, out int x, out int y)
        {
            var deltaPos = SnapToGrid(worldPos) - referenceCornerPos;
            x = (int)(deltaPos.x / TileDiameter);
            y = (int)(deltaPos.z / TileDiameter);

            return IsInRange(x, y);
        }

        public bool WorldToGrid(Vector3 worldPos, out int x, out int y, out float dX, out float dY)
        {
            var snappedPos = SnapToGrid(worldPos);
            var deltaPos = snappedPos - referenceCornerPos;
            x = (int)(deltaPos.x / TileDiameter);
            y = (int)(deltaPos.z / TileDiameter);

            dX = snappedPos.x - worldPos.x;
            dY = snappedPos.z - worldPos.z;

            return IsInRange(x, y);
        }

        public bool WorldToGrid(Vector3 worldPos, out Vector2Int indices)
        {
            SnapToGrid(ref worldPos);

            var deltaPos = worldPos - referenceCornerPos;
            indices = new Vector2Int((int)(deltaPos.x / TileDiameter), (int)(deltaPos.z / TileDiameter));
            return IsInRange(indices);
        }
    }
}
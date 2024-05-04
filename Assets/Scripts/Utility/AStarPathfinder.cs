﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public class Node
    {
        public Vector2Int GridLocation;
        public float gCost = 0f;
        public float hCost = 0f;
        public Node Parent;

        public Node(Vector2Int loc, Node parent)
        {
            GridLocation = loc;
            this.Parent = parent;
        }

        public float fCost {
            get { return gCost + hCost; }
        }

        public override bool Equals(object obj)
        {
            var node = obj as Node;
            return node != null && GridLocation.Equals(node.GridLocation);
        }

        public override int GetHashCode()
        {
            return GridLocation.GetHashCode();
        }
    }

    protected Dictionary<Vector2Int, LinkedListNode<Node>> nodeMap = new();
    public LinkedList<Node> Nodes;
    protected LinkedListNode<Node> currentNode;

    public event Action OnComplete;

    protected bool isValid = false;
    public bool IsValid {
        get => isValid;
        set {
            if(isValid == value)
            {
                return;
            }

            isValid = value;
            Game.Instance.Player.OnMoved -= Invalidate;
            DestroyVisualizations();
            if (isValid)
            {
                Game.Instance.Player.OnMoved += Invalidate;
                Game.Instance.CreateVisualizationsFor(this);
            }
        }
    }
    
    public Path( LinkedList<Node> nodes )
    {
        Nodes = nodes;

        var node = nodes.First;
        while(node != null)
        {
            nodeMap.Add(node.Value.GridLocation, node);
            node = node.Next;
        }
        IsValid = true;        
    }

    public   void Invalidate()
    {
        IsValid = false;
    }

    public  bool    FindNode( Vector2Int loc, out Node node )
    {
        node = null;
        if( !nodeMap.TryGetValue(loc, out LinkedListNode<Node> llNode) )
        {
            return false;
        }

        node = llNode.Value;
        return true;
    }

    public bool FindNextNode(Vector2Int loc, out Node node)
    {
        node = null;
        if( !FindNextNode(loc, out LinkedListNode<Node> llNode) )
        {
            return false;
        }

        node = llNode.Value;
        return node != null;
    }

    public bool FindNextNode(Vector2Int loc, out LinkedListNode<Node> node)
    {
        if(!nodeMap.TryGetValue(loc, out node))
        {
            return false;
        }

        node = node.Next;
        return node != null;
    }

    public Vector3 DetermineFirstDirection(out Vector3 startPos)
    {
        try
        {
            currentNode = null;

            if (Nodes.Count < 2 )
            {
                startPos = Vector3.zero;
                return Vector3.zero;
            }

            var firstNode = Nodes.First.Value;
            var nextNode = Nodes.First.Next.Value;

            Game.Instance.GridToWorld(nextNode.GridLocation, out Vector3 toPos);
            Game.Instance.GridToWorld(firstNode.GridLocation, out startPos);
            return Vector3.Normalize(toPos - startPos);
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
            startPos = Vector3.zero;
            return Vector3.zero;
        }
    }

    public bool DetermineNextDirection(out Vector3 dir)
    {
        Game.Instance.GridToWorld(currentNode.Value.GridLocation, out Vector3 fromPos);

        currentNode = currentNode.Next;
        if (currentNode == null)
        {
            dir = Vector3.zero;
            OnComplete?.Invoke();
            return false;
        }

        Game.Instance.GridToWorld(currentNode.Value.GridLocation, out Vector3 toPos);
        

        dir = Vector3.Normalize(toPos - fromPos);
        return true;
    }

    public bool  DetermineNextDirection( Vector2Int from, out Vector3 dir )
    {
        if(!FindNextNode(from, out currentNode))
        {
            dir = Vector3.zero;
            OnComplete?.Invoke();
            return false;
        }

        Game.Instance.GridToWorld(currentNode.Value.GridLocation, out Vector3 toPos);
        Game.Instance.GridToWorld(from, out Vector3 fromPos);

        dir = Vector3.Normalize(toPos - fromPos);
        return true;
    }

    public bool IsComplete() {
        return currentNode == null || currentNode == Nodes.Last;
    }

    public bool IsComplete( Vector3 pos ) {
        var node = Nodes.Last.Value;
        Game.Instance.GridToWorld(node.GridLocation, out Vector3 nodePos);
        return Mathf.Approximately(nodePos.x, pos.x) && Mathf.Approximately(nodePos.z, pos.z);
    }

    protected List<GameObject> pathVisualizedGOs = new List<GameObject>();
    public void DestroyVisualizations()
    {
        //if(pathVisualizedGOs.Count > 0)
        //{
        //    GameObject go = pathVisualizedGOs[0];
        //    pathVisualizedGOs.Clear();
        //    GameObject.DestroyImmediate(go);
        //}
        for( int ix = pathVisualizedGOs.Count - 1; ix >= 0; --ix)
        {
            GameObject.Destroy(pathVisualizedGOs[ix]);
        }
    }

    public void CreateVisualizations(Neo.Grid grid)
    {
        GameObject go = null;
        GameObject prevGo = null;

        var node = Nodes.First;
        int nodeIndex = 0;
        while(node != null)
        {
            grid.GridToWorld(node.Value.GridLocation, out Vector3 pos);
            prevGo = go;

            var resource = Resources.Load<GameObject>("Prefabs/FlagMarker");
            go = GameObject.Instantiate(resource) as GameObject;
            go.name = string.Format("PathNode{0}", nodeIndex++);
            pathVisualizedGOs.Add(go);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.identity;
            if (prevGo != null) go.transform.SetParent(prevGo.transform, true);

            node = node.Next;
        }
    }
}

public class AStarPathfinder
{
    private Neo.Grid grid;

    public AStarPathfinder(Neo.Grid extGrid)
    {
        grid = extGrid;
    }

    protected Path.Node FindEquivalentOf(Vector2Int loc, LinkedList<Path.Node> nodes)
    {
        foreach (var n in nodes)
        {
            if (n.GridLocation.Equals(loc))
            {
                return n;
            }
        }

        return null;
    }

    protected bool IsContainedIn(Vector2Int loc, LinkedList<Path.Node> nodes)
    {
        foreach (var n in nodes)
        {
            if (n.GridLocation.Equals(loc))
            {
                return true;
            }
        }

        return false;
    }

    public Path FindPath(Vector2Int start, Vector2Int target, int collisionLayers)
    {
        if (start.Equals(target))
        {
            return null;
        }

        Path.Node startNode = new Path.Node(start, null);

        LinkedList<Path.Node> openSet = new LinkedList<Path.Node>();
        LinkedList<Path.Node> closedSet = new LinkedList<Path.Node>();
        openSet.AddLast(startNode);

        int maxCount = grid.Area;
        while (openSet.Count > 0)
        {
            if (openSet.Count > maxCount || closedSet.Count > maxCount)
            {
                return null;
            }

            Path.Node currentNode = openSet.First.Value;
            openSet.VisitAll((Path.Node node) => {
                foreach (var openNode in openSet)
                {
                    if (openNode.fCost < currentNode.fCost || (openNode.fCost == currentNode.fCost && openNode.hCost < currentNode.hCost))
                    {
                        currentNode = openNode;
                    }
                }
                return true;
            });

            openSet.Remove(currentNode);
            closedSet.AddLast(currentNode);

            if (currentNode.GridLocation.Equals(target))
            {
                return BuildPath(closedSet.First.Value, closedSet.Last.Value);
            }

            for (int ix = 0; ix < Neo.Grid.Compass.Length; ++ix)
            {
                if (grid.IsObstructed(currentNode.GridLocation, ix))
                {
                    continue;
                }

                Vector2Int neighbor = currentNode.GridLocation + Neo.Grid.Compass[ix];
                float newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                Path.Node neighborNode = FindEquivalentOf(neighbor, closedSet);
                if (neighborNode == null)
                {
                    neighborNode = new Path.Node(neighbor, currentNode);
                    neighborNode.gCost = newMovementCostToNeighbor;
                    neighborNode.hCost = GetDistance(neighbor, target);
                    neighborNode.Parent = currentNode;
                    openSet.AddLast(neighborNode);
                }

                if (newMovementCostToNeighbor < neighborNode.gCost)
                {
                    neighborNode.gCost = newMovementCostToNeighbor;
                    neighborNode.hCost = GetDistance(neighbor, target);
                    neighborNode.Parent = currentNode;

                    closedSet.Remove(neighborNode);
                    if (!openSet.Contains(neighborNode))
                    {
                        openSet.AddLast(neighborNode);
                    }
                }
            }
        }

        //Debug.Log(string.Format("No Path Found..."));
        return null;
    }

    public Path FindPath(Vector2Int start, Vector2Int target)
    {
        return FindPath(start, target, 0);
    }

    protected Path BuildPath(Path.Node startNode, Path.Node endNode)
    {
        LinkedList<Path.Node> pathList = new();
        Path.Node currentNode = endNode;

        //Debug.LogFormat("Start Loc: {0}", startNode.GridLocation);
        //Debug.LogFormat("End Loc: {0}", endNode.GridLocation);
        while (currentNode != startNode)
        {
            //Debug.LogFormat("Node Loc: {0}", currentNode.GridLocation);
            pathList.AddFirst(currentNode);
            currentNode = currentNode.Parent;
        }
        //Debug.LogFormat("Node Loc: {0}", currentNode.GridLocation);
        pathList.AddFirst(currentNode);
        var path = new Path(pathList);
        return path;
    }

    protected Path.Node FindEquivalentOf(Path.Node node, LinkedList<Path.Node> nodes)
    {
        Path.Node outNode = null;
        nodes.VisitAll((Path.Node n) => {
            if (node.Equals(n))
            {
                outNode = n;
                return false;
            }
            return true;
        });

        return outNode;
    }

    float GetDistance(Path.Node nodeA, Path.Node nodeB)
    {
        int dstX = Math.Abs(nodeA.GridLocation.x - nodeB.GridLocation.x);
        int dstY = Math.Abs(nodeA.GridLocation.y - nodeB.GridLocation.y);
        return dstX + dstY; // Manhattan distance
    }

    float GetDistance(Path.Node nodeA,  Vector2Int target)
    {
        int dstX = Math.Abs(nodeA.GridLocation.x - target.x);
        int dstY = Math.Abs(nodeA.GridLocation.y - target.y);
        return dstX + dstY; // Manhattan distance
    }

    float GetDistance(Vector2Int start, Vector2Int target)
    {
        int dstX = Math.Abs(start.x - target.x);
        int dstY = Math.Abs(start.y - target.y);
        return dstX + dstY; // Manhattan distance
    }
}
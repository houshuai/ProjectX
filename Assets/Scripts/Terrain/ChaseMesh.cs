using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChaseMesh
{
    private int xCountOfTile, yCountOfTile;
    private static List<Rect> allRect;
    private static List<Node[,]> allNodes;

    public ChaseMesh(int xCountOfTile, int yCountOfTile)
    {
        this.xCountOfTile = xCountOfTile;
        this.yCountOfTile = yCountOfTile;
        allRect = new List<Rect>();
        allNodes = new List<Node[,]>();
    }

    public static Node GetNode(Vector3 position)
    {
        Node result = null;
        for (int i = 0; i < allRect.Count; i++)
        {
            if (allRect[i].Contains(new Vector2(position.x, position.z)))
            {
                foreach (var node in allNodes[i])
                {
                    if (Vector3.Distance(node.center, position) < 1.0f)
                    {
                        result = node;
                    }
                }
            }
        }
        return result;
    }

    public Node[,] BuildMesh(Rect rect, Vector3[] vertices, List<GameObject> plantList)
    {
        if (allRect.Contains(rect))
        {
            return null;
        }
        var xTick = rect.width / (xCountOfTile - 1);
        var yTick = rect.height / (yCountOfTile - 1);
        var pos = new Vector3(rect.x, 0, rect.y);
        
        var positions = new List<Vector3>();
        foreach (var plant in plantList)
        {
            if (plant.name!="Grass")
            {
                positions.Add(plant.transform.localPosition);
            }
            
        }

        var nodes = new Node[xCountOfTile, yCountOfTile];
        int index = 0;
        for (int j = 0; j < yCountOfTile; j++)
        {
            for (int i = 0; i < xCountOfTile; i++)
            {
                var v = vertices[index++];
                nodes[i, j] = new Node(new Vector3(pos.x + v.x, pos.y + v.y, pos.z + v.z), xTick, yTick, true);
            }
        }

        foreach (var p in positions)
        {
            int x = (int)(p.x / xTick);
            int y = (int)(p.z / yTick);
            nodes[x, y].isWalkable = false;
        }

        AddData(rect, nodes);

        return nodes;
    }

    public Task AddDataAsync(Rect rect, Node[,] nodes)
    {
        return Task.Run(() => AddData(rect, nodes));
    }

    private void AddData(Rect rect, Node[,] nodes)
    {
        for (int i = 0; i < xCountOfTile - 1; i++)
        {
            for (int j = 1; j < yCountOfTile - 1; j++)
            {
                var node = nodes[i, j];
                node.AddNeighbor(nodes[i + 1, j - 1]);
                node.AddNeighbor(nodes[i + 1, j]);
                node.AddNeighbor(nodes[i + 1, j + 1]);
                node.AddNeighbor(nodes[i, j + 1]);
            }
        }
        for (int i = 0; i < xCountOfTile - 1; i++)
        {
            var node = nodes[i, 0];
            node.AddNeighbor(nodes[i + 1, 0]);
            node.AddNeighbor(nodes[i + 1, 1]);
            node.AddNeighbor(nodes[i, 1]);
        }

        for (int i = 0; i < allRect.Count; i++)
        {
            // in the left of rect
            if (Mathf.Approximately(rect.xMin, allRect[i].xMax) && Mathf.Approximately(rect.yMin, allRect[i].yMin))
            {
                LinkHorizontal(allNodes[i], nodes);
            }
            // in the bottom of rect
            else if (Mathf.Approximately(rect.yMin, allRect[i].yMax) && Mathf.Approximately(rect.x, allRect[i].xMin))
            {
                LinkVertical(allNodes[i], nodes);
            }
            // in the right of rect
            else if (Mathf.Approximately(rect.xMax, allRect[i].xMin) && Mathf.Approximately(rect.yMin, allRect[i].yMin))
            {
                LinkHorizontal(nodes, allNodes[i]);
            }
            // in the top of rect
            else if (Mathf.Approximately(rect.yMax, allRect[i].yMin) && Mathf.Approximately(rect.xMin, allRect[i].xMin))
            {
                LinkVertical(nodes, allNodes[i]);
            }

        }

        lock (allRect)
        {
            allRect.Add(rect);
            allNodes.Add(nodes);
        }
        
    }

    private void LinkHorizontal(Node[,] left, Node[,] right)
    {
        var node = left[xCountOfTile - 1, 0];
        node.AddNeighbor(right[0, 0]);
        node.AddNeighbor(right[0, 1]);
        node = left[xCountOfTile - 1, yCountOfTile - 1];
        node.AddNeighbor(right[0, yCountOfTile - 2]);
        node.AddNeighbor(right[0, yCountOfTile - 1]);
        for (int j = 1; j < yCountOfTile - 1; j++)
        {
            node = left[xCountOfTile - 1, j];
            node.AddNeighbor(right[0, j - 1]);
            node.AddNeighbor(right[0, j]);
            node.AddNeighbor(right[0, j + 1]);
        }
    }

    private void LinkVertical(Node[,] bottom, Node[,] top)
    {
        var node = bottom[0, yCountOfTile - 1];
        node.AddNeighbor(top[0, 0]);
        node.AddNeighbor(top[1, 0]);
        node = bottom[xCountOfTile - 1, yCountOfTile - 1];
        node.AddNeighbor(top[xCountOfTile - 2, 0]);
        node.AddNeighbor(top[xCountOfTile - 1, 0]);
        for (int i = 1; i < xCountOfTile - 1; i++)
        {
            node = bottom[i, yCountOfTile - 1];
            node.AddNeighbor(top[i - 1, 0]);
            node.AddNeighbor(top[i, 0]);
            node.AddNeighbor(top[i + 1, 0]);
        }

    }

    public void RemoveData(Rect rect)
    {
        if (!allRect.Contains(rect))
        {
            return;
        }
        var nodes = allNodes[allRect.IndexOf(rect)];

        for (int i = 0; i < allRect.Count; i++)
        {
            // in the left of rect
            if (Mathf.Approximately(rect.xMin, allRect[i].xMax) && Mathf.Approximately(rect.yMin, allRect[i].yMin))
            {
                UnlinkHorizontal(allNodes[i], nodes);
            }
            // in the bottom of rect
            else if (Mathf.Approximately(rect.yMin, allRect[i].yMax) && Mathf.Approximately(rect.x, allRect[i].xMin))
            {
                UnlinkVertical(allNodes[i], nodes);
            }
            // in the right of rect
            else if (Mathf.Approximately(rect.xMax, allRect[i].xMin) && Mathf.Approximately(rect.yMin, allRect[i].yMin))
            {
                UnlinkHorizontal(nodes, allNodes[i]);
            }
            // in the top of rect
            else if (Mathf.Approximately(rect.yMax, allRect[i].yMin) && Mathf.Approximately(rect.xMin, allRect[i].xMin))
            {
                UnlinkVertical(nodes, allNodes[i]);
            }

        }

        lock (allRect)
        {
            allRect.Remove(rect);
            allNodes.Remove(nodes);
        }
    }

    private void UnlinkHorizontal(Node[,] left, Node[,] right)
    {
        var node = left[xCountOfTile - 1, 0];
        node.RemoveNeighbor(right[0, 0]);
        node.RemoveNeighbor(right[0, 1]);
        node = left[xCountOfTile - 1, yCountOfTile - 1];
        node.RemoveNeighbor(right[0, yCountOfTile - 2]);
        node.RemoveNeighbor(right[0, yCountOfTile - 1]);
        for (int j = 1; j < yCountOfTile - 1; j++)
        {
            node = left[xCountOfTile - 1, j];
            node.RemoveNeighbor(right[0, j - 1]);
            node.RemoveNeighbor(right[0, j]);
            node.RemoveNeighbor(right[0, j + 1]);
        }
    }

    private void UnlinkVertical(Node[,] bottom, Node[,] top)
    {
        var node = bottom[0, yCountOfTile - 1];
        node.RemoveNeighbor(top[0, 0]);
        node.RemoveNeighbor(top[1, 0]);
        node = bottom[xCountOfTile - 1, yCountOfTile - 1];
        node.RemoveNeighbor(top[xCountOfTile - 2, 0]);
        node.RemoveNeighbor(top[xCountOfTile - 1, 0]);
        for (int i = 1; i < xCountOfTile - 1; i++)
        {
            node = bottom[i, yCountOfTile - 1];
            node.RemoveNeighbor(top[i - 1, 0]);
            node.RemoveNeighbor(top[i, 0]);
            node.RemoveNeighbor(top[i + 1, 0]);
        }
    }
}

public class Node
{
    public List<Node> neighbors;
    public Vector3 center;
    public float xMin, yMin, xMax, yMax;
    public bool isWalkable;

    public Node(Vector3 center, float xLen, float yLen, bool isWalkable)
    {
        this.center = center;
        xMin = center.x - xLen / 2;
        yMin = center.z - yLen / 2;
        xMax = center.x + xLen / 2;
        yMax = center.z + yLen / 2;
        this.isWalkable = isWalkable;
        neighbors = new List<Node>();
    }

    public void AddNeighbor(Node node)
    {
        if (isWalkable && node.isWalkable)
        {
            neighbors.Add(node);
            node.neighbors.Add(this);
        }
    }

    public void RemoveNeighbor(Node node)
    {
        if (neighbors.Contains(node))
        {
            neighbors.Remove(node);
            node.neighbors.Remove(this);
        }
    }

    public bool Contains(Vector3 point)
    {
        return point.x > xMin && point.x < xMax && point.z > yMin && point.z < yMax;
    }
}

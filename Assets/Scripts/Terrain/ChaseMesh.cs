using System.Collections.Generic;
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
                    if (Vector3.Distance(node.position, position) < 1.0f)
                    {
                        result = node;
                    }
                }
            }
        }
        return result;
    }

    public void AddData(Rect rect, Node[,] nodes)
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

        allRect.Add(rect);
        allNodes.Add(nodes);
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
}

public class Node
{
    public List<Node> neighbors;
    public Vector3 position;
    public bool isWalkable;

    private float xHalfLen, yHalfLen;

    public Node(Vector3 position, bool isWalkable, float xLen, float yLen)
    {
        this.position = position;
        this.isWalkable = isWalkable;
        xHalfLen = xLen / 2;
        yHalfLen = yLen / 2;
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

    public bool Contains(Vector3 point)
    {
        bool test = point.x < position.x - xHalfLen || point.x > position.x + xHalfLen ||
            point.y < position.y - yHalfLen || point.y > position.y + yHalfLen;

        return !test;
    }
}

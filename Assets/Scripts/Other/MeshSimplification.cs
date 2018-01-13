using System.Collections.Generic;
using UnityEngine;

public class MeshSimplification : MonoBehaviour
{
    public float epsilon = 0.01f;

    private Mesh mesh;
    private List<Vertex> vertices;
    private List<Triangle> triangles;

    private void Start()
    {
        Initial();
        for (int i = 0; i < vertices.Count; i++)
        {
            CalcCostAtVertex(vertices[i]);
        }

    }

    private void Update()
    {
        var v = MinimumCost();
        Simplify(v);

        var verts = new Vector3[vertices.Count];
        int index = 0;
        foreach (var vertex in vertices)
        {
            verts[index++] = vertex.position;
        }

        var tris = new int[triangles.Count * 3];
        index = 0;
        foreach (var triangle in triangles)
        {
            tris[index++] = triangle.vertices[0].id;
            tris[index++] = triangle.vertices[1].id;
            tris[index++] = triangle.vertices[2].id;
        }

        mesh.triangles = tris;
        mesh.RecalculateNormals();

    }


    private void Initial()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        var oldVertices = mesh.vertices;
        var normals = mesh.normals;
        var oldTriangles = mesh.triangles;

        vertices = new List<Vertex>();
        for (int i = 0; i < oldVertices.Length; i++)
        {
            vertices.Add(new Vertex(i, oldVertices[i], normals[i]));
        }

        triangles = new List<Triangle>();
        for (int i = 0; i < oldTriangles.Length; i += 3)
        {
            var v1 = vertices[oldTriangles[i]];
            var v2 = vertices[oldTriangles[i + 1]];
            var v3 = vertices[oldTriangles[i + 2]];
            var triangle = new Triangle(v1, v2, v3);
            triangles.Add(triangle);

            v1.faces.Add(triangle);
            v1.AddNeighbor(v2);
            v1.AddNeighbor(v3);

            v2.faces.Add(triangle);
            v2.AddNeighbor(v1);
            v2.AddNeighbor(v3);

            v3.faces.Add(triangle);
            v3.AddNeighbor(v1);
            v3.AddNeighbor(v2);
        }
    }

    private float CalcEdgeCost(Vertex v1, Vertex v2)
    {
        var E = v2.position - v1.position;
        var D = Vector3.Cross(v1.normal, E).normalized;

        var sides = new List<Triangle>();
        var others = new List<Triangle>();
        for (int i = 0; i < v1.faces.Count; i++)
        {
            if (v1.faces[i].HasVertex(v2))
            {
                sides.Add(v1.faces[i]);
            }
            else
            {
                others.Add(v1.faces[i]);
            }
        }

        if (sides.Count < 2)
        {
            return -1;
        }
        var test1 = Vector3.Dot(D, sides[0].GetThirdVertex(v1, v2).position - v1.position);
        var test2 = Vector3.Dot(D, sides[1].GetThirdVertex(v1, v2).position - v1.position);
        if (test1 * test2 > 0)
        {
            return -1;
        }

        var Tpos = Vector3.zero;
        var Tneg = Vector3.zero;
        if (test1 >= 0)
        {
            Tpos = sides[0].normal;
            Tneg = sides[1].normal;
        }
        else
        {
            Tpos = sides[1].normal;
            Tneg = sides[0].normal;
        }

        float d = 1;
        for (int i = 0; i < others.Count; i++)
        {
            var curr = others[i];
            float a = 0, b = 0;
            for (int j = 0; j < 3; j++)
            {
                var v = curr.vertices[j];
                if (v != v1)
                {
                    if (a == 0)
                    {
                        a = Vector3.Dot(D, v.position - v1.position);
                    }
                    else
                    {
                        b = Vector3.Dot(D, v.position - v1.position);
                    }
                }
            }

            if (a > epsilon)
            {
                d = Mathf.Min(d, Vector3.Dot(curr.normal, Tpos));
            }
            else if (a < -epsilon)
            {
                d = Mathf.Min(d, Vector3.Dot(curr.normal, Tneg));
            }
            if (b > epsilon)
            {
                d = Mathf.Min(d, Vector3.Dot(curr.normal, Tpos));
            }
            else if (b < -epsilon)
            {
                d = Mathf.Min(d, Vector3.Dot(curr.normal, Tneg));
            }
        }

        return (1 - d) * E.magnitude;
    }

    private void CalcCostAtVertex(Vertex v)
    {
        if (v.neighbors.Count == 0)
        {
            v.simplification = null;
            v.cost = -1;
            return;
        }

        foreach (var neighbor in v.neighbors)
        {
            int faceCount = 0;
            foreach (var face in neighbor.faces)
            {
                if (face.HasVertex(neighbor))
                {
                    faceCount++;
                }
            }
            if (faceCount < 2)
            {
                v.simplification = null;
                v.cost = -1;
                return;
            }
        }

        v.cost = 100000;
        for (int i = 0; i < v.neighbors.Count; i++)
        {
            float c = CalcEdgeCost(v, v.neighbors[i]);
            if (c < v.cost)
            {
                v.simplification = v.neighbors[i];
                v.cost = c;
            }
        }
    }

    private void Simplify(Vertex v)
    {
        if (v == null || v.simplification == null)
        {
            return;
        }

        var temp = new List<Vertex>(v.neighbors);

        for (int i = v.faces.Count - 1; i >= 0; i--)
        {
            var face = v.faces[i];
            if (face.HasVertex(v.simplification))
            {
                triangles.Remove(face);
                v.faces.RemoveAt(i);
                for (int j = 0; j < v.neighbors.Count; j++)
                {
                    if (v.neighbors[j].faces.Contains(face))
                    {
                        v.neighbors[j].faces.Remove(face);
                    }
                }
            }
        }

        for (int i = v.faces.Count - 1; i >= 0; i--)
        {
            v.faces[i].ReplaceVertex(v, v.simplification);
        }

        var v2 = v.simplification;
        for (int i = 0; i < v2.neighbors.Count; i++)
        {
            if (v2.neighbors[i] == v)
            {
                v2.neighbors.RemoveAt(i);
            }
        }

        for (int i = 0; i < v.neighbors.Count; i++)
        {
            var neighbor = v.neighbors[i];
            if (neighbor != v2)
            {
                for (int j = 0; j < neighbor.neighbors.Count; j++)
                {
                    if (neighbor.neighbors[j] == v)
                    {
                        neighbor.neighbors[j] = v2;
                    }
                }
            }
        }

        vertices.Remove(v);

        for (int i = 0; i < temp.Count; i++)
        {
            CalcCostAtVertex(temp[i]);
        }
    }

    private Vertex MinimumCost()
    {
        Vertex vertex = null;
        float c = 100000;
        for (int i = 0; i < vertices.Count; i++)
        {
            var v = vertices[i];
            if (v.cost > -1 && c > v.cost)
            {
                c = v.cost;
                vertex = v;
            }
        }

        return vertex;
    }
}

internal class Triangle
{
    public Vertex[] vertices = new Vertex[3];      //the 3 points that make this triangle
    public Vector3 normal;                         //unit vector

    public Triangle(Vertex v1, Vertex v2, Vertex v3)
    {
        vertices[0] = v1;
        vertices[1] = v2;
        vertices[2] = v3;
        normal = Vector3.Cross(v2.position - v1.position, v3.position - v1.position).normalized;
    }

    public bool HasVertex(Vertex v)
    {
        for (int i = 0; i < 3; i++)
        {
            if (vertices[i] == v)
            {
                return true;
            }
        }

        return false;
    }

    public Vertex GetThirdVertex(Vertex v1, Vertex v2)
    {
        for (int i = 0; i < 3; i++)
        {
            if (vertices[i] != v1 && vertices[i] != v2)
            {
                return vertices[i];
            }
        }
        return null;
    }

    public void ReplaceVertex(Vertex v1, Vertex v2)
    {
        for (int i = 0; i < 3; i++)
        {
            var vert = vertices[i];
            if (vert == v1)
            {
                vertices[i] = v2;
            }
        }

        v2.faces.Add(this);
    }
}

internal class Vertex
{
    public int id;                    //place of vertex in original list
    public Vector3 position;          //location of this point
    public Vector3 normal;
    public List<Vertex> neighbors;    //adjacent vertices
    public List<Triangle> faces;       //adjacent triangles
    public float cost;                //cost of collapsing edge
    public Vertex simplification;     //candidate 

    public Vertex(int id, Vector3 position, Vector3 normal)
    {
        this.id = id;
        this.position = position;
        this.normal = normal;
        neighbors = new List<Vertex>();
        faces = new List<Triangle>();
        cost = -1;
        simplification = null;
    }

    public void AddNeighbor(Vertex neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
        }
    }
}

using UnityEngine;

public class Grass : MonoBehaviour
{
    public GrassBox[] grassBoxes;
    public Material grassMat;

    private int totalGrassCount;
    private Vector3[] vertices;
    private int[] indices;
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(Tags.Player).transform;

        foreach (var grassBox in grassBoxes)
        {
            totalGrassCount += grassBox.grassCount;
        }
        vertices = new Vector3[totalGrassCount];
        indices = new int[totalGrassCount];
        for (int i = 0; i < totalGrassCount; i++)
        {
            indices[i] = i;
        }

        int startIndex = 0;
        foreach (var grassBox in grassBoxes)
        {
            GenerateGrass(grassBox, startIndex);
            startIndex += grassBox.grassCount;
        }

        var mesh = new Mesh
        {
            vertices = vertices
        };
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = grassMat;

    }

    private void GenerateGrass(GrassBox grassBox,int startIndex)
    {
        int grassCount = grassBox.grassCount;
        float width = grassBox.width;
        float height = grassBox.height;
        var start = grassBox.transform.position;
        
        for (int i = startIndex; i < startIndex+grassCount; i++)
        {
            vertices[i] = new Vector3(start.x + Random.value * width, 0f,start.z + Random.value * height);
        }

    }

    private void Update()
    {
        grassMat.SetVector("_PlayerPos", player.position);
    }
}

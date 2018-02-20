using UnityEngine;

public class HealthBoard : MonoBehaviour
{
    private Material material;

    private void Start()
    {
        var vertices = new Vector3[1];
        vertices[0] = new Vector3(0, 0, 0);

        var indices = new int[1];
        indices[0] = 0;

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        GetComponent<MeshFilter>().mesh = mesh;

        material = GetComponent<MeshRenderer>().material;
    }

    public void ChangeMaterial(float value)
    {
        material.SetFloat("_Health", value);
    }
}

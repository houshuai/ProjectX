using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footprint : MonoBehaviour
{
    public Material material;

    private List<GameObject> printList;
    private WaitForSeconds wait;

    private void Start()
    {
        printList = new List<GameObject>();
        for (int i = 0; i < 10; i++)
        {
            printList.Add(GenerateFootprint());
        }

        wait = new WaitForSeconds(5);
    }

    private GameObject GenerateFootprint()
    {
        var vertices = new Vector3[4]
        {
            new Vector3(-0.1f, 0, -0.2f),
            new Vector3(-0.1f, 0, 0.2f),
            new Vector3(0.1f, 0, 0.2f),
            new Vector3(0.1f, 0, -0.2f)

        };

        var indices = new int[6]
        {
            0,1,2, 0,2,3
        };

        var uv = new Vector2[4]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0)
        };

        var mesh = new Mesh
        {
            vertices = vertices,
            triangles = indices,
            uv = uv
        };
        mesh.RecalculateNormals();

        var o = new GameObject("footprint");
        o.AddComponent<MeshFilter>().mesh = mesh;
        o.AddComponent<MeshRenderer>().sharedMaterial = material;
        o.SetActive(false);

        return o;
    }

    public void ShowFootprint()
    {
        foreach (var print in printList)
        {
            if (!print.activeSelf)
            {
                print.transform.position = transform.position;
                print.transform.rotation = transform.rotation;
                print.SetActive(true);
                StartCoroutine(HideFootprint(print));
                break;
            }
        }
    }

    private IEnumerator HideFootprint(GameObject print)
    {
        yield return wait;
        print.SetActive(false);
    }
}

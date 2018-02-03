using UnityEngine;

public class InteractionWater : Water
{
    public float vertexTick = 3;
    public int xCount = 5;
    public int zCount = 5;
    public float waveSpeed = 1f;
    public float viscosity = 0.2f;
    public int noiseFrequency = 2;
    public float noiseAmplitude = 0.2f;
    public float interactMult = 0.0005f;

    private Mesh mesh;
    private float xmin, xmax, zmin, zmax;
    private float timeTick;
    private float f1, f2, k1, k2, k3;
    private Vector3[] prev;
    private Vector3[] curr;

    private Rigidbody player;
    private float noiseTime;
    private float noiseTimer;

    protected override void Start()
    {
        base.Start();

        GenerateMesh();
        timeTick = Time.fixedDeltaTime;
        f1 = waveSpeed * waveSpeed * timeTick * timeTick / (vertexTick * vertexTick);
        f2 = 1.0f / (viscosity * timeTick + 2);
        k1 = (4.0f - 8.0f * f1) * f2;
        k2 = (viscosity * timeTick - 2.0f) * f2;
        k3 = 2.0f * f1 * f2;

        player = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Rigidbody>();
        noiseTime = 1.0f / noiseFrequency;

        gameObject.AddComponent<Reflection>().Initial(transform.position.y);
        gameObject.AddComponent<Refraction>().Initial(transform.position.y);
    }

    private void GenerateMesh()
    {
        var xStart = -vertexTick * xCount / 2;
        var zStart = -vertexTick * zCount / 2;

        prev = new Vector3[xCount * zCount];
        curr = new Vector3[xCount * zCount];
        var indices = new int[(xCount - 1) * (zCount - 1) * 6];

        int index = 0;
        for (int j = 0; j < zCount; j++)
        {
            for (int i = 0; i < xCount; i++)
            {
                prev[index] = new Vector3(xStart + i * vertexTick, 0, zStart + j * vertexTick);
                curr[index] = prev[index];
                index++;
            }
        }

        index = 0;
        for (int j = 0; j < zCount - 1; j++)
        {
            for (int i = 0; i < xCount - 1; i++)
            {
                indices[index++] = i + xCount * j;
                indices[index++] = i + xCount * (j + 1);
                indices[index++] = i + 1 + xCount * j;
                indices[index++] = i + 1 + xCount * j;
                indices[index++] = i + xCount * (j + 1);
                indices[index++] = i + 1 + xCount * (j + 1);
            }
        }

        mesh = new Mesh
        {
            vertices = curr,
            triangles = indices
        };
        mesh.RecalculateNormals();

        gameObject.AddComponent<MeshFilter>().mesh = mesh;

        xmin = transform.position.x + xStart + vertexTick;
        zmin = transform.position.z + zStart + vertexTick;
        xmax = xmin + vertexTick * (xCount - 1);
        zmax = zmin + vertexTick * (zCount - 1);
    }

    private void Update()
    {
        //interact with water
        var pos = player.position;
        if (pos.x > xmin && pos.z > zmin && pos.x < xmax && pos.z < zmax)
        {
            int x = Mathf.FloorToInt((pos.x - xmin) / vertexTick) + 1;
            int z = Mathf.FloorToInt((pos.z - zmin) / vertexTick) + 1;
            var p = prev[x + xCount * z];
            prev[x + xCount * z] = new Vector3(p.x, p.y - interactMult * player.velocity.magnitude, p.z);
        }

        //generate random wave
        noiseTimer += Time.deltaTime;
        if (noiseTimer > noiseTime)
        {
            int x = Random.Range(1, xCount - 1);
            int z = Random.Range(1, zCount - 1);
            var p = prev[x + xCount * z];
            prev[x + xCount * z] = new Vector3(p.x, p.y - noiseAmplitude, p.z);
            noiseTimer = 0;
        }
    }

    private void FixedUpdate()
    {
        var next = prev;
        for (int j = 1; j < zCount - 1; j++)
        {
            for (int i = 1; i < xCount - 1; i++)
            {
                int index = i + xCount * j;
                next[index] = k1 * curr[index] + k2 * prev[index] +
                    k3 * (curr[index + 1] + curr[index - 1] + curr[index - xCount] + curr[index + xCount]);
            }
        }

        mesh.vertices = next;
        mesh.RecalculateNormals();

        prev = curr;
        curr = next;
    }
}

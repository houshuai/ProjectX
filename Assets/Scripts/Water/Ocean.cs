using UnityEngine;

public class Ocean : MonoBehaviour
{
    public Material material;
    public float xLength = 100;
    public float yLength = 50;
    public int xCount = 20;
    public int yCount = 10;

    public float a = 1;
    public float s = 1;
    public Vector2 d = new Vector2(1, 2);
    public float l = 1;
    public float q = 1;

    private Vector3[] vertices;
    private Mesh mesh;

    private void Start()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        var vertexCount = xCount * yCount;
        var xTick = xLength / (xCount - 1);
        var yTick = yLength / (yCount - 1);
        var u = 1.0f / (xCount - 1);
        var v = 1.0f / (yCount - 1);

        vertices = new Vector3[vertexCount];
        var uv = new Vector2[vertexCount];

        int index = 0;
        for (int j = 0; j < yCount; j++)
        {
            for (int i = 0; i < xCount; i++)
            {
                vertices[index] = new Vector3(i * xTick, 0, j * yTick);
                uv[index] = new Vector2(i * u, j * v);
                index++;
            }
        }

        index = 0;
        var indices = new int[(xCount - 1) * (yCount - 1) * 6];
        for (int j = 0; j < yCount - 1; j++)
        {
            for (int i = 0; i < xCount - 1; i++)
            {
                int self = i + (j * xCount);
                int next = i + ((j + 1) * xCount);
                indices[index++] = self;
                indices[index++] = next;
                indices[index++] = next + 1;
                indices[index++] = self;
                indices[index++] = next + 1;
                indices[index++] = self + 1;
            }
        }

        mesh = new Mesh()
        {
            vertices = vertices,
            uv = uv,
            triangles = indices,
        };
        mesh.RecalculateNormals();

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void GerstnerWave()
    {
        float w = 2 * 9.8f * Mathf.PI / l;
        float psi = s * w;

        var temp = (Vector3[])vertices.Clone();
        int index = 0;
        for (int j = 0; j < yCount; j++)
        {
            for (int i = 0; i < xCount; i++)
            {
                var vertex = temp[index];
                var p = (vertex.x * d.x + vertex.z * d.y) * w + Time.time * psi;
                var h = a * Mathf.Sin(p);
                var cosp = Mathf.Cos(p);
                var pos = new Vector3(q * a * d.x * cosp, h, q * a * d.y * cosp);
                temp[index] += pos;
                temp[index].y = h;
                index++;
            }
        }

        mesh.vertices = temp;
        mesh.RecalculateNormals();
    }

    private void FixedUpdate()
    {
        //GerstnerWave();
    }
}

public struct Complex
{
    private float real;
    private float imaginary;

    public Complex(float real, float imaginary)
    {
        this.real = real;
        this.imaginary = imaginary;
    }

    public float Real
    {
        get { return real; }
        set { real = value; }
    }

    public float Imaginary
    {
        get { return imaginary; }
        set { imaginary = value; }
    }

    public Complex Conjugate
    {
        get
        {
            return new Complex(this.real, -this.imaginary);
        }
    }

    public float Modulus
    {
        get
        {
            return Mathf.Sqrt((this.real * this.real) + (this.imaginary * this.imaginary));
        }
    }

    public static Complex operator +(Complex z1, Complex z2)
    {
        return new Complex(z1.real + z2.real, z1.imaginary + z2.imaginary);
    }

    public static Complex operator +(Complex c)
    {
        return c;
    }

    public static Complex operator -(Complex c)
    {
        return new Complex(-c.real, -c.imaginary);
    }

    public static Complex operator -(Complex z1, Complex z2)
    {
        return new Complex(z1.real - z2.real, z1.imaginary - z2.imaginary);
    }

    public static Complex operator *(Complex z1, Complex z2)
    {
        return new Complex((z1.real * z2.real) - (z1.imaginary * z2.imaginary), (z1.real * z2.imaginary) + (z1.imaginary * z2.real));
    }

    public static Complex operator *(float d1, Complex z2)
    {
        return new Complex(d1 * z2.real, d1 * z2.imaginary);
    }

    public static Complex operator *(Complex z1, float d2)
    {
        return d2 * z1;
    }

    public static Complex operator /(Complex z1, Complex z2)
    {
        float num = (z2.real * z2.real) + (z2.imaginary * z2.imaginary);
        return new Complex(((z1.real * z2.real) + (z1.imaginary * z2.imaginary)) / num, ((z1.imaginary * z2.real) - (z1.real * z2.imaginary)) / num);
    }

    public static bool operator ==(Complex z1, Complex z2)
    {
        return ((z1.real == z2.real) && (z1.imaginary == z2.imaginary));
    }

    public static bool operator !=(Complex z1, Complex z2)
    {
        if (z1.real == z2.real)
        {
            return (z1.imaginary != z2.imaginary);
        }
        return true;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return (this.real.GetHashCode() ^ this.imaginary.GetHashCode());
    }

    public override string ToString()
    {
        if (this.imaginary < 0.0)
        {
            return (this.real.ToString() + " " + this.imaginary.ToString() + " i");
        }
        return (this.real.ToString() + " +" + this.imaginary.ToString() + " i");
    }

    public string ToString(string format)
    {
        if (this.imaginary < 0.0)
        {
            return (this.real.ToString(format) + " " + this.imaginary.ToString(format) + " i");
        }
        return (this.real.ToString(format) + " +" + this.imaginary.ToString(format) + " i");
    }

}

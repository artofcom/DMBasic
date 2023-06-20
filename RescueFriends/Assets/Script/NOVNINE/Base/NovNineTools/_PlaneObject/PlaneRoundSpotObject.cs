using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class PlaneRoundSpotObject : MonoBehaviour
{
    public int divideCount = 36;

    public float innerRadius = 0f;
    public float outerRadius = 1f;

    public Color innerColor = Color.white;
    public Color outerColor = Color.black;

    public Color color
    {
        get {
            return innerColor;
        } set {
            if (this.color == value) return;
            innerColor = value;
            outerColor = value;
            BuildMesh();
        }
    }

    public Color colorOuterBlur
    {
        get {
            return innerColor;
        } set {
            if (this.color == value) return;
            innerColor = value;
            outerColor = value;
            outerColor.a = 0f;
            BuildMesh();
        }

    }

    // Use this for initialization
    void Start()
    {
        BuildMesh();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BuildMesh()
    {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        //MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();

        Mesh newMesh = new Mesh();
        newMesh.vertices = BuildVertices();

        newMesh.uv = BuildUvs();
        newMesh.triangles = BuildTriangles();

        newMesh.colors = BuildColors();

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        filter.mesh = newMesh;

        Mesh mesh = filter.sharedMesh;

        Bounds bounds = new Bounds();
        bounds.center = Vector3.zero;
        bounds.size = Vector3.one * outerRadius;

        mesh.bounds = bounds;
    }

    Vector3[] BuildVertices()
    {
        Vector3 []vertices = null;
        if (innerRadius >0f) {
            vertices = new Vector3[divideCount * 2];

            Vector3 pos = Vector3.zero;
            for (int i = 0; i < divideCount; i++) {
                pos.x = Mathf.Sin(2 * Mathf.PI / divideCount * (i+0.5f));
                pos.y = Mathf.Cos(2 * Mathf.PI / divideCount * (i + 0.5f));

                vertices[i * 2 + 0] = pos * innerRadius;
                vertices[i * 2 + 1] = pos * outerRadius;
            }
        } else {
            vertices = new Vector3[divideCount  + 1];

            Vector3 pos = Vector3.zero;
            for (int i = 0; i < divideCount; i++) {
                pos.x = Mathf.Sin(2 * Mathf.PI / divideCount * (i+0.5f));
                pos.y = Mathf.Cos(2 * Mathf.PI / divideCount * (i + 0.5f));

                vertices[i] = pos * outerRadius;
            }

            vertices[divideCount] = Vector3.zero;
        }

        return vertices;
    }

    Vector2[] BuildUvs()
    {
        Vector2 []uvs = null;
        if (innerRadius >0f) {
            uvs = new Vector2[divideCount * 2];
        } else {
            uvs = new Vector2[divideCount  + 1];
        }

        for (int i=0; i<uvs.Length; i++) {
            uvs[i] = Vector2.zero;
        }
        return uvs;
    }

    int[] BuildTriangles()
    {
        int []triangles = null;
        if (innerRadius >0f) {
            triangles = new int[divideCount * 6];
            for (int i = 0; i < divideCount; i++) {
                triangles[i * 6 + 0] = i * 2 + 0;
                triangles[i * 6 + 1] = i * 2 + 1;
                triangles[i * 6 + 2] = (i < (divideCount - 1)) ? i * 2 + 3 : 1;

                triangles[i * 6 + 3] = i * 2 + 0;
                triangles[i * 6 + 4] = (i < (divideCount - 1)) ? i * 2 + 3 : 1;
                triangles[i * 6 + 5] = (i < (divideCount - 1)) ? i * 2 + 2 : 0;
            }
        } else {
            triangles = new int[divideCount * 3];
            for (int i = 0; i < divideCount; i++) {
                triangles[i * 3 + 0] = divideCount;
                triangles[i * 3 + 1] = i;
                triangles[i * 3 + 2] = (i < (divideCount - 1)) ? i+1 : 0;
            }
        }

        return triangles;

    }

    Color[] BuildColors()
    {
        Color[] colors = null;
        if (innerRadius >0f) {
            colors = new Color[divideCount * 2];

            for (int i = 0; i < divideCount; i++) {
                colors[i * 2 + 0] = innerColor;
                colors[i * 2 + 1] = outerColor;
            }
        } else {
            colors = new Color[divideCount + 1];

            for (int i = 0; i < divideCount; i++) {
                colors[i] = outerColor;
            }
            colors[divideCount] = innerColor;
        }

        return colors;
    }
}


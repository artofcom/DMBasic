using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class PlaneMeshObject : MonoBehaviour
{
    public Rect region = new Rect(-1f, -1f, 2f, 2f);
    public Rect uv = new Rect(0f, 0f, 1f, 1f);
    public Vector3 scale = Vector3.one;

    public Color[] colors;

    public Color color
    {
        get {
            if (null == colors) return Color.white;
            return colors[0];
        } set {
            if (null == colors) return;
            if (this.color == value) return;

            for (int i=0; i<colors.Length; i++)
                colors[i] = value;
            BuildMesh();
        }
    }

    // Use this for initialization
    void Start ()
    {
        //MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        //if (null == filter.sharedMesh)
        BuildMesh();

    }

    // Update is called once per frame
    void Update ()
    {

    }

    public void BuildMesh()
    {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        //MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();

        Mesh newMesh = new Mesh();
        newMesh.vertices = new Vector3[] {
            Vector3.right * region.xMin * scale.x + Vector3.up * region.yMin * scale.y,
            Vector3.right * region.xMin * scale.x + Vector3.up * region.yMax * scale.y,
            Vector3.right * region.xMax * scale.x + Vector3.up * region.yMin * scale.y,
            Vector3.right * region.xMax * scale.x + Vector3.up * region.yMax * scale.y

            /* Vector3.left * scale.x + Vector3.down * scale.y,
            Vector3.left * scale.x + Vector3.up * scale.y,
            Vector3.right*scale.x + Vector3.down * scale.y,
            Vector3.right*scale.x + Vector3.up * scale.y  */
        };

        //newMesh.uv = new Vector2[] { Vector2.zero, Vector2.up, Vector2.right, Vector2.one };
        newMesh.uv = new Vector2[] {
            new Vector2(uv.xMin, uv.yMin),
            new Vector2(uv.xMin, uv.yMax),
            new Vector2(uv.xMax, uv.yMin),
            new Vector2(uv.xMax, uv.yMax)
        };

        newMesh.triangles = new int[] { 0, 1, 3, 0, 3, 2 };
        if (null != colors && colors.Length == newMesh.vertices.Length)
            newMesh.colors = colors;

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        filter.mesh = newMesh;

        Mesh mesh = filter.sharedMesh;

        /* Vector3 center = new Vector3(region.center.x * scale.x, region.center.y * scale.y, 0f);
        Vector3 size = new Vector3(region.width * scale.x, region.height * scale.y, 1f);

        Bounds bounds = new Bounds(center, size);
        mesh.bounds = bounds; */

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

}


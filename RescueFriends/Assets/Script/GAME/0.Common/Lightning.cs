using UnityEngine;
using System.Collections;

public class Lightning : NNRecycler {
    public float curvatureFrequency = 7;
    public float curvature = 5;

    public float widthFrequency = 7;
    public float width = 0.8f;

    public int segments = 3;

    public Material material;

    float length = 1;

    MeshFilter mf;
    Mesh mesh1, mesh2;
    Vector3[] vert;

    bool swapFlag;
    float timeOffset;

    void Start()
    {
        if(mesh1 == null) 
            GetComponent<Renderer>().enabled = false;
    }

    void Setup()
    {
        timeOffset = Random.value * 2;
        mf = GetComponent<MeshFilter> ();
        if (null == mf)
        {
            mf = gameObject.AddComponent<MeshFilter> ();
        }

        MeshRenderer mr = GetComponent<MeshRenderer> ();
        if (null == mr)
        {
            mr = gameObject.AddComponent<MeshRenderer> ();
        }
        mr.sharedMaterial = material;

        vert = new Vector3[segments*2-2];
        UpdateLightningVertices();
        mesh1 = BuildBaseMesh(segments);
        mesh2 = BuildBaseMesh(segments);
        mesh1.vertices = mesh2.vertices = vert;
        GetComponent<Renderer>().enabled = true;
    }

    Mesh BuildBaseMesh(int segments)
    {
        int vertCnt = segments*2-2;
        int triCnt = 2*(segments-3)+2;
        Vector2[] uv = new Vector2[vertCnt];
        int[] tri = new int[triCnt*3];

        uv[0] = new Vector2(0,0.5f);
        tri[0] = 0; tri[1] = 1; tri[2] = 2;
        float oneOverSeg = 1.0f/(float)segments;
        for(int s=1; s<segments-2; ++s){
            int baseIdx = (s*2-1);

            uv[baseIdx] = new Vector2(oneOverSeg*s, 1);
            uv[baseIdx+1] = new Vector2(oneOverSeg*s, 0);

            tri[baseIdx*3] = baseIdx;
            tri[baseIdx*3+1] = baseIdx+2;
            tri[baseIdx*3+2] = baseIdx+1;

            tri[baseIdx*3+3] = baseIdx+1;
            tri[baseIdx*3+4] = baseIdx+2;
            tri[baseIdx*3+5] = baseIdx+3;
        }
        uv[vertCnt-3] = new Vector2(oneOverSeg*(segments-1), 1);
        uv[vertCnt-2] = new Vector2(oneOverSeg*(segments-1), 0);
        uv[vertCnt-1] = new Vector2(1, 0.5f);
        tri[triCnt*3-3] = vertCnt-3;
        tri[triCnt*3-2] = vertCnt-1;
        tri[triCnt*3-1] = vertCnt-2;

        Mesh mesh = new Mesh();
        mesh.vertices = vert;
        mesh.uv = uv;
        mesh.triangles = tri;
        return mesh;
    }

    void UpdateLightningVertices()
    {
        float oneOverSeg = 1.0f/(float)(segments-1);
        int vertCnt = segments*2-2;

        vert[0] = Vector3.zero;
        for(int s=1; s<segments-1; ++s){
            int baseIdx = (s*2-1);
            float W = (Mathf.PerlinNoise((Time.time-timeOffset) * widthFrequency, Random.value)-0.5f)*width;
            float C = (Mathf.PerlinNoise((Time.time-timeOffset) * curvatureFrequency, oneOverSeg*s)-0.5f)*curvature*length;
            vert[baseIdx] = new Vector3(oneOverSeg*s, C+W, 0);
            vert[baseIdx+1] = new Vector3(oneOverSeg*s, C-W, 0);
        }
        vert[vertCnt-1] = new Vector3(1, 0, 0);
    }

    void Update()
    {
        if(GetComponent<Renderer>().enabled) {
            UpdateLightningVertices();

            //Procedual mesh double buffering
            //see (http://forum.unity3d.com/threads/118723-Huge-performance-loss-in-Mesh.CreateVBO-for-dynamic-meshes-IOS)
            if(swapFlag) 
            {
                mesh1.vertices = vert;
                mf.sharedMesh = mesh1;
            }
            else {
                mesh2.vertices = vert;
                mf.sharedMesh = mesh2;
            }
            swapFlag = !swapFlag;
        }
    }

    public void EmitLightning(Vector3 from, Vector3 to, float duration)
    {
        Vector3 dist = to - from;
        length = dist.magnitude;
        segments = Mathf.Max((int)(segments*length),4);
        transform.position = from;
        transform.localScale = new Vector3(length, 1, 1);
        transform.rotation = Quaternion.FromToRotation(Vector3.right, dist);
        StartCoroutine(coEmitLighting(duration));
    }

    IEnumerator coEmitLighting(float duration)
    {
        Setup();
        yield return new WaitForSeconds(duration);
        //gameObject.SetActiveRecursively(false);
        //Destroy(gameObject);//.SetActiveRecursively(false);
        NNPool.Abandon(gameObject);
    }

    public override void Reset () {
        base.Reset();
        segments = 3;
    }
}


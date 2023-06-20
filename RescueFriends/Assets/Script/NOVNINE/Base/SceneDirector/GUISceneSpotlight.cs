using UnityEngine;
using System.Collections;
using NOVNINE.Diagnostics;
//using Holoville.HOTween;
using DG.Tweening;

public class GUISceneSpotlight : SingletonMonoBehaviour<GUISceneSpotlight>
{
    static GameObject _spotlight;
    static GameObject _plane;
    static GameObject _spot;
    static GameObject _cam;

    const string kSpotlightTag = "spotlightTransition";
    static float amount;
    
    static void SInit ()
    {
        if (_spotlight == null) {
            GameObject prefab = UnityEngine.Resources.Load("Spotlight") as GameObject;
            _spotlight = GameObject.Instantiate(prefab) as GameObject;
            _spotlight.name = kSpotlightTag;

            _plane = _spotlight.transform.Find("plane").gameObject;
            _spot = _spotlight.transform.Find("spot").gameObject;
            _cam = _spotlight.transform.Find("cam").gameObject;
            _spotlight.transform.localPosition = Vector3.back * 150f;

            UpdateScale();
        }
    }

    public static void Show (float sec, string mode = null)
    {
        SInit();
        _spotlight.SetActive(true);

        if (mode != null && mode.Equals("StarSpotlight")) {
            _spot.GetComponent<MeshFilter>().mesh = GetStarMesh();
        } else if (mode != null && mode.Equals("Spotlight")) {
            _spot.GetComponent<MeshFilter>().mesh = GetCircleMesh();
        } else if (mode != null && mode.Equals("HexagonSpotlight")) {
            _spot.GetComponent<MeshFilter>().mesh = GetHexagonMesh();
            _spot.transform.eulerAngles = Vector3.forward* 30f;
        }

		_spot.transform.DOScale(new Vector3(amount, amount, 1f), sec).From();
    }


    public static void Hide (float sec, string mode = null)
    {
        SInit();
        if (mode != null && mode.Equals("StarSpotlight")) {
            _spot.GetComponent<MeshFilter>().mesh = GetStarMesh();
        } else if (mode != null && mode.Equals("Spotlight")) {
            _spot.GetComponent<MeshFilter>().mesh = GetCircleMesh();
        } else if (mode != null && mode.Equals("HexagonSpotlight")) {
            _spot.GetComponent<MeshFilter>().mesh = GetHexagonMesh();
            _spot.transform.eulerAngles = Vector3.forward* 30f;
        }

		_spot.transform.DOScale(new Vector3(amount, amount, 1f), sec).SetEase(Ease.InQuad).OnComplete(Completed);
    }

    static void Completed() 
    {
        _spotlight.SetActive(false);
        _spot.transform.localScale = Vector3.zero;
    }

    static void UpdateScale () {
        Vector2 screenSize = GetScreenSize();
        amount = Mathf.Sqrt((Mathf.Pow(screenSize.x, 2.0f) + Mathf.Pow(screenSize.y, 2.0f))) * 2.0f;
        _plane.transform.localScale = new Vector3(screenSize.x, screenSize.y, 0.1f);
    }

    static Mesh GetStarMesh () 
    {
        Mesh mesh = new Mesh();
        mesh.name = "Star Mesh";
        
        int numberOfPoints = 10;
        int[] triangles = new int[numberOfPoints * 3];
        float angle = -360.0f / numberOfPoints;
        Vector3[] vertices = new Vector3[numberOfPoints + 1];
        Vector3[] points = new[] { new Vector3(0f, 1f, 0f), new Vector3(0f, 0.5f, 0f)};

        for (int i = 0, v = 1, t = 1; i < 5; i++) {
            for (int j = 0; j < points.Length; j++, v++, t += 3) {
                vertices[v] = Quaternion.Euler(0f, 0f, angle * (v - 1)) * points[j];
                triangles[t] = v;
                triangles[t + 1] = v + 1;
            }
        }

        triangles[triangles.Length - 1] = 1;
        
        mesh.vertices = vertices; 
        mesh.triangles = triangles;

        return mesh;
    }

    static Mesh GetCircleMesh ()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Circle Mesh";
        
        int numberOfPoints = 30;
        int[] triangles = new int[numberOfPoints * 3];
        float angle = -360.0f / numberOfPoints;
        Vector3[] vertices = new Vector3[numberOfPoints + 1];
        Vector3 point = Vector3.up;

        for (int v = 1, t = 1; v < vertices.Length; v++, t += 3) {
            vertices[v] = Quaternion.Euler(0f, 0f, angle * (v - 1)) * point;
            triangles[t] = v;
            triangles[t + 1] = v + 1;
        }

        triangles[triangles.Length - 1] = 1;
        
        mesh.vertices = vertices; 
        mesh.triangles = triangles;

        return mesh;
    }

    static Mesh GetHexagonMesh ()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Hexagon Mesh";
        
        int numberOfPoints = 6;
        int[] triangles = new int[numberOfPoints * 3];
        float angle = -360.0f / numberOfPoints;
        Vector3[] vertices = new Vector3[numberOfPoints + 1];
        Vector3 point = Vector3.up;

        for (int v = 1, t = 1; v < vertices.Length; v++, t += 3) {
            vertices[v] = Quaternion.Euler(0f, 0f, angle * (v - 1)) * point;
            triangles[t] = v;
            triangles[t + 1] = v + 1;
        }

        triangles[triangles.Length - 1] = 1;
        
        mesh.vertices = vertices; 
        mesh.triangles = triangles;

        return mesh;
    }

    static Vector2 GetScreenSize () {
        Camera cam = _cam.GetComponent<Camera>();
        Debugger.Assert(cam != null);

        Vector3 minSize = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 maxSize = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        return new Vector2((maxSize.x - minSize.x),(maxSize.y - minSize.y));
    }
}

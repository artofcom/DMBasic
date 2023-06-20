using UnityEngine;
using System.Collections;

[RequireComponent(typeof(tk2dSprite))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class tk2dSpriteExpand: MonoBehaviour
{

    Mesh _mesh = null;

    Vector3[] _vertices;
    Vector2[] _uvs;

    bool _flipTextureY = false;

    Vector3[] _progressVertices;
    Vector2[] _progressUvs;

    int[] _progressTriangles;
    Color[] _progressColors;

    [System.NonSerialized]
    public tk2dBaseSprite sprite;

    public Rect marginRate = new Rect(0.1f, 0.1f, 0.8f, 0.8f);
    public Rect uvRate = new Rect(0f, 0f, 1f, 1f);

    void Awake()
    {
        sprite = gameObject.GetComponent<tk2dBaseSprite>();

    }

    // Use this for initialization
    void Start()
    {
        Init();

    }

    void Init()
    {
        _mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        _vertices = new Vector3[_mesh.vertices.Length];
        _mesh.vertices.CopyTo(_vertices, 0);

        _uvs = new Vector2[_mesh.uv.Length];
        _mesh.uv.CopyTo(_uvs, 0);

        _progressVertices = new Vector3[16];
        _progressUvs = new Vector2[16];

        _progressTriangles = new int[54];
        _progressColors = new Color[16];

        //try
        {
            tk2dBaseSprite spr = gameObject.GetComponent<tk2dBaseSprite>();
            _flipTextureY = (spr.Collection.spriteDefinitions[spr.spriteId].flipped != tk2dSpriteDefinition.FlipMode.None);
        }
        //catch
        {
            //Debug.LogError("Null reference exception : " + this.gameObject.name);

        }

        Commit();
    }

    public void Commit()
    {
        if (null == _mesh) return;

        //Debug.Log("Rate " + marginRate.ToString() + " flip " + gameObject.GetComponent<tk2dBaseSprite>().spriteDefinition.flipped.ToString());

        Vector3 localScale = transform.localScale;
        if (localScale.x < 0) localScale.x *= -1f;
        if (localScale.y < 0) localScale.y *= -1f;

        _progressVertices[0] = _vertices[0];
        _progressVertices[1] = (_vertices[1] - _vertices[0]) * (0f+marginRate.xMin) / localScale.x + _vertices[0];
        _progressVertices[2] = (_vertices[0] - _vertices[1]) * (1f-marginRate.xMax) / localScale.x + _vertices[1];
        _progressVertices[3] = _vertices[1];

        _progressVertices[4] = (_vertices[2] - _vertices[0]) * (0f+marginRate.yMin) / localScale.y + _vertices[0];
        _progressVertices[8] = (_vertices[0] - _vertices[2]) * (1f-marginRate.yMax) / localScale.y + _vertices[2];

        _progressVertices[12] = _vertices[2];
        _progressVertices[13] = (_vertices[3] - _vertices[2]) * (0f+marginRate.xMin) / localScale.x + _vertices[2];
        _progressVertices[14] = (_vertices[2] - _vertices[3]) * (1f-marginRate.xMax) / localScale.x + _vertices[3];
        _progressVertices[15] = _vertices[3];

        _progressVertices[7] = (_vertices[3] - _vertices[1]) * (0f+marginRate.yMin) / localScale.y + _vertices[1];
        _progressVertices[11] = (_vertices[1] - _vertices[3]) * (1f-marginRate.yMax) / localScale.y + _vertices[3];

        _progressVertices[5] = new Vector3(_progressVertices[1].x, _progressVertices[4].y, 0f);
        _progressVertices[6] = new Vector3(_progressVertices[2].x, _progressVertices[4].y, 0f);
        _progressVertices[9] = new Vector3(_progressVertices[1].x, _progressVertices[8].y, 0f);
        _progressVertices[10] = new Vector3(_progressVertices[2].x, _progressVertices[8].y, 0f);

        if (false == _flipTextureY) {
            Vector2 uv1 = _uvs[1] - _uvs[0];
            _uvs[0] = _uvs[0] + uv1 * uvRate.xMin;
            _uvs[1] = _uvs[0] + uv1 * uvRate.width;

            Vector2 uv2 = _uvs[3] - _uvs[2];
            _uvs[2] = _uvs[2] + uv2 * uvRate.xMin;
            _uvs[3] = _uvs[2] + uv2 * uvRate.width;

            Vector2 uv3 = _uvs[2] - _uvs[0];
            _uvs[0] = _uvs[0] + uv3 * uvRate.yMin;
            _uvs[2] = _uvs[0] + uv3 * uvRate.height;

            Vector2 uv4 = _uvs[3] - _uvs[1];
            _uvs[1] = _uvs[1] + uv4 * uvRate.yMin;
            _uvs[3] = _uvs[1] + uv4 * uvRate.height;
        } else {
            Vector2 uv1 = _uvs[2] - _uvs[0];
            _uvs[0] = _uvs[0] + uv1 * uvRate.yMin;
            _uvs[2] = _uvs[0] + uv1 * uvRate.height;

            Vector2 uv2 = _uvs[3] - _uvs[1];
            _uvs[1] = _uvs[1] + uv2 * uvRate.yMin;
            _uvs[3] = _uvs[1] + uv2 * uvRate.height;

            Vector2 uv3 = _uvs[1] - _uvs[0];
            _uvs[0] = _uvs[0] + uv3 * uvRate.xMin;
            _uvs[1] = _uvs[0] + uv3 * uvRate.width;

            Vector2 uv4 = _uvs[3] - _uvs[2];
            _uvs[2] = _uvs[2] + uv4 * uvRate.xMin;
            _uvs[3] = _uvs[2] + uv4 * uvRate.width;
        }

        if (false == _flipTextureY) {
            _progressUvs[0] = _uvs[0];
            _progressUvs[1] = (_uvs[1] - _uvs[0]) * (0f + marginRate.xMin) + _uvs[0];
            _progressUvs[2] = (_uvs[0] - _uvs[1]) * (1f - marginRate.xMax) + _uvs[1];
            _progressUvs[3] = _uvs[1];

            _progressUvs[4] = (_uvs[2] - _uvs[0]) * (0f + marginRate.yMin) + _uvs[0];
            _progressUvs[8] = (_uvs[0] - _uvs[2]) * (1f - marginRate.yMax) + _uvs[2];

            _progressUvs[12] = _uvs[2];
            _progressUvs[13] = (_uvs[3] - _uvs[2]) * (0f + marginRate.xMin) + _uvs[2];
            _progressUvs[14] = (_uvs[2] - _uvs[3]) * (1f - marginRate.xMax) + _uvs[3];
            _progressUvs[15] = _uvs[3];

            _progressUvs[7] = (_uvs[3] - _uvs[1]) * (0f + marginRate.yMin) + _uvs[1];
            _progressUvs[11] = (_uvs[1] - _uvs[3]) * (1f - marginRate.yMax) + _uvs[3];

            _progressUvs[5] = new Vector2(_progressUvs[1].x, _progressUvs[4].y);
            _progressUvs[6] = new Vector2(_progressUvs[2].x, _progressUvs[4].y);
            _progressUvs[9] = new Vector2(_progressUvs[1].x, _progressUvs[8].y);
            _progressUvs[10] = new Vector2(_progressUvs[2].x, _progressUvs[8].y);
        } else {
            _progressUvs[0] = _uvs[0];
            _progressUvs[4] = (_uvs[2] - _uvs[0]) * (0f + marginRate.yMin) + _uvs[0];
            _progressUvs[8] = (_uvs[0] - _uvs[2]) * (1f - marginRate.yMax) + _uvs[2];
            _progressUvs[12] = _uvs[2];

            _progressUvs[1] = (_uvs[1] - _uvs[0]) * (0f + marginRate.xMin) + _uvs[0];
            _progressUvs[2] = (_uvs[0] - _uvs[1]) * (1f - marginRate.xMax) + _uvs[1];

            _progressUvs[3] = _uvs[1];
            _progressUvs[7] = (_uvs[3] - _uvs[1]) * (0f + marginRate.yMin) + _uvs[1];
            _progressUvs[11] = (_uvs[1] - _uvs[3]) * (1f - marginRate.yMax) + _uvs[3];
            _progressUvs[15] = _uvs[3];

            _progressUvs[13] = (_uvs[3] - _uvs[2]) * (0f + marginRate.xMin) + _uvs[2];
            _progressUvs[14] = (_uvs[2] - _uvs[3]) * (1f - marginRate.xMax) + _uvs[3];

            _progressUvs[5] = new Vector2(_progressUvs[4].x, _progressUvs[1].y);
            _progressUvs[6] = new Vector2(_progressUvs[4].x, _progressUvs[2].y);
            _progressUvs[9] = new Vector2(_progressUvs[8].x, _progressUvs[1].y);
            _progressUvs[10]= new Vector2(_progressUvs[8].x, _progressUvs[2].y);

        }

        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                _progressTriangles[18 * y + 6 * x + 0] = 4 * y + x;
                _progressTriangles[18 * y + 6 * x + 1] = 4 * (y + 1) + x;
                _progressTriangles[18 * y + 6 * x + 2] = 4 * (y + 1) + x + 1;

                _progressTriangles[18 * y + 6 * x + 3] = 4 * (y) + x;
                _progressTriangles[18 * y + 6 * x + 4] = 4 * (y + 1) + x + 1;
                _progressTriangles[18 * y + 6 * x + 5] = 4 * (y) + x + 1;
            }
        }

        if (null != sprite && null != _progressColors) {
            for (int i = 0; i < 16; i++)
                _progressColors[i] = sprite.color;
        }

        /*
        Mesh mesh = new Mesh();
        mesh.vertices = _progressVertices;
        mesh.uv = _progressUvs;

        mesh.colors = _progressColors;
        mesh.triangles = _progressTriangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        _mesh = mesh;
        */

        _mesh.vertices = _progressVertices;
        _mesh.uv = _progressUvs;

        _mesh.colors = _progressColors;
        _mesh.triangles = _progressTriangles;

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
    }

}


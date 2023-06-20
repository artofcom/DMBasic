using UnityEngine;
using System.Collections;

[RequireComponent(typeof(tk2dSprite))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class tk2dSpriteProgress : MonoBehaviour
{

    public enum eSTYLE {
        eLANDSCAPE = 0,
        ePORTRATE
    }

    Mesh _mesh = null;

    Vector3[] _vertices;
    Vector2[] _uvs;

    bool _flipTextureY = false;

    Vector3[] _progressVertices;
    Vector2[] _progressUvs;

    public eSTYLE eStyle = eSTYLE.eLANDSCAPE;

    public float min = 0f;
    public float max = 10f;

    public float value = 10f;

    // Use this for initialization
    void Start ()
    {
        _mesh = gameObject.GetComponent<MeshFilter>().mesh;

        _vertices = new Vector3[_mesh.vertices.Length];
        _mesh.vertices.CopyTo(_vertices, 0);

        _uvs = new Vector2[_mesh.uv.Length];
        _mesh.uv.CopyTo(_uvs, 0);

        _progressVertices = new Vector3[_vertices.Length];
        _progressUvs = new Vector2[_uvs.Length];

        //_flipTextureY = gameObject.GetComponent<tk2dBaseSprite>().spriteDefinition.flipped;
        tk2dBaseSprite spr = gameObject.GetComponent<tk2dBaseSprite>();
        _flipTextureY = (spr.Collection.spriteDefinitions[spr.spriteId].flipped != tk2dSpriteDefinition.FlipMode.None);

        Commit();

    }

    public void Commit()
    {
        if (max == min) return;

        float rate = (value - min) / (max - min);    // min(0) <-> max(1)
        if (rate < 0f) rate = 0f;
        if (rate > 1f) rate = 1f;

        if (null == _mesh) return;

        //Debug.Log("Rate " + rate.ToString() + " flip " + gameObject.GetComponent<tk2dBaseSprite>().spriteDefinition.flipped.ToString());

        _vertices.CopyTo(_progressVertices, 0);

        _uvs.CopyTo(_progressUvs, 0);

        for (int i = 0; i < _vertices.Length / 4; i++) {
            if (eStyle == eSTYLE.eLANDSCAPE) {
                _progressVertices[i * 4 + 1].x = _vertices[i * 4 + 0].x + (_vertices[i * 4 + 1].x - _vertices[i * 4 + 0].x) * rate;
                _progressVertices[i * 4 + 3].x = _vertices[i * 4 + 2].x + (_vertices[i * 4 + 3].x - _vertices[i * 4 + 2].x) * rate;
            } else {
                _progressVertices[i * 4 + 2].y = _vertices[i * 4 + 0].y + (_vertices[i * 4 + 2].y - _vertices[i * 4 + 0].y) * rate;
                _progressVertices[i * 4 + 3].y = _vertices[i * 4 + 1].y + (_vertices[i * 4 + 3].y - _vertices[i * 4 + 1].y) * rate;

            }
        }

        for (int i = 0; i < _uvs.Length / 4; i++) {
            if (eStyle == eSTYLE.eLANDSCAPE) {
                if (false == _flipTextureY) {
                    _progressUvs[i * 4 + 1].x = _uvs[i * 4 + 0].x + (_uvs[i * 4 + 1].x - _uvs[i * 4 + 0].x) * rate;
                    _progressUvs[i * 4 + 3].x = _uvs[i * 4 + 2].x + (_uvs[i * 4 + 3].x - _uvs[i * 4 + 2].x) * rate;
                } else {
                    _progressUvs[i * 4 + 1].y = _uvs[i * 4 + 0].y + (_uvs[i * 4 + 1].y - _uvs[i * 4 + 0].y) * rate;
                    _progressUvs[i * 4 + 3].y = _uvs[i * 4 + 2].y + (_uvs[i * 4 + 3].y - _uvs[i * 4 + 2].y) * rate;
                }
            } else {
                if (false == _flipTextureY) {
                    _progressUvs[i * 4 + 2].y = _uvs[i * 4 + 0].y + (_uvs[i * 4 + 2].y - _uvs[i * 4 + 0].y) * rate;
                    _progressUvs[i * 4 + 3].y = _uvs[i * 4 + 1].y + (_uvs[i * 4 + 3].y - _uvs[i * 4 + 1].y) * rate;
                } else {
                    _progressUvs[i * 4 + 2].x = _uvs[i * 4 + 0].x + (_uvs[i * 4 + 2].x - _uvs[i * 4 + 0].x) * rate;
                    _progressUvs[i * 4 + 3].x = _uvs[i * 4 + 1].x + (_uvs[i * 4 + 3].x - _uvs[i * 4 + 1].x) * rate;
                }
            }
        }

        /*
        Mesh mesh = new Mesh();
        mesh.vertices = _progressVertices;
        mesh.uv = _progressUvs;

        mesh.colors = _mesh.colors;
        mesh.triangles = _mesh.triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        _mesh = mesh;
        */

        _mesh.vertices = _progressVertices;
        _mesh.uv = _progressUvs;

        _mesh.colors = _mesh.colors;
        _mesh.triangles = _mesh.triangles;

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
    }

}


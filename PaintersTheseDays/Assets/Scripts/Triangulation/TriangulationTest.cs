using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Rendering;

public class TriangulationTest : MonoBehaviour
{
    [SerializeField] Material meshMaterial;
    DelaunayTriangulation tr = new DelaunayTriangulation();
    List<Vector2> _vertices = new List<Vector2>();
    List<Triangle> _triangles = new List<Triangle>();
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            _vertices.Add(new Vector2(Random.Range(-10f, 10f), Random.Range(-8f, 8f)));
        }
        _triangles = tr.Triangulate(_vertices);
        foreach(Triangle t in _triangles)
        {
            Debug.Log(t.a + " - " + t.b + " - " + t.c);
        }

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        GetComponent<MeshRenderer>().material = meshMaterial;

        List<Vector3> meshVertices = new List<Vector3>();
        List<Vector2> meshUVs = new List<Vector2>();
        List<int> meshTriangles = new List<int>();
        for (int i = 0; i < _triangles.Count; i++)
        {
            Random.seed = (int)((_triangles[i].a.x + _triangles[i].a.y) * 100000);
            float z = Random.Range(0f, 10f);
            meshVertices.Add(new Vector3(_triangles[i].a.x, _triangles[i].a.y, z));
            Random.seed = (int)((_triangles[i].b.x + _triangles[i].b.y) * 100000);
            z = Random.Range(0f, 10f);
            meshVertices.Add(new Vector3(_triangles[i].b.x, _triangles[i].b.y, z));
            Random.seed = (int)((_triangles[i].c.x + _triangles[i].c.y) * 100000);
            z = Random.Range(0f, 10f);
            meshVertices.Add(new Vector3(_triangles[i].c.x, _triangles[i].c.y, z));
            meshTriangles.Add(i * 3);
            meshTriangles.Add(i * 3 + 1);
            meshTriangles.Add(i * 3 + 2);
            meshUVs.Add(_triangles[i].a);
            meshUVs.Add(_triangles[i].b);
            meshUVs.Add(_triangles[i].c);
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.uv = meshUVs.ToArray();
        mesh.triangles = meshTriangles.ToArray();
    }
    void Update()
    {
        
    }
}

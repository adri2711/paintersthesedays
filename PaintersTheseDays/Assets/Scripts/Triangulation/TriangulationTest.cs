using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class TriangulationTest : MonoBehaviour
{
    [SerializeField] Material[] meshMaterials;
    DelaunayTriangulation tr = new DelaunayTriangulation();
    List<Vertex> _vertices = new List<Vertex>();
    List<Triangle> _triangles = new List<Triangle>();
    List<int[]> tris = new List<int[]>();
    void Start()
    {
        Vector2 canvasLimit = new Vector2(8f, 10f);
        for (int i = 0; i < 100; i++)
        {
            _vertices.Add(new Vertex(i, new Vector2(Random.Range(-canvasLimit.x, canvasLimit.x), Random.Range(-canvasLimit.y, canvasLimit.y))));
        }
        _vertices.Add(new Vertex(_vertices.Count, canvasLimit * new Vector2(1f, 1f)));
        _vertices.Add(new Vertex(_vertices.Count, canvasLimit * new Vector2(-1f, 1f)));
        _vertices.Add(new Vertex(_vertices.Count, canvasLimit * new Vector2(-1f, -1f)));
        _vertices.Add(new Vertex(_vertices.Count, canvasLimit * new Vector2(1f, -1f)));
        _vertices.Add(new Vertex(_vertices.Count, canvasLimit * new Vector2(0f, 1f)));
        _vertices.Add(new Vertex(_vertices.Count, canvasLimit * new Vector2(-1f, 0f)));
        _vertices.Add(new Vertex(_vertices.Count, canvasLimit * new Vector2(0f, -1f)));
        _vertices.Add(new Vertex(_vertices.Count, canvasLimit * new Vector2(1f, 0f)));

        _triangles = tr.Triangulate(_vertices);
        tr.GenerateAdjacency();

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        List<Vector3> meshVertices = new List<Vector3>();
        List<Vector2> meshUVs = new List<Vector2>();
        List<int> meshTriangles = new List<int>();
        for (int i = 0; i < _vertices.Count; i++)
        {
            float z = 0;
            meshVertices.Add(new Vector3(_vertices[i].pos.x, _vertices[i].pos.y, z));
            meshUVs.Add(_vertices[i].pos);
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.uv = meshUVs.ToArray();

        mesh.subMeshCount = _triangles.Count;
        Material[] materials = new Material[_triangles.Count];
        for (int i = 0; i < _triangles.Count; i++)
        {
            tris.Add(new int[3]);
            tris[i][0] = (_triangles[i].a.id);
            tris[i][1] = (_triangles[i].b.id);
            tris[i][2] = (_triangles[i].c.id);
            mesh.SetTriangles(tris[i], i);
            materials[i] = meshMaterials[Random.Range(0, meshMaterials.Length)];
        }
        GetComponent<MeshRenderer>().materials = materials;

        mesh.RecalculateNormals();
    }
    void Update()
    {
        
    }
}

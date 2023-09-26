using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PaintingCanvas : MonoBehaviour
{
    DelaunayTriangulation triangulation = new DelaunayTriangulation();
    List<Vertex> vertices = new List<Vertex>();
    List<Vector3> meshVertices = new List<Vector3>();
    List<Triangle> triangles = new List<Triangle>();
    List<int[]> meshTriangles = new List<int[]>();
    List<Vector2> meshUVs = new List<Vector2>();
    Material[] meshMaterials;
    Material[] materials;

    public float width = 2f;
    public float resolution = 1.61f;
    Vector2 size;
    int subdivisions = 11;
    float vertexVariation = 0.75f;
    private void Start()
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        meshMaterials = Resources.LoadAll<Material>("Materials/Paint");
        GenerateVertices();
        GenerateTriangles();
        GenerateMesh();
    }
    private void OnGUI()
    {
        Event e = Event.current;
        if (!e.isKey || !(e.type == EventType.KeyDown)) return;
        if (e.keyCode == KeyCode.R)
        {
            GenerateVertices();
            GenerateTriangles();
            GenerateMesh();
        }
    }

    public void SetTriangleMaterial(int id, int materialId)
    {
        materials[id] = meshMaterials[materialId];
        GetComponent<MeshRenderer>().materials = materials;
    }

    private void GenerateVertices()
    {
        size = new Vector2(width, width * resolution);
        vertices = new List<Vertex>();
        int subdivisionsY = (int)(subdivisions * size.y / size.x);
        for (int i = 0; i <= subdivisions; i++)
        {
            for (int j = 0; j <= subdivisionsY; j++)
            {
                int id = i * (subdivisionsY + 1) + j;
                Vector2 pos = new Vector2(-size.x + (size.x * 2f / subdivisions) * i, -size.y + (size.y * 2f / subdivisionsY) * j);
                if (i > 0 && i < subdivisions) {
                    //pos += Random.insideUnitCircle * size * new Vector2(vertexVariation / subdivisions, vertexVariation / subdivisionsY);
                    pos.x += Random.Range(-1f, 1f) * size.x * vertexVariation / subdivisions;
                }
                if (j > 0 && j < subdivisionsY)
                {
                    pos.y += Random.Range(-1f, 1f) * size.y * vertexVariation / subdivisionsY;
                }
                vertices.Add(new Vertex(id, pos));
            }
        }
    }

    private void GenerateTriangles()
    {
        triangulation = new DelaunayTriangulation();
        triangles = triangulation.Triangulate(vertices);
    }

    private void GenerateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        for (int i = 0; i < vertices.Count; i++)
        {
            float z = 0;
            meshVertices.Add(new Vector3(vertices[i].pos.x, vertices[i].pos.y, z));
            meshUVs.Add(vertices[i].pos);
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.uv = meshUVs.ToArray();

        mesh.subMeshCount = triangles.Count;
        materials = new Material[triangles.Count];
        for (int i = 0; i < triangles.Count; i++)
        {
            meshTriangles.Add(new int[3]);
            meshTriangles[i][0] = (triangles[i].a.id);
            meshTriangles[i][1] = (triangles[i].b.id);
            meshTriangles[i][2] = (triangles[i].c.id);
            mesh.SetTriangles(meshTriangles[i], i);
            materials[i] = meshMaterials[Random.Range(0, meshMaterials.Length)];
        }
        GetComponent<MeshRenderer>().materials = materials;

        mesh.RecalculateNormals();

        if (GetComponent<MeshCollider>() == null)
        {
            gameObject.AddComponent<MeshCollider>();
        }
    }
}

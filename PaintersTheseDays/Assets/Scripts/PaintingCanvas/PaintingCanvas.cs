using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PaintingCanvas : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    DelaunayTriangulation triangulation = new DelaunayTriangulation();
    List<Vertex> vertices = new List<Vertex>();
    List<Vector3> meshVertices = new List<Vector3>();
    List<Triangle> triangles = new List<Triangle>();
    List<int[]> meshTriangles = new List<int[]>();
    List<Vector2> meshUVs = new List<Vector2>();
    Material[] materials;

    public float width = 2f;
    public float resolution = 1.61f;
    public int subdivisions = 11;
    Vector2 size;
    public float vertexVariation = 0.75f;

    public void Generate(bool first = false)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (first)
        {
            GenerateVertices();
            GenerateTriangles();
        }
        GenerateMesh(first);
    }
    private void OnGUI()
    {
        Event e = Event.current;
        if (!e.isKey || !(e.type == EventType.KeyDown)) return;
        //if (e.keyCode == KeyCode.R)
        //{
        //    GenerateVertices();
        //    GenerateTriangles();
        //    GenerateMesh();
        //}
    }

    public void SetTransparency(float a)
    {
        foreach (Material m in materials)
        {
            Color c = m.color;
            c.a = a;
            m.color = c;
        }
        ApplyMaterials();
    }
    public IEnumerator CanvasTransparencyCoroutine(float init, float target, float t)
    {
        for (int i = 0; i <= 20; i++)
        {
            SetTransparency(Mathf.Lerp(init, target, i / 20f));
            yield return new WaitForSeconds(t / 20f);
        }
    }

    public void SetMaterialToTriangles(int[] id, Material material)
    {
        foreach (int i in id)
        {
            materials[i] = material;
        }
        meshRenderer.materials = materials;
    }

    public void SetTriangleMaterial(int id, Material material)
    {
        float a = materials[id].color.a;
        materials[id] = material;
        materials[id].color = material.color.WithAlpha(a);
    }

    public void ApplyMaterials()
    {
        meshRenderer.materials = materials;
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
                Vector2 pos = new Vector2(-size.x + (size.x * 2f / subdivisions) * i, size.y * 2f / subdivisionsY * j);
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

    private void GenerateMesh(bool first = false)
    {
        Mesh mesh = meshFilter.mesh;

        for (int i = 0; i < vertices.Count; i++)
        {
            float z = 0;
            meshVertices.Add(new Vector3(vertices[i].pos.x, vertices[i].pos.y, z));
            meshUVs.Add(vertices[i].pos);
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.uv = meshUVs.ToArray();

        mesh.subMeshCount = triangles.Count;
        if (materials == null) materials = new Material[triangles.Count];
        for (int i = 0; i < triangles.Count; i++)
        {
            meshTriangles.Add(new int[3]);
            meshTriangles[i][0] = (triangles[i].a.id);
            meshTriangles[i][1] = (triangles[i].b.id);
            meshTriangles[i][2] = (triangles[i].c.id);
            mesh.SetTriangles(meshTriangles[i], i);
            if (first)
            {
                materials[i] = Paint.GenerateCanvasColor().GetMaterial();
            }
        }
        meshRenderer.materials = materials;

        mesh.RecalculateNormals();

        if (GetComponent<MeshCollider>() == null)
        {
            gameObject.AddComponent<MeshCollider>();
        }
    }

    public void LoadPainting(PaintingData paintingData)
    {
        vertices = paintingData.vertices.ToList();
        triangles = paintingData.triangles.ToList();
        materials = paintingData.materials;
    }

    public PaintingData SavePainting() 
    {
        return new PaintingData(vertices, triangles, materials);
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}

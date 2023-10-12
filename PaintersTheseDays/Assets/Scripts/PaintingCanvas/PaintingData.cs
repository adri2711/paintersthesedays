using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PaintingData
{
    public Vertex[] vertices;
    public Triangle[] triangles;
    public Material[] materials;
    public PaintingData(List<Vertex> vertices, List<Triangle> triangles, Material[] materials)
    {
        this.vertices = vertices.ToArray();
        this.triangles = triangles.ToArray();
        this.materials = materials;
    }
}

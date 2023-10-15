using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PaintingData
{
    public Vertex[] vertices;
    public Triangle[] triangles;
    public Material[] materials;
    public int strokeCount;
    public PaintingData(List<Vertex> vertices, List<Triangle> triangles, Material[] materials, int strokeCount)
    {
        this.vertices = vertices.ToArray();
        this.triangles = triangles.ToArray();
        this.materials = materials;
        this.strokeCount = strokeCount;
    }
}

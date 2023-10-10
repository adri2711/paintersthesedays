using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PaintingData
{
    public List<Vertex> vertices;
    public List<Triangle> triangles;
    public Material[] materials;
    public PaintingData(List<Vertex> vertices, List<Triangle> triangles, Material[] materials)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.materials = materials;
    }
}

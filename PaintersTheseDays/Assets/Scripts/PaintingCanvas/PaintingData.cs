using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Loading;
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

    public List<List<int>> GenerateAdjacency()
    {
        List<List<int>> adjacentTriangles = new List<List<int>>();
        for (int i = 0; i < triangles.Length; i++)
        {
            int a = 0;
            adjacentTriangles.Add(new List<int>());
            for (int j = 0; j < triangles.Length && a < 3; j++)
            {
                if (j == i) continue;
                if (triangles[i].IsAdjacent(triangles[j]))
                {
                    a++;
                    adjacentTriangles[i].Add(j);
                }
            }
        }
        return adjacentTriangles;
    }
}

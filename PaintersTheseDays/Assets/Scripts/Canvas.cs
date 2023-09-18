using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas : MonoBehaviour
{
    DelaunayTriangulation triangulation = new DelaunayTriangulation();
    List<Vertex> vertices = new List<Vertex>();
    List<Vector3> meshVertices = new List<Vector3>();
    List<Triangle> triangles = new List<Triangle>();

    Vector2 size = new Vector2(7f, 11.27f);
    int subdivisions = 10;

    private void Start()
    {
        GenerateTriangles();
        GenerateMesh();
    }

    private void GenerateTriangles()
    {

    }

    private void GenerateMesh()
    {
        
    }
}

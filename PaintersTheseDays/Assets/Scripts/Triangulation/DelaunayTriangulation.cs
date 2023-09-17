using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class DelaunayTriangulation : MonoBehaviour
{
    public List<Triangle> _triangles { get; private set; } = new List<Triangle>();
    public List<Edge> _edges { get; private set; } = new List<Edge>();
    public List<Vector2> _vertices { get; private set; } = new List<Vector2>();
    public List<List<int>> _adjcacentTriangles { get; private set; } = new List<List<int>>();

    public List<Triangle> Triangulate(List<Vector2> vertices)
    {
        // Store the vertices locally
        _vertices = vertices;

        // Determinate the super triangle
        float minX = vertices[0].x;
        float minY = vertices[0].y;
        float maxX = minX;
        float maxY = minY;

        for (int i = 0; i < vertices.Count; ++i)
        {
            if (vertices[i].x < minX) minX = vertices[i].x;
            if (vertices[i].y < minY) minY = vertices[i].y;
            if (vertices[i].x > maxX) maxX = vertices[i].x;
            if (vertices[i].y > maxY) maxY = vertices[i].y;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float deltaMax = Mathf.Max(dx, dy);
        float midx = (minX + maxX) / 2;
        float midy = (minY + maxY) / 2;

        Vector2 p1 = new Vector2(midx - 20 * deltaMax, midy - deltaMax);
        Vector2 p2 = new Vector2(midx, midy + 20 * deltaMax);
        Vector2 p3 = new Vector2(midx + 20 * deltaMax, midy - deltaMax);

        // Create a list of triangles, and add the supertriangle in it
        _triangles.Add(new Triangle(p1, p2, p3));

        foreach (Vector2 p in vertices)
        {
            List<Edge> polygon = new List<Edge>();
            foreach (Triangle t in _triangles)
            {
                if (t.CircumcircleContains(p))
                {
                    t.isBad = true;
                    polygon.Add(new Edge(t.a, t.b));
                    polygon.Add(new Edge(t.b, t.c));
                    polygon.Add(new Edge(t.c, t.a));
                }
            }

            _triangles.RemoveAll(t => t.isBad);

            for (int i = 0; i < polygon.Count; ++i)
            {
                for (int j = i + 1; j < polygon.Count; ++j)
                {
                    if (polygon[i] == polygon[j])
                    {
                        polygon[i].isBad = true;
                        polygon[j].isBad = true;
                    }
                }
            }

            polygon.RemoveAll(e =>
            {
                if (!e.isBad) _triangles.Add(new Triangle(e.v, e.w, p));
                return e.isBad;
            });
        }

        _triangles.RemoveAll(t =>
        {
            bool r = t.ContainsVertex(p1) || t.ContainsVertex(p2) || t.ContainsVertex(p3);
            if (!r)
            {
                _edges.Add(new Edge(t.a, t.b));
                _edges.Add(new Edge(t.b, t.c));
                _edges.Add(new Edge(t.c, t.a));
            }
            return r;
        });

        return _triangles;
    }
}

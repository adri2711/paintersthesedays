using JetBrains.Annotations;
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
    public List<Vertex> _vertices { get; private set; } = new List<Vertex>();
    public List<List<int>> _adjcacentTriangles { get; private set; } = new List<List<int>>();

    public List<Triangle> Triangulate(List<Vertex> vertices)
    {
        // Store the vertices locally
        _vertices = vertices;

        // Determinate the super triangle
        float minX = vertices[0].pos.x;
        float minY = vertices[0].pos.y;
        float maxX = minX;
        float maxY = minY;

        for (int i = 0; i < vertices.Count; ++i)
        {
            if (vertices[i].pos.x < minX) minX = vertices[i].pos.x;
            if (vertices[i].pos.y < minY) minY = vertices[i].pos.y;
            if (vertices[i].pos.x > maxX) maxX = vertices[i].pos.x;
            if (vertices[i].pos.y > maxY) maxY = vertices[i].pos.y;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float deltaMax = Mathf.Max(dx, dy);
        float midx = (minX + maxX) / 2;
        float midy = (minY + maxY) / 2;

        Vertex p1 = new Vertex(-1, new Vector2(midx - 20 * deltaMax, midy - deltaMax));
        Vertex p2 = new Vertex(-1, new Vector2(midx, midy + 20 * deltaMax));
        Vertex p3 = new Vertex(-1, new Vector2(midx + 20 * deltaMax, midy - deltaMax));

        // Create a list of triangles, and add the supertriangle in it
        _triangles.Add(new Triangle(p1, p2, p3));

        foreach (Vertex p in _vertices)
        {
            List<Edge> polygon = new List<Edge>();
            foreach (Triangle t in _triangles)
            {
                if (t.CircumcircleContains(p.pos))
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
    public List<List<int>> GenerateAdjacency()
    {
        for (int i = 0; i < _triangles.Count; i++)
        {
            int a = 0;
            _adjcacentTriangles.Add(new List<int>());
            for (int j = 0; j < _triangles.Count && a < 3; j++)
            {
                if (_triangles[i].IsAdjacent(_triangles[j]))
                {
                    a++;
                    _adjcacentTriangles[i].Add(j);
                }
            }
        }
        return _adjcacentTriangles;
    }
}

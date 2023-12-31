using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    public Vertex a { get; private set; }
    public Vertex b { get; private set; }
    public Vertex c { get; private set; }
    public bool isBad = false;
    private Vector2 circumcircleCenter;
    private bool calculateCenter = false;

    public Triangle(Vertex a, Vertex b, Vertex c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }

    public static bool operator ==(Triangle a, Triangle b)
    {
        return (a.a == b.a || a.a == b.b || a.a == b.c) &&
               (a.b == b.a || a.b == b.b || a.b == b.c) &&
               (a.c == b.a || a.c == b.b || a.c == b.c);
    }

    public static bool operator !=(Triangle a, Triangle b)
    {
        return !(a == b);
    }
    
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public bool ContainsVertex(Vertex v)
    {
        return a == v || b == v || c == v;
    }

    public bool IsAdjacent(Triangle t)
    {
        return ContainsVertex(t.a) && ContainsVertex(t.b) ||
               ContainsVertex(t.b) && ContainsVertex(t.c) ||
               ContainsVertex(t.c) && ContainsVertex(t.a);
    }

    public bool CircumcircleContains(Vector2 v)
    {
        Vector2 circum = GetCircumcircleCenter();
        float circum_radius = (circum - a.pos).sqrMagnitude;
        float dist = (circum - v).sqrMagnitude;
        return dist <= circum_radius;
    }

    public Vector2 GetCircumcircleCenter()
    {
        if (!calculateCenter)
        {
            float ab = a.pos.sqrMagnitude;
            float cd = b.pos.sqrMagnitude;
            float ef = c.pos.sqrMagnitude;

            float ax = a.pos.x;
            float ay = a.pos.y;
            float bx = b.pos.x;
            float by = b.pos.y;
            float cx = c.pos.x;
            float cy = c.pos.y;

            float circum_x = (ab * (cy - by) + cd * (ay - cy) + ef * (by - ay)) / (ax * (cy - by) + bx * (ay - cy) + cx * (by - ay));
            float circum_y = (ab * (cx - bx) + cd * (ax - cx) + ef * (bx - ax)) / (ay * (cx - bx) + by * (ax - cx) + cy * (bx - ax));

            circumcircleCenter = new Vector2(circum_x / 2, circum_y / 2);
            calculateCenter = true;
        }
        return circumcircleCenter;
    }
}

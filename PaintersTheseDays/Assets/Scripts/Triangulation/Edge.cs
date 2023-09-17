using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public Vertex v { get; private set; }
    public Vertex w  { get; private set; }
    public bool isBad;

    public Edge(Vertex v, Vertex w)
    {
        this.v = v;
        this.w = w;
    }

    public static bool operator ==(Edge a, Edge b)
    {
        return (a.v == b.v && a.w == b.w) ||
               (a.v == b.w && a.w == b.v);
    }

    public static bool operator !=(Edge a, Edge b)
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
}

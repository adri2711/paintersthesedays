using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public Vector2 pos;
    public int id = -1;
    public Vertex(int id, Vector2 pos)
    {
        this.id = id;
        this.pos = pos;
    }
}

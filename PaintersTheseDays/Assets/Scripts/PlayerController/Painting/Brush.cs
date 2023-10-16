using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Brush
{
    public Paint paint = new Paint(Color.black);
    public int density = 3;
    public float dispersion = 4f;
    public Brush(Color color, int density = 3, float dispersion = 4f)
    {
        paint = new Paint(color);
        this.density = density;
        this.dispersion = dispersion;
    }
    public Material GetMaterial()
    {
        return paint.GetMaterial();
    }
}

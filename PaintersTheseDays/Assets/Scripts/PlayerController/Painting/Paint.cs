using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paint
{
    Material material;
    Color color;
    
    public static Paint GenerateCanvasColor()
    {
        Color c = new Color(.824f, .761f, .596f, 1f);
        float h, s, v;
        Color.RGBToHSV(c, out h, out s, out v);
        s = Random.Range(.1f, .2f);
        v += Random.Range(-.075f, .075f);
        c = Color.HSVToRGB(h, s, v);
        return new Paint(c);
    }
    public static Paint CombinePaint(Paint p1, Paint p2, float w1 = .5f)
    {
        return new Paint(CombineColors(p1.color, p2.color, w1));
    }
    public static Color CombineColors(Color c1, Color c2, float w1 = .5f)
    {
        return c1 * w1 + c2 * (1f - w1);
    }
    public Paint(Color color)
    {
        this.color = color;
    }
    public Material GetMaterial()
    {
        if (material == null)
        {
            material = new Material(PaintingController.defaultShader);
            material.color = color;
        }
        return material;
    }
    public Color GetColor()
    {
        return color;
    }
    public void SetColor(Color newColor)
    {
        material = null;
        color = newColor;
    }
}

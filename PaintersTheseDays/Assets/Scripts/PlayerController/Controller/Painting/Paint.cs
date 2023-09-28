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
    public static Paint CombinePaint(Paint p1, Paint p2)
    {
        return new Paint(CombineColors(p1.GetColor(), p2.GetColor()));
    }
    private static Color CombineColors(Color c1, Color c2)
    {
        Color newColor;
        newColor = new Color((c1.r + c2.r) / 2f, (c1.g + c2.g) / 2f, (c1.b + c2.b) / 2f, (c1.a + c2.a) / 2f);
        return newColor;
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

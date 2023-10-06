using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paint
{
    bool _primary = false;
    Material _material;
    Color _color;
    
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
        return new Paint(CombineColors(p1._color, p2._color, w1));
    }
    public static Color CombineColors(Color c1, Color c2, float w1 = .5f)
    {
        return c1 * w1 + c2 * (1f - w1);
    }
    public Paint(Color color, bool primary = false)
    {
        this._color = color;
        this._primary = primary;
    }
    public Material GetMaterial()
    {
        if (_material == null)
        {
            _material = new Material(PaintingController.defaultShader);
            _material.color = _color;
        }
        return _material;
    }
    public Color GetColor()
    {
        return _color;
    }
    public bool IsPrimary()
    {
        return _primary;
    }
    public void SetColor(Color newColor)
    {
        _material = null;
        _color = newColor;
    }
}

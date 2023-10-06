using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Palette
{
    public List<Paint> paints = new List<Paint>();
    public Paint mixerPaint;

    public void AddPaintToMix(Paint p)
    {
        if (mixerPaint == null)
        {
            mixerPaint = p;
        }
        else
        {
            mixerPaint = Paint.CombinePaint(mixerPaint, p);
        }
    }
    public void ConfirmMix()
    {
        paints.Add(mixerPaint);
    }
    public void ClearMix()
    {
        mixerPaint = null;
    }

    public void AddPaint(Color c)
    {
        paints.Add(new Paint(c));
    }
    public void SetPaints(Color[] colors)
    {
        paints = new List<Paint>();
        foreach(Color c in colors)
        {
            paints.Add(new Paint(c));
        }
    }
}

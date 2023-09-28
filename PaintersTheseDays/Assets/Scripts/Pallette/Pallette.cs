using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pallette
{
    List<Paint> paints = new List<Paint>();
    int selectedPaint;
    Paint mixerPaint;

    public void AddPaintToMix(float w)
    {
        if (mixerPaint == null)
        {
            mixerPaint = paints[selectedPaint];
        }
        else
        {
            mixerPaint = Paint.CombinePaint(mixerPaint, paints[selectedPaint], w);
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
    public void SelectPaint(int id)
    {
        selectedPaint = id;
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

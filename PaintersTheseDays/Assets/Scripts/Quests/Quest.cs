using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public Paint[] paints;
    public Brush[] brushes;
    public PaintingData incompletePainting;
    public bool hasIncompletePainting = false;
}

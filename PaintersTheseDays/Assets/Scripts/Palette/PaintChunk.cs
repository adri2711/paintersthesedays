using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintChunk : MonoBehaviour
{
    public Paint paint;
    public bool isMix = false;
    public int timesMixed = 0;
    private MeshRenderer _meshRenderer;
    public void Setup(Paint p, bool isMix = false)
    {
        paint = p;
        this.isMix = isMix;
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = paint.GetMaterial();
    }
    public void UpdateMaterial()
    {
        _meshRenderer.material = paint.GetMaterial();
    }
}

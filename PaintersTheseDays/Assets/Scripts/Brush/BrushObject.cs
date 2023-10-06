using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class BrushObject : MonoBehaviour
{
    private GameObject _characterSignalsInterfaceTarget;
    private ICharacterSignals _characterSignals;
    private FirstPersonController _firstPersonController;
    private PaintingController _paintingController;
    private Camera _camera;
    private MeshRenderer _model;
    private Material brushTipMaterial;
    bool _active = false;

    void Start()
    {
        _characterSignalsInterfaceTarget = transform.parent.parent.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _firstPersonController = _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>();
        _paintingController = GetComponentInParent<PaintingController>();
        _camera = GetComponentInParent<Camera>();
        _model = transform.Find("BrushModel").GetComponent<MeshRenderer>();
        _model.enabled = false;

        foreach (Material material in _model.materials)
        {
            if (material.name == "Not Fixed Brush Hair (Instance)")
            {
                brushTipMaterial = material;
            }
        }

        _characterSignals.PlacedCanvas.Subscribe(w =>
        {
            Activate();
        }).AddTo(this);
        _characterSignals.ExitedCanvas.Subscribe(w =>
        {
            Deactivate();
        }).AddTo(this);
    }
    public void SetPaint(Paint p)
    {
        brushTipMaterial.EnableKeyword("_EMISSION");
        brushTipMaterial.SetColor("_EmissionColor", p.GetColor());
        brushTipMaterial.color = p.GetColor();
    }
    public void ClearPaint()
    {
        brushTipMaterial.DisableKeyword("_EMISSION");
    }
    private void Activate()
    {
        _active = true;
        _model.enabled = true;
        _model.GetComponent<Animator>().Play("Show");
    }
    private void Deactivate()
    {
        _active = false;
        StartCoroutine(DeactivateCoroutine());
    }
    private IEnumerator DeactivateCoroutine()
    {
        _model.GetComponent<Animator>().Play("Hide");
        yield return new WaitForSeconds(1f);
        _model.enabled = _active;
    }
}

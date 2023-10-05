using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PalletteObject : MonoBehaviour
{
    private GameObject _characterSignalsInterfaceTarget;
    private ICharacterSignals _characterSignals;
    private FirstPersonController _firstPersonController;
    private Camera _camera;
    private Pallette _pallette = new Pallette();
    private MeshRenderer _model;
    private List<PaintChunk> _paintChunks = new List<PaintChunk>();

    void Start()
    {
        _characterSignalsInterfaceTarget = transform.parent.parent.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _firstPersonController = _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>();
        _camera = GetComponentInParent<Camera>();
        _model = transform.Find("PaletteModel").GetComponent<MeshRenderer>();
        _model.enabled = false;

        _characterSignals.PlacedCanvas.Subscribe(w =>
        {
            Activate();
        }).AddTo(this);
        _characterSignals.ExitedCanvas.Subscribe(w =>
        {
            Deactivate();
        }).AddTo(this);
    }

    void Update()
    {
        if (_firstPersonController.currentActiveCanvas != null)
        {
            if (Input.GetMouseButton(0))
            {
                ClickChunk();
            }
            if (Input.GetMouseButton(1))
            {
                ClickChunk(true);
            }
        }
    }
    private void ClickChunk(bool small = false)
    {
        RaycastHit hit;
        if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit)) return;
        PaintChunk paintChunk = hit.collider.GetComponent<PaintChunk>();
        if (paintChunk == null) return;

    }
    private void Activate()
    {
        _pallette = new Pallette();
        _pallette.SetPaints(new Color[] { Color.cyan, Color.white, Color.magenta, Color.black });
        _model.enabled = true;
        _model.GetComponent<Animator>().Play("Show");
    }
    private void Deactivate()
    {
        StartCoroutine(DeactivateCoroutine());
    }
    private IEnumerator DeactivateCoroutine()
    {
        _model.GetComponent<Animator>().Play("Hide");
        yield return new WaitForSeconds(1f);
        _model.enabled = false;
    }
}

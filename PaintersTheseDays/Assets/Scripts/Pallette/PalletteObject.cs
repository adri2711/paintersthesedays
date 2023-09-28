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
    private List<PaintChunk> _paintChunks = new List<PaintChunk>();

    void Start()
    {
        _characterSignalsInterfaceTarget = transform.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _firstPersonController = _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>();
        _camera = _characterSignalsInterfaceTarget.GetComponent<Camera>();

        _characterSignals.PlacedCanvas.Subscribe(w =>
        {
            Activate();
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
        //make pallette visual
    }
}

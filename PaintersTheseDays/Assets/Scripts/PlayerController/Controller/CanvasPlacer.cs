using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class CanvasPlacer : MonoBehaviour
{
    [Header("References")]
    private GameObject _characterSignalsInterfaceTarget;
    private ICharacterSignals _characterSignals;
    private GameObject _paintingCanvasPrefab;

    [SerializeField] float canvasPlacementDistance = 3f;

    private void Awake()
    {
        _characterSignalsInterfaceTarget = transform.parent.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _paintingCanvasPrefab = Resources.Load("Prefab/PaintingCanvas") as GameObject;
    }
    private void Start()
    {
        _characterSignals.PlacedCanvas.Subscribe(w =>
        {
            Place();
        }).AddTo(this);
    }
    private void Place()
    {
        PaintingCanvas paintingCanvasObject = Instantiate(_paintingCanvasPrefab, FindCanvasPosition(), transform.rotation).GetComponent<PaintingCanvas>();
        
    }
    private Vector3 FindCanvasPosition()
    {
        return transform.position + transform.forward * canvasPlacementDistance;
    }
}

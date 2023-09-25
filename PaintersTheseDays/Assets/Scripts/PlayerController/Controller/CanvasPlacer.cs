using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.VisualScripting;
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
        Quaternion rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        PaintingCanvas paintingCanvasObject = Instantiate(_paintingCanvasPrefab, transform.position, rotation).GetComponent<PaintingCanvas>();

        RaycastHit hit;
        Vector3 raycastPos;
        float thickness = .5f;
        Vector3 canvasHalfExtents = new Vector3(paintingCanvasObject.width / 2f, paintingCanvasObject.width * paintingCanvasObject.resolution / 2f, thickness / 2f);
        
        if (Physics.BoxCast(transform.position, canvasHalfExtents, transform.forward, out hit, rotation, canvasPlacementDistance))
        {
            raycastPos = transform.position + transform.forward * (hit.distance - thickness);
        }
        else
        {
            raycastPos = transform.position + transform.forward * canvasPlacementDistance;
        }

        if (Physics.BoxCast(raycastPos, canvasHalfExtents, transform.TransformDirection(Vector3.down), out hit, rotation, 15f))
        {
            paintingCanvasObject.transform.position = new Vector3(hit.point.x, hit.point.y + paintingCanvasObject.width * paintingCanvasObject.resolution, hit.point.z);
        }

        _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>().canPlaceCanvas = false;
        _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>().canMove = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CanvasPlacer : MonoBehaviour
{
    private GameObject _characterSignalsInterfaceTarget;
    private ICharacterSignals _characterSignals;
    private GameObject _paintingCanvasPrefab;
    private FirstPersonController _firstPersonController;

    [SerializeField] float canvasPlacementDistance = 3f;
    [SerializeField] float canvasEditingDistance = 5f;

    private void Awake()
    {
        _characterSignalsInterfaceTarget = transform.parent.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _paintingCanvasPrefab = Resources.Load("Prefab/PaintingCanvas") as GameObject;
        _firstPersonController = _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>();
    }

    private void Start()
    {
        _characterSignals.PlacedCanvas.Subscribe(w =>
        {
            Place();
        }).AddTo(this);
        _characterSignals.EditedCanvas.Subscribe(w =>
        {
            Edit();
        }).AddTo(this);
        _characterSignals.RemovedCanvas.Subscribe(w =>
        {
            Remove();
        }).AddTo(this);
    }

    private void Place()
    {
        Quaternion rotation;
        rotation = Quaternion.Euler(15f, transform.rotation.eulerAngles.y, 0f);
        PaintingCanvas paintingCanvasObject = Instantiate(_paintingCanvasPrefab, transform.position, rotation).GetComponent<PaintingCanvas>();

        QuestPoint questPoint = null;
        foreach (QuestPoint p in FindObjectsByType<QuestPoint>(FindObjectsSortMode.None))
        {
            if (p.questActive)
            {
                questPoint = p;
            }
        }

        if (questPoint != null)
        {
            paintingCanvasObject.transform.position = questPoint.quest.position;
            paintingCanvasObject.transform.rotation = Quaternion.Euler(0f, questPoint.quest.yRotation, 0f);

            if (questPoint.quest.hasIncompletePainting)
            {
                paintingCanvasObject.LoadPainting(questPoint.quest.incompletePainting);
            }
            paintingCanvasObject.Generate();
        }
        else
        {
            float thickness = .5f;
            Vector3 canvasHalfExtents = new Vector3(paintingCanvasObject.width / 2f, paintingCanvasObject.width * paintingCanvasObject.resolution / 2f, thickness / 2f);

            Vector3 raycastPos;
            RaycastHit hit;

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
                paintingCanvasObject.transform.position = new Vector3(hit.point.x, hit.point.y + 1f, hit.point.z);
            }

            bool generate = FirstPersonController.paintingSave == null;
            if (!generate)
            {
                paintingCanvasObject.LoadPainting(FirstPersonController.paintingSave);
            }
            paintingCanvasObject.Generate(generate);
        }

        
        _firstPersonController.EnableCanvasMode(paintingCanvasObject);
    }
    private void Edit()
    {
        PaintingCanvas colliderCanvas = CheckIfClickingCanvas();
        if (colliderCanvas != null)
        {
            _firstPersonController.EnableCanvasMode(colliderCanvas);
        }
    }

    private void Remove()
    {
        PaintingCanvas colliderCanvas = CheckIfClickingCanvas();
        if (colliderCanvas != null)
        {
            if (_firstPersonController.currentActiveCanvas != null)
            {
                _firstPersonController.RemoveCanvas();
            }
            else
            {
                FirstPersonController.paintingSave = colliderCanvas.SavePainting();
                colliderCanvas.Remove();
                _firstPersonController.DisableCanvasMode();
                _firstPersonController.canPlaceCanvas = true;
            }
        }
    }


    private PaintingCanvas CheckIfClickingCanvas()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, canvasEditingDistance))
        {
            PaintingCanvas colliderCanvas = hit.collider.GetComponent<PaintingCanvas>();
            if (colliderCanvas != null)
            {
                return colliderCanvas;
            }
        }
        return null;
    }
}

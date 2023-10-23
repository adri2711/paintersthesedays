using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    FirstPersonController _player;
    ICharacterSignals _characterSignals;
    CharacterController _controller;
    Camera _camera;
    PaintingCanvas paintingCanvas;
    Canvas _movingUICanvas;
    Canvas _paintingUICanvas;
    GameObject _removeEdit;
    GameObject _cursor;
    GameObject _lean;
    GameObject _paintingCursor;
    GameObject _paintingCursorMix;
    GameObject _paintingCursorConfirm;
    GameObject _paintingCursorErase;
    GameObject _paintingCursorEraseMode;

    bool _checkForNearbyCanvas = false;
    [SerializeField] private float _removeEditPromptDistance = 8f;
    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<FirstPersonController>();
        _characterSignals = _player.GetComponent<ICharacterSignals>();
        _controller = _player.GetComponent<CharacterController>();
        _camera = Camera.main;
        _movingUICanvas = transform.Find("MovingUICanvas").GetComponent<Canvas>();
        _paintingUICanvas = transform.Find("PaintingUICanvas").GetComponent<Canvas>();
        _cursor = _movingUICanvas.transform.Find("cursor").gameObject;
        _removeEdit = _cursor.transform.Find("removeedit").gameObject;
        _lean = _paintingUICanvas.transform.Find("lean").gameObject;
        _paintingCursor = _paintingUICanvas.transform.Find("cursor").gameObject;
        _paintingCursorMix = _paintingCursor.transform.Find("mix").gameObject;
        _paintingCursorConfirm = _paintingCursor.transform.Find("confirm").gameObject;
        _paintingCursorErase = _paintingCursor.transform.Find("erase").gameObject;
        _paintingCursorEraseMode = _paintingCursor.transform.Find("erasemode").gameObject;

        ShowMoving();
        HidePainting();
        HideRemoveEdit();

        _characterSignals.PlacedCanvas.Subscribe(w =>
        {
            PlacedCanvas();
        }).AddTo(this);
        _characterSignals.EditedCanvas.Subscribe(w =>
        {
            EditedCanvas();
        }).AddTo(this);
        _characterSignals.RemovedCanvas.Subscribe(w =>
        {
            RemovedCanvas();
        }).AddTo(this);
        _characterSignals.ExitedCanvas.Subscribe(w =>
        {
            ExitedCanvas();
        }).AddTo(this);
        _characterSignals.Moved.Subscribe(w =>
        {
            Moved();
        }).AddTo(this);
    }
    private void Update()
    {
        if (_checkForNearbyCanvas)
        {
            if (paintingCanvas != null && Vector3.Distance(_player.transform.position + _player.transform.forward * _removeEditPromptDistance, paintingCanvas.transform.position) < _removeEditPromptDistance)
            {
                ShowRemoveEdit();
            }
            else
            {
                HideRemoveEdit();
            }
        }

        if (_paintingCursor.activeSelf)
        {
            UpdatePaintingCursor(Input.mousePosition);
        }
    }
    private void UpdatePaintingCursor(Vector2 pos)
    {
        RaycastHit hit;
        if (!Physics.Raycast(_camera.ScreenPointToRay(pos), out hit)) return;
        PaintChunk paintChunk = hit.collider.GetComponent<PaintChunk>();
        PaletteGlass paletteGlass = hit.collider.GetComponentInParent<PaletteGlass>();
        if (paintChunk != null)
        {
            if (PaletteObject.erase)
            {
                if (paintChunk.paint.IsPrimary())
                {
                    ShowPaintingCursorMix();
                }
                else
                {
                    ShowPaintingCursorErase();
                }
            }
            else
            {
                if (paintChunk.isMix)
                {
                    ShowPaintingCursorConfirm();
                }
                else
                {
                    ShowPaintingCursorMix();
                }
            }
        }
        else if (paletteGlass != null)
        {
            ShowPaintingCursorEraseMode();
        }
        else ClearPaintingCursor();

        _paintingCursor.transform.position = pos;
    }
    private void ClearPaintingCursor()
    {
        _paintingCursorConfirm.SetActive(false);
        _paintingCursorMix.SetActive(false);
        _paintingCursorErase.SetActive(false);
        _paintingCursorEraseMode.SetActive(false);
    }
    private void ShowPaintingCursorMix()
    {
        ClearPaintingCursor();
        _paintingCursorMix.SetActive(true);
    }
    private void ShowPaintingCursorErase()
    {
        ClearPaintingCursor();
        _paintingCursorErase.SetActive(true);
    }
    private void ShowPaintingCursorEraseMode()
    {
        ClearPaintingCursor();
        _paintingCursorEraseMode.SetActive(true);
    }
    private void ShowPaintingCursorConfirm()
    {
        ClearPaintingCursor();
        _paintingCursorConfirm.SetActive(true);
    }
    private void PlacedCanvas()
    {
        if (_player.currentActiveCanvas == null) return;
        HideMoving();
        ShowPainting();
    }
    private void EditedCanvas()
    {
        if (_player.currentActiveCanvas == null) return;
        _checkForNearbyCanvas = false;
        HideRemoveEdit();
        HideMoving();
        ShowPainting();
    }
    private void RemovedCanvas()
    {
        _checkForNearbyCanvas = false;
        HideRemoveEdit();
    }
    private void ExitedCanvas()
    {
        HidePainting();
        ShowMoving();
        _checkForNearbyCanvas = true;
        paintingCanvas = _player.currentActiveCanvas;
    }
    private void Moved()
    {
        if (_player.canLean && _controller.isGrounded && (_controller.velocity.x != 0f || _controller.velocity.z != 0f))
        {
            HideLean();
        }
    }

    private void ShowLean()
    {
        _lean.SetActive(true);
    }
    private void HideLean()
    {
        _lean.SetActive(false);
    }
    private void ShowRemoveEdit()
    {
        if (_removeEdit.activeSelf) return;
        _removeEdit.SetActive(true);
    }
    private void HideRemoveEdit()
    {
        if (!_removeEdit.activeSelf) return;
        _removeEdit.SetActive(false);
    }
    private void ShowMoving()
    {
        _movingUICanvas.gameObject.SetActive(true);
    }
    private void HideMoving()
    {
        _movingUICanvas.gameObject.SetActive(false);
    }
    private void ShowPainting()
    {
        _paintingUICanvas.gameObject.SetActive(true);
        ShowLean();
    }
    private void HidePainting()
    {
        _paintingUICanvas.gameObject.SetActive(false);
    }
}

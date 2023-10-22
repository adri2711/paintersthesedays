using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    FirstPersonController _player;
    ICharacterSignals _characterSignals;
    PaintingCanvas paintingCanvas;
    Canvas _movingUICanvas;
    Canvas _paintingUICanvas;
    GameObject _removeEdit;
    GameObject _cursor;

    bool _checkForNearbyCanvas = false;
    [SerializeField] private float _removeEditPromptDistance = 8f;
    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<FirstPersonController>();
        _characterSignals = _player.GetComponent<ICharacterSignals>();
        _movingUICanvas = transform.Find("MovingUICanvas").GetComponent<Canvas>();
        _paintingUICanvas = transform.Find("PaintingUICanvas").GetComponent<Canvas>();
        _cursor = _movingUICanvas.transform.Find("cursor").gameObject;
        _removeEdit = _cursor.transform.Find("removeedit").gameObject;
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
    }
    private void Update()
    {
        if (_checkForNearbyCanvas)
        {
            if (Vector3.Distance(_player.transform.position + _player.transform.forward * _removeEditPromptDistance, paintingCanvas.transform.position) < _removeEditPromptDistance)
            {
                ShowRemoveEdit();
            }
            else
            {
                HideRemoveEdit();
            }
        }
    }
    private void PlacedCanvas()
    {
    }
    private void EditedCanvas()
    {
        _checkForNearbyCanvas = false;
        HideRemoveEdit();
    }
    private void RemovedCanvas()
    {
        _checkForNearbyCanvas = false;
        HideRemoveEdit();
    }
    private void ExitedCanvas()
    {
        _checkForNearbyCanvas = true;
        paintingCanvas = _player.currentActiveCanvas;
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
}

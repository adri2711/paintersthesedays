using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class FovEffects : MonoBehaviour
{
    [Header("References")]
    private GameObject _characterSignalsInterfaceTarget;
    private ICharacterSignals _characterSignals;
    private Camera _camera;

    [Header("Configuration")]
    [SerializeField] private float fovChangeSpeed = 3f;
    [SerializeField] private float runningFovIncrease = 7f;

    public float defaultFov = 90f;
    private float _currentFov = 90f;
    private float fovSmoothstepT = 0f;
    private bool _sprinting = false;

    private void Awake()
    {
        _characterSignalsInterfaceTarget = transform.parent.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _camera = GetComponent<Camera>();
    }
    private void Start()
    {
        _currentFov = defaultFov;

        _characterSignals.Moved.Subscribe(w =>
        {
            if (_characterSignals.IsRunning.Value) {
                StartSprint();
            }
            else
            {
                StopSprint();
            }
        }).AddTo(this);

        _characterSignals.Stop.Subscribe(w =>
        {
            StopSprint();
        }).AddTo(this);

        _characterSignals.Landed.Subscribe(w =>
        {
            StopSprint();
        }).AddTo(this);
    }
    private void StartSprint()
    {
        if (!_sprinting)
        {
            ModifyFov(runningFovIncrease);
        }
        _sprinting = true;
    }
    private void StopSprint()
    {
        if (_sprinting)
        {
            ModifyFov(-runningFovIncrease);
            if (_currentFov < defaultFov) _currentFov = defaultFov;
        }
        _sprinting = false;
    }
    private void ModifyFov(float amount)
    {
        _currentFov += amount;
        fovSmoothstepT = 0f;
    }
    private void Update()
    {
        _camera.fieldOfView = Mathf.SmoothStep(_camera.fieldOfView, _currentFov, fovSmoothstepT);
        fovSmoothstepT = Mathf.Min(1f, fovSmoothstepT + Time.deltaTime * fovChangeSpeed);
    }
}

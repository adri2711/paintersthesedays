using JetBrains.Annotations;
using System.Collections;
using UniRx;
using UnityEngine;

public class HeadLean : MonoBehaviour
{
    [Header("References")]
    private GameObject _characterSignalsInterfaceTarget;
    private ICharacterSignals _characterSignals;
    private CharacterController _characterController;
    private Camera _camera;

    [Header("Configuration")]
    [SerializeField] private float leanSpeed = 2f;

    private Vector3 _initialCameraPosition;

    private void Awake()
    {
        _characterSignalsInterfaceTarget = transform.parent.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _characterController = _characterSignalsInterfaceTarget.GetComponent<CharacterController>();
        _camera = GetComponent<Camera>();
        _initialCameraPosition = _camera.transform.localPosition;
    }

    private void Start()
    {
        _characterSignals.Moved
            .Where(w => _characterController.GetComponent<FirstPersonController>().canLean && _characterController.isGrounded)
            .Subscribe(w =>
            {
                float r = (Vector3.Dot(w, -1 * _characterSignalsInterfaceTarget.transform.right));
                _camera.transform.rotation = Quaternion.Euler(_camera.transform.rotation.eulerAngles + new Vector3(0f, 0f, (leanSpeed * r)));
            }).AddTo(this);
        _characterSignals.ExitedCanvas
            .Subscribe(w =>
            {
                StartCoroutine(ResetTilt(0.1f));
            }).AddTo(this);
    }
    private IEnumerator ResetTilt(float t, int it = 100)
    {
        float r = _camera.transform.rotation.eulerAngles.z;
        for (int i = 0; i < it; i++)
        {
            Vector3 e = _camera.transform.rotation.eulerAngles;
            e.z = Mathf.LerpAngle(r, 0f, (float)i / it);
            _camera.transform.rotation = Quaternion.Euler(e);
            yield return new WaitForSeconds(t / it);
        }
    }
}
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static FirstPersonController;

public class FirstPersonController : MonoBehaviour, ICharacterSignals
{
    #region Character Signals

    public IObservable<Vector3> Moved => _moved;
    private Subject<Vector3> _moved;

    public ReactiveProperty<bool> IsRunning => _isRunning;
    private ReactiveProperty<bool> _isRunning;

    public IObservable<Unit> Stop => _stop;
    private Subject<Unit> _stop;

    public IObservable<Unit> Landed => _landed;
    private Subject<Unit> _landed;

    public IObservable<Unit> Jumped => _jumped;
    private Subject<Unit> _jumped;

    public IObservable<Unit> Stepped => _stepped;
    private Subject<Unit> _stepped;

    public IObservable<Unit> PlacedCanvas => _placedCanvas;
    private Subject<Unit> _placedCanvas;
    public IObservable<Unit> EnteredCanvas => _enteredCanvas;
    private Subject<Unit> _enteredCanvas;

    public IObservable<Unit> ExitedCanvas => _exitedCanvas;
    private Subject<Unit> _exitedCanvas;

    public IObservable<Unit> RemovedCanvas => _removedCanvas;
    private Subject<Unit> _removedCanvas;

    public IObservable<Unit> EditedCanvas => _editedCanvas;
    private Subject<Unit> _editedCanvas;

    public IObservable<Unit> MadeCanvasTransparent => _madeCanvasTransparent;
    private Subject<Unit> _madeCanvasTransparent;

    #endregion

    #region Configuration

    [Header("References")]
    private PlayerControllerInput _playerControllerInput;
    private CharacterController _characterController;
    private Camera _camera;

    [Header("Locomotion Properties")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float runSpeed = 17f;
    [SerializeField] private int jumps = 1;
    [SerializeField] private float jumpForceMagnitude = 10f;
    [SerializeField] private float jumpCooldown = .5f;
    [SerializeField] private float jumpCoyoteTime = .2f;
    [SerializeField] private float strideLength = 6f;
    [SerializeField] private float gravity = -9.81f;
    public float StrideLength => strideLength;
    [SerializeField] private float stickToGroundForceMagnitude = 5f;

    [Header("Look Properties")]
    [SerializeField] private float cameraSpeed = 5f;
    [Range(-90, 0)][SerializeField] private float minViewAngle = -60f;
    [Range(0, 90)][SerializeField] private float maxViewAngle = 60f;

    [Header("Canvas Properties")]
    [SerializeField] private float adjustToCanvasDuration = 0.5f;
    [SerializeField] private float distanceFromCanvas = 1.5f;
    [SerializeField] private float leanSpeed = 7f;
    [SerializeField] private float leanLeniency = 10f;
    [SerializeField] private float leanSnap = 1f;
    [SerializeField] private float canvasEnableTransparencyDuration = .4f;
    [SerializeField] private float canvasDisableTransparencyDuration = .2f;
    #endregion

    [NonSerialized] public bool canLean = false;
    [NonSerialized] public bool canMove = true;
    [NonSerialized] public bool canMoveCamera = true;
    [NonSerialized] public bool canPlaceCanvas = true;
    [NonSerialized] public bool canEditCanvas = true;
    [NonSerialized] public bool transparentCanvas = false;

    public static PaintingData paintingSave;

    private int _jumpsRemaining = 1;
    private bool _jumpPressed = false;
    private float _jumpT = 0;
    private float _jumpCoyoteT = 0;
    private float _placeT = 0;
    private float _transparentT = 0;

    public PaintingCanvas currentActiveCanvas { get; private set; }

    private void Awake()
    {
        _playerControllerInput = GetComponent<PlayerControllerInput>();
        _characterController = GetComponent<CharacterController>();
        _camera = GetComponentInChildren<Camera>();

        _isRunning = new ReactiveProperty<bool>(false);
        _moved = new Subject<Vector3>().AddTo(this);
        _stop = new Subject<Unit>().AddTo(this);
        _jumped = new Subject<Unit>().AddTo(this);
        _landed = new Subject<Unit>().AddTo(this);
        _stepped = new Subject<Unit>().AddTo(this);
        _placedCanvas = new Subject<Unit>().AddTo(this);
        _enteredCanvas = new Subject<Unit>().AddTo(this);
        _exitedCanvas = new Subject<Unit>().AddTo(this);
        _removedCanvas = new Subject<Unit>().AddTo(this);
        _editedCanvas = new Subject<Unit>().AddTo(this);
        _madeCanvasTransparent = new Subject<Unit>().AddTo(this);
    }
    private void Start()
    {
        HandleCanvasInteraction();
        HandleMovement();
        HandleSteppedEvents();
        HandleLook();
    }

    private void Update()
    {
        _jumpT = Mathf.Max(_jumpT - Time.deltaTime, 0f);
        _jumpPressed = Input.GetKey(KeyCode.Space);
        if (_characterController.isGrounded)
        {
            _jumpCoyoteT = jumpCoyoteTime;
        }
        else
        {
            _jumpCoyoteT = MathF.Max(_jumpCoyoteT - Time.deltaTime, 0f);
        }

        _placeT = Mathf.Max(_placeT - Time.deltaTime, 0f);
        _transparentT = Mathf.Max(_placeT - Time.deltaTime, 0f);
    }
    public void AdjustCameraToCanvas(float t)
    {
        if (currentActiveCanvas == null) return;
        StartCoroutine(MoveToPosition(GetCanvasPlayerAnchor(), t));
    }

    private IEnumerator MoveToPosition(Vector3 pos, float t)
    {
        canLean = false;
        Vector3 dist = pos - transform.position;
        float speed = dist.magnitude / t;

        float d = (currentActiveCanvas.transform.position + new Vector3(0f, currentActiveCanvas.width * currentActiveCanvas.resolution * 0.5f, 0f) - _camera.transform.position).magnitude;
        float h = currentActiveCanvas.transform.position.y + currentActiveCanvas.width * currentActiveCanvas.resolution - _camera.transform.position.y;
        Vector3 startRotation = new Vector3(_camera.transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0f);
        Vector3 canvasRotation = currentActiveCanvas.transform.rotation.eulerAngles + new Vector3(-0f, 0f, 0f);

        for (float f = 0f; f <= t; f += Time.deltaTime)
        {
            Vector3 movement = dist.normalized * speed * Time.deltaTime;
            _characterController.Move(new Vector3(movement.x, 0f, movement.z));

            float smoothCurve = Mathf.SmoothStep(0f, 1f, f / t);
            float xRotation = Mathf.LerpAngle(startRotation.x, canvasRotation.x, smoothCurve);
            float yRotation = Mathf.LerpAngle(startRotation.y, canvasRotation.y, smoothCurve);
            transform.localRotation = Quaternion.Euler(new Vector3(0f, yRotation, 0f));
            _camera.transform.localRotation = Quaternion.Euler(new Vector3(xRotation, 0f, 0f));

            yield return new WaitForEndOfFrame();
        }
        canLean = true;
    }

    private Vector3 GetCanvasPlayerAnchor()
    {
        if (currentActiveCanvas == null) return Vector3.zero;
        Vector3 b = currentActiveCanvas.transform.forward * -1;
        return currentActiveCanvas.transform.position + b * distanceFromCanvas;
    }

    public void EnableCanvasMode(PaintingCanvas canvas)
    {
        if (canvas == null) return;
        _enteredCanvas.OnNext(Unit.Default);
        canEditCanvas = false;
        canPlaceCanvas = false;
        canMove = false;
        canMoveCamera = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        currentActiveCanvas = canvas;
        AdjustCameraToCanvas(adjustToCanvasDuration);
    }
    public void DisableCanvasMode()
    {
        DisableCanvasTransparency();
        if (currentActiveCanvas != null)
        {
            paintingSave = currentActiveCanvas.SavePainting();
        }
        canEditCanvas = true;
        canMove = true;
        canLean = false;
        canMoveCamera = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        currentActiveCanvas = null;
    }
    public void RemoveCanvas()
    {
        _exitedCanvas.OnNext(Unit.Default);
        currentActiveCanvas.Remove();
        DisableCanvasMode();
        canPlaceCanvas = true;
    }
    public void EnableCanvasTransparency()
    {
        if (!transparentCanvas) StartCoroutine(currentActiveCanvas.CanvasTransparencyCoroutine(1f, .5f, canvasEnableTransparencyDuration));
        transparentCanvas = true;
    }
    public void DisableCanvasTransparency()
    {
        if (transparentCanvas) StartCoroutine(currentActiveCanvas.CanvasTransparencyCoroutine(.5f, 1f, canvasDisableTransparencyDuration));
        transparentCanvas = false;
    }
    private void HandleCanvasInteraction()
    {
        var placeCanvasLatch = LatchObservables.Latch(this.UpdateAsObservable(), _playerControllerInput.PlaceCanvas, false);

        _playerControllerInput.PlaceCanvas
            .Zip(placeCanvasLatch, (m, j) => new Unit())
            .Where(i => _placeT == 0 && _characterController.isGrounded)
            .Subscribe(i =>
            {
                _placeT = adjustToCanvasDuration;
                if (canPlaceCanvas)
                {
                    _placedCanvas.OnNext(Unit.Default);
                }
                else if (canEditCanvas)
                {
                    _editedCanvas.OnNext(Unit.Default);
                }
                else if (currentActiveCanvas != null)
                {
                    _exitedCanvas.OnNext(Unit.Default);
                    DisableCanvasMode();
                }
            }).AddTo(this);

        _playerControllerInput.RemoveCanvas
            .Zip(placeCanvasLatch, (m, j) => new Unit())
            .Subscribe(i =>
            {
                _removedCanvas.OnNext(Unit.Default);
            }).AddTo(this);

        _playerControllerInput.MakeCanvasTransparent
            .Zip(placeCanvasLatch, (m, j) => new Unit())
            .Where(i => _transparentT == 0)
            .Subscribe(i =>
            {
                if (canLean)
                {
                    _transparentT = canvasEnableTransparencyDuration;
                    if (transparentCanvas)
                    {
                        DisableCanvasTransparency();
                    }
                    else
                    {
                        EnableCanvasTransparency();
                    }
                    _madeCanvasTransparent.OnNext(Unit.Default);
                }
            }).AddTo(this);
    }
    private void HandleMovement()
    {
        _characterController.Move(-stickToGroundForceMagnitude * transform.up);

        var jumpLatch = LatchObservables.Latch(this.UpdateAsObservable(), _playerControllerInput.Jump, false);
        var stopLatch = LatchObservables.Latch(this.UpdateAsObservable(), _playerControllerInput.Stop, false);

        _playerControllerInput.Stop
            .Zip(stopLatch, (m, j) => new Unit())
            .Subscribe(i =>
            {
                if (canLean && (GetCanvasPlayerAnchor() - transform.position).magnitude < leanSnap)
                {
                    AdjustCameraToCanvas(adjustToCanvasDuration / 2f);
                }
                _stop.OnNext(Unit.Default);
            }).AddTo(this);

        _playerControllerInput.Move
            .Zip(jumpLatch, (m, j) => new MoveInputData(m, j))
            .Where(moveInputData => _jumpPressed || moveInputData.Move != Vector2.zero || _characterController.isGrounded == false)
            .Subscribe(i =>
            {
                //// Vertical Movement ////
                bool wasGrounded = _characterController.isGrounded;
                float verticalVelocity = 0f;

                // jump while grounded
                if (canMove && _jumpPressed && _jumpT == 0 && _jumpCoyoteT > 0)
                {
                    RaycastHit hit;
                    if (Physics.SphereCast(transform.position, 0.6f, Vector3.down, out hit, 10f) && hit.normal.y > 0.8f)
                    {
                        verticalVelocity = jumpForceMagnitude;
                        _jumped.OnNext(Unit.Default);
                        _jumpPressed = false;
                        _jumpT = jumpCooldown;
                        _jumpsRemaining--;
                    }
                }
                //mid-air jump following jump
                else if (i.Jump && _jumpT == 0 && _jumpsRemaining > 0 && _jumpsRemaining < jumps)
                {
                    verticalVelocity = jumpForceMagnitude * 1.5f;
                    _jumped.OnNext(Unit.Default);
                    _jumpT = jumpCooldown;
                    _jumpsRemaining--;
                }
                //jump mid-air without having jumped previously
                else if (i.Jump && _jumpT == 0 && _jumpsRemaining == jumps && jumps > 1)
                {
                    verticalVelocity = jumpForceMagnitude * 1.5f;
                    _jumped.OnNext(Unit.Default);
                    _jumpT = jumpCooldown;
                    _jumpsRemaining -= 2;
                }
                // mid-air
                else if (!wasGrounded)
                {
                    verticalVelocity = _characterController.velocity.y + gravity * Time.deltaTime * 3.0f;
                }
                // grounded
                else
                {
                    verticalVelocity = -Mathf.Abs(stickToGroundForceMagnitude);
                }

                Vector2 horizontalVelocity = new Vector2();
                if (canMove)
                {
                    //// Horizontal Movement ////
                    var currentSpeed = _playerControllerInput.Run.Value ? runSpeed : walkSpeed;
                    horizontalVelocity = i.Move * currentSpeed; //Calculate velocity (direction * speed).
                }
                else if (canLean && currentActiveCanvas != null)
                {
                    if (i.Move.x == 0 && i.Move.y < 0)
                    {
                        //Step back
                        _exitedCanvas.OnNext(Unit.Default);
                        DisableCanvasMode();
                    }
                    else
                    {
                        // Leaning
                        Vector2 p = new Vector2(transform.position.x, transform.position.z);
                        Vector3 c3d = GetCanvasPlayerAnchor();
                        Vector2 c = new Vector2(c3d.x, c3d.z);
                        Vector2 pcn = (p - c).normalized;
                        Vector2 pcna = Vector2Rotate.rotate(pcn, transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
                        Vector2 m = new Vector2(i.Move.x, 0f);
                        float d = (p - c).magnitude;
                        float a = (-leanLeniency * d);
                        float v = leanSpeed + Mathf.Min(0f, a * Vector2.Dot(m, pcna));
                        horizontalVelocity = m * v;
                    }
                }

                // Apply
                var characterVelocity = transform.TransformVector(new Vector3(
                    horizontalVelocity.x,
                    verticalVelocity,
                    horizontalVelocity.y));
                var motion = characterVelocity * Time.deltaTime;
                _characterController.Move(motion);

                //land
                if (!wasGrounded && _characterController.isGrounded)
                {
                    // The character was airborne at the beginning, but grounded at the end of this frame.
                    _jumpsRemaining = jumps;
                }

                HandleCharacterOutputSignals(wasGrounded, _characterController.isGrounded);

            }).AddTo(this);
    }

    private void HandleCharacterOutputSignals(bool wasGrounded, bool isGrounded)
    {
        _isRunning.Value = _characterController.velocity.magnitude > 0 && _playerControllerInput.Run.Value;
        _moved.OnNext(_characterController.velocity * Time.deltaTime);
        if (!wasGrounded && isGrounded)
        {
            _landed.OnNext(Unit.Default);
        }
    }
    private void HandleSteppedEvents()
    {
        // Emit stepped events:
        var stepDistance = 0f;
        Moved.Subscribe(w =>
        {
            stepDistance += w.magnitude;

            if (stepDistance > strideLength)
            {
                _stepped.OnNext(Unit.Default);
            }

            stepDistance %= strideLength;
        }).AddTo(this);
    }
    private void HandleLook()
    {
        _playerControllerInput.Look
        .Where(v => v != Vector2.zero)
        .Subscribe(inputLook =>
        {
            if (canMoveCamera)
            {
                // Horizontal look with rotation around the vertical axis, where + means clockwise.
                var horizontalLook = inputLook.x * Vector3.up * Time.deltaTime * cameraSpeed;
                transform.localRotation *= Quaternion.Euler(horizontalLook);

                // Vertical look with rotation around the horizontal axis, where + means upwards.
                var verticalLook = inputLook.y * Vector3.left * Time.deltaTime * cameraSpeed;
                var newQ = _camera.transform.localRotation * Quaternion.Euler(verticalLook);

                _camera.transform.localRotation = RotationTools.ClampRotationAroundXAxis(newQ, -maxViewAngle, -minViewAngle);
            }
            else
            {
                //var horizontalLook = inputLook.x * Vector3.up * Time.deltaTime * cameraSpeed;
                //var newQX = transform.localRotation * Quaternion.Euler(horizontalLook);
                //transform.localRotation = RotationTools.ClampRotationAroundYAxis(newQX, -45f, 45f);
            
                //var verticalLook = inputLook.y * Vector3.left * Time.deltaTime * cameraSpeed;
                //var newQ = _camera.transform.localRotation * Quaternion.Euler(verticalLook);

                //_camera.transform.localRotation = RotationTools.ClampRotationAroundXAxis(newQ, -45f, 45f);
            }
        }).AddTo(this);
    }
    public struct MoveInputData
    {
        public readonly Vector2 Move;
        public readonly bool Jump;

        public MoveInputData(Vector2 move, bool jump)
        {
            Move = move;
            Jump = jump;
        }
    }
}
